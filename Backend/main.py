import json
import os
from pathlib import Path
from typing import AsyncIterator

from anthropic import AsyncAnthropic
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import StreamingResponse
from pydantic import BaseModel

app = FastAPI(title="AetherThrone — Navigator's Character Engine")
client = AsyncAnthropic()

MEMORY_DIR = Path("memory")
MEMORY_DIR.mkdir(exist_ok=True)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


# ── Request / Response models ────────────────────────────────────────────────

class CharacterRequest(BaseModel):
    characterId: str
    characterName: str
    systemPrompt: str
    memoryContext: str
    playerMessage: str
    bond: float
    stage: str


class CharacterResponse(BaseModel):
    reply: str
    bondDelta: float


# ── Memory persistence ───────────────────────────────────────────────────────

def _memory_path(character_id: str) -> Path:
    return MEMORY_DIR / f"{character_id}.json"


def load_memory(character_id: str) -> list[dict]:
    path = _memory_path(character_id)
    if not path.exists():
        return []
    return json.loads(path.read_text())


def append_memory(character_id: str, player_msg: str, reply: str) -> None:
    path = _memory_path(character_id)
    history = load_memory(character_id)
    history.append({"role": "user", "content": player_msg})
    history.append({"role": "assistant", "content": reply})
    # Keep last 40 turns (80 messages) to stay within context limits
    if len(history) > 80:
        history = history[-80:]
    path.write_text(json.dumps(history, ensure_ascii=False, indent=2))


# ── Bond classifier ──────────────────────────────────────────────────────────

async def classify_bond_delta(reply: str, current_bond: float, stage: str) -> float:
    """Ask Claude to rate the emotional valence of the reply on -10..+10."""
    prompt = (
        f"You are a game system scoring emotional valence for a relationship mechanic.\n"
        f"Current bond: {current_bond:.0f}/100. Stage: {stage}.\n"
        f"Character reply: \"{reply}\"\n\n"
        f"Output a single JSON object with one key \"delta\" (float, range -10 to +10). "
        f"Positive = reply builds trust/warmth. Negative = reply is cold/hostile/hurtful. "
        f"0 = neutral. No explanation, just JSON."
    )
    response = await client.messages.create(
        model="claude-haiku-4-5-20251001",
        max_tokens=20,
        messages=[{"role": "user", "content": prompt}],
    )
    try:
        data = json.loads(response.content[0].text.strip())
        delta = float(data.get("delta", 0.0))
        return max(-10.0, min(10.0, delta))
    except (json.JSONDecodeError, KeyError, ValueError):
        return 0.0


# ── Endpoints ────────────────────────────────────────────────────────────────

@app.post("/character/respond", response_model=CharacterResponse)
async def character_respond(req: CharacterRequest):
    """Non-streaming endpoint — returns full reply + bondDelta."""
    history = load_memory(req.characterId)
    messages = history + [
        {
            "role": "user",
            "content": f"MEMORY CONTEXT:\n{req.memoryContext}\n\nNavigator says: \"{req.playerMessage}\""
        }
    ]

    response = await client.messages.create(
        model="claude-sonnet-4-6",
        max_tokens=300,
        system=req.systemPrompt,
        messages=messages,
    )

    reply = response.content[0].text.strip()
    bond_delta = await classify_bond_delta(reply, req.bond, req.stage)
    append_memory(req.characterId, req.playerMessage, reply)

    return CharacterResponse(reply=reply, bondDelta=bond_delta)


@app.post("/character/stream")
async def character_stream(req: CharacterRequest):
    """SSE streaming endpoint — yields text chunks then a final JSON metadata line."""

    history = load_memory(req.characterId)
    messages = history + [
        {
            "role": "user",
            "content": f"MEMORY CONTEXT:\n{req.memoryContext}\n\nNavigator says: \"{req.playerMessage}\""
        }
    ]

    async def generate() -> AsyncIterator[str]:
        full_reply = []

        async with client.messages.stream(
            model="claude-sonnet-4-6",
            max_tokens=300,
            system=req.systemPrompt,
            messages=messages,
        ) as stream:
            async for text in stream.text_stream:
                full_reply.append(text)
                yield f"data: {json.dumps({'type': 'chunk', 'text': text})}\n\n"

        reply = "".join(full_reply).strip()
        bond_delta = await classify_bond_delta(reply, req.bond, req.stage)
        append_memory(req.characterId, req.playerMessage, reply)

        yield f"data: {json.dumps({'type': 'done', 'reply': reply, 'bondDelta': bond_delta})}\n\n"

    return StreamingResponse(generate(), media_type="text/event-stream")


@app.delete("/character/{character_id}/memory")
async def clear_memory(character_id: str):
    path = _memory_path(character_id)
    if path.exists():
        path.unlink()
    return {"status": "cleared", "characterId": character_id}


@app.get("/health")
async def health():
    return {"status": "alive"}
