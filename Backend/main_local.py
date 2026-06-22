import json
import os
from pathlib import Path
from typing import AsyncIterator
import random
import re

from dotenv import load_dotenv
load_dotenv()

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import StreamingResponse
from pydantic import BaseModel

app = FastAPI(title="AetherThrone — Navigator's Local Character Engine")

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


# ── Local bond classifier (keyword-based) ────────────────────────────────────

def classify_bond_delta(reply: str, current_bond: float, stage: str) -> float:
    """Local keyword-based bond delta classifier as replacement for Claude Haiku."""
    positive_keywords = [
        'happy', 'glad', 'good', 'great', 'wonderful', 'amazing', 'pleased', 
        'delighted', 'thrilled', 'excited', 'thank', 'appreciate', 'love', 
        'care', 'trust', 'friend', 'ally', 'understand', 'support', 'help',
        'accept', 'welcome', 'comfort', 'reassure', 'hope', 'optimistic'
    ]
    
    negative_keywords = [
        'angry', 'hate', 'terrible', 'awful', 'horrible', 'disgusted', 
        'annoyed', 'frustrated', 'mad', 'upset', 'worried', 'concerned', 
        'fear', 'scared', 'disappointed', 'sorry', 'apologize', 'doubt',
        'refuse', 'reject', 'ignore', 'avoid', 'danger', 'threat', 'hurt'
    ]
    
    reply_lower = reply.lower()
    
    positive_count = sum(1 for word in positive_keywords if word in reply_lower)
    negative_count = sum(1 for word in negative_keywords if word in reply_lower)
    
    # Calculate base score
    base_score = (positive_count - negative_count) * 1.5
    
    # Factor in current bond level (higher bond makes positive replies more positive)
    bond_factor = 1.0 + (current_bond / 100.0) * 0.5
    adjusted_score = base_score * bond_factor
    
    # Apply stage-based modifiers
    stage_multiplier = {
        "Stranger": 0.7,
        "Acquaintance": 0.8,
        "Ally": 1.0,
        "Friend": 1.2,
        "Intimate": 1.4,
        "Devoted": 1.6
    }.get(stage, 1.0)
    
    final_score = adjusted_score * stage_multiplier
    
    # Clamp to reasonable bounds
    return max(-10.0, min(10.0, final_score))


# ── Local LLM Simulation ─────────────────────────────────────────────────────

def generate_local_response(system_prompt: str, memory_context: str, player_message: str) -> str:
    """
    Generate a response using a local simulation instead of calling Claude API.
    This simulates responses based on character traits and context.
    """
    # Extract character info from system prompt
    char_match = re.search(r"You are (\w+) in the game AETHER THRONE", system_prompt)
    character_name = char_match.group(1) if char_match else "Character"
    
    # Extract personality traits
    personality_match = re.search(r"PERSONALITY:\n(.+?)\n\n", system_prompt, re.DOTALL)
    personality = personality_match.group(1).strip() if personality_match else "Friendly and helpful"
    
    # Extract speech style
    speech_match = re.search(r"SPEECH STYLE:\n(.+?)\n\n", system_prompt, re.DOTALL)
    speech_style = speech_match.group(1).strip() if speech_match else "Natural conversation"
    
    # Combine context for response generation
    context = f"""
    Character: {character_name}
    Personality: {personality}
    Speech Style: {speech_style}
    Memory Context: {memory_context}
    Player Message: {player_message}
    """
    
    # Generate response based on context and personality
    response_templates = [
        # Positive responses for friendly characters
        f"I appreciate you reaching out, Navigator. {character_name} considers the situation carefully.",
        f"That's an interesting point you've raised. {character_name} nods thoughtfully.",
        f"Your words resonate with me, given our shared experiences.",
        f"The galaxy is complex, but I'm glad we can discuss these matters together.",
        f"Your leadership shows wisdom, and I'm here to assist in whatever way I can.",
        f"That reminds me of our previous conversations about similar situations.",
        f"I understand your perspective, and I believe we can find a path forward.",
        f"Thank you for sharing that with me - it means a lot coming from you.",
        f"Your concerns are valid, and I'd like to help address them properly.",
        f"As someone who values our relationship, I want to be honest with you."
    ]
    
    # Negative responses for less friendly characters
    negative_templates = [
        f"I'm not sure I agree with your assessment of the situation.",
        f"That's... not quite how I see things from my perspective.",
        f"I have reservations about the direction you're suggesting.",
        f"Our past experiences make me cautious about that approach.",
        f"While I understand your position, I must respectfully disagree.",
        f"That's easier said than done, given the complexities involved.",
        f"I hope you understand that not all decisions are simple ones.",
        f"This matter requires more consideration than you might realize.",
        f"Perhaps we should examine the implications more carefully.",
        f"I'm not convinced that's the best course of action."
    ]
    
    # Neutral responses
    neutral_templates = [
        f"That's a fair observation about the current state of affairs.",
        f"I'll take your input under advisement and consider the implications.",
        f"The situation is indeed complex, as you've noted.",
        f"I acknowledge your concerns and will factor them into my thinking.",
        f"That's one way to look at the circumstances we find ourselves in.",
        f"I appreciate the insight, though I may have a different perspective.",
        f"The galaxy rarely offers simple solutions to complex problems.",
        f"Your point is well taken, though the reality may be more nuanced.",
        f"I recognize the validity of what you're saying.",
        f"That's certainly worth considering in our strategic planning."
    ]
    
    # Choose template based on personality keywords
    if "hostile" in personality.lower() or "cold" in personality.lower() or "distant" in personality.lower():
        templates = negative_templates
    elif "warm" in personality.lower() or "friendly" in personality.lower() or "loyal" in personality.lower():
        templates = response_templates
    else:
        # Mix based on random chance and context
        if random.random() > 0.5:
            templates = response_templates
        else:
            templates = neutral_templates
    
    # Select a template and add some variation
    base_response = random.choice(templates)
    
    # Add some personality-specific variations
    if "formal" in speech_style.lower() or "respectful" in speech_style.lower():
        base_response = f"With respect, {base_response}"
    elif "casual" in speech_style.lower() or "informal" in speech_style.lower():
        base_response = f"Hey, {base_response}"
    
    # Add more context-specific elements
    if "strategy" in context.lower() or "tactical" in context.lower():
        base_response += " The tactical implications are significant."
    elif "relationship" in context.lower() or "bond" in context.lower():
        base_response += " Our relationship continues to evolve."
    elif "war" in context.lower() or "battle" in context.lower():
        base_response += " The threat of conflict looms large."
    elif "trade" in context.lower() or "economy" in context.lower():
        base_response += " Economic factors must be considered."
    
    # Ensure response ends with proper punctuation
    if not base_response.endswith(('.', '!', '?')):
        base_response += '.'
    
    return base_response


# ── Endpoints ────────────────────────────────────────────────────────────────

@app.post("/character/respond", response_model=CharacterResponse)
async def character_respond(req: CharacterRequest):
    """Non-streaming endpoint — returns full reply + bondDelta."""
    try:
        reply = generate_local_response(req.systemPrompt, req.memoryContext, req.playerMessage)
        bond_delta = classify_bond_delta(reply, req.bond, req.stage)
        append_memory(req.characterId, req.playerMessage, reply)
        
        return CharacterResponse(reply=reply, bondDelta=bond_delta)
    except Exception as e:
        return CharacterResponse(reply=f"I'm having trouble responding right now: {str(e)}", bondDelta=0.0)


@app.post("/character/stream")
async def character_stream(req: CharacterRequest):
    """SSE streaming endpoint — yields text chunks then a final JSON metadata line."""

    async def generate() -> AsyncIterator[str]:
        try:
            # Simulate streaming by breaking response into chunks
            full_reply = generate_local_response(req.systemPrompt, req.memoryContext, req.playerMessage)
            
            # Break reply into chunks to simulate streaming
            words = full_reply.split()
            full_text = ""
            
            for i, word in enumerate(words):
                chunk = word + (" " if i < len(words) - 1 else "")
                full_text += chunk
                
                yield f"data: {json.dumps({'type': 'chunk', 'text': chunk})}\n\n"
                
                # Add a small delay to simulate streaming
                import asyncio
                await asyncio.sleep(0.02)
            
            # Calculate and append bond delta
            bond_delta = classify_bond_delta(full_text, req.bond, req.stage)
            append_memory(req.characterId, req.playerMessage, full_text)

            yield f"data: {json.dumps({'type': 'done', 'reply': full_text, 'bondDelta': bond_delta})}\n\n"
        except Exception as e:
            yield f"data: {json.dumps({'type': 'done', 'reply': f'Error generating response: {str(e)}', 'bondDelta': 0.0})}\n\n"

    return StreamingResponse(generate(), media_type="text/event-stream")


@app.delete("/character/{character_id}/memory")
async def clear_memory(character_id: str):
    path = _memory_path(character_id)
    if path.exists():
        path.unlink()
    return {"status": "cleared", "characterId": character_id}


@app.get("/health")
async def health():
    return {"status": "alive", "engine": "local"}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main_local:app", host="127.0.0.1", port=8000, reload=True)