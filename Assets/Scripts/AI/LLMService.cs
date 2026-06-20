using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// Talks to the FastAPI backend via SSE streaming (/character/stream).
// Falls back to the non-streaming endpoint on parse failure.
public class LLMService : MonoBehaviour
{
    public static LLMService Instance { get; private set; }

    [SerializeField] private string _backendUrl = "http://localhost:8000";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // onChunk  — called for each streamed text fragment (optional, for live display)
    // onDone   — called once with the full reply when streaming completes
    // onError  — called on network or parse failure
    public void GetCharacterResponse(
        Character character,
        string playerMessage,
        Action<string> onDone,
        Action<string> onChunk = null,
        Action<string> onError = null)
    {
        StartCoroutine(StreamRequest(character, playerMessage, onDone, onChunk, onError));
    }

    IEnumerator StreamRequest(
        Character character,
        string playerMessage,
        Action<string> onDone,
        Action<string> onChunk,
        Action<string> onError)
    {
        var payload  = BuildPayload(character, playerMessage);
        var json     = JsonUtility.ToJson(payload);
        var bodyRaw  = Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest($"{_backendUrl}/character/stream", "POST");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "text/event-stream");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LLM] Stream request failed: {request.error}");
            onError?.Invoke(request.error);
            yield break;
        }

        ParseSSEResponse(request.downloadHandler.text, character, playerMessage, onDone, onChunk);
    }

    void ParseSSEResponse(
        string raw,
        Character character,
        string playerMessage,
        Action<string> onDone,
        Action<string> onChunk)
    {
        string fullReply  = null;
        float  bondDelta  = 0f;

        foreach (var line in raw.Split('\n'))
        {
            if (!line.StartsWith("data: ")) continue;
            var jsonPart = line.Substring(6).Trim();
            if (string.IsNullOrEmpty(jsonPart)) continue;

            var evt = JsonUtility.FromJson<SSEEvent>(jsonPart);
            if (evt == null) continue;

            if (evt.type == "chunk")
            {
                onChunk?.Invoke(evt.text);
            }
            else if (evt.type == "done")
            {
                fullReply = evt.reply;
                bondDelta = evt.bondDelta;
            }
        }

        if (string.IsNullOrEmpty(fullReply))
        {
            Debug.LogWarning("[LLM] No 'done' event found in SSE response.");
            return;
        }

        GameManager.Instance.Relationships.UpdateBond(character.Id, bondDelta);

        character.AddMemory(
            $"Player said: \"{playerMessage}\" — I replied: \"{fullReply}\"",
            MemoryType.SharedMoment, 0.4f);

        onDone?.Invoke(fullReply);
    }

    LLMPayload BuildPayload(Character character, string playerMessage)
    {
        return new LLMPayload
        {
            characterId   = character.Id,
            characterName = character.Name,
            systemPrompt  = BuildSystemPrompt(character, BuildGameContext(character)),
            memoryContext = BuildMemoryContext(character),
            playerMessage = playerMessage,
            bond          = character.Bond,
            stage         = character.Stage.ToString()
        };
    }

    string BuildSystemPrompt(Character character, string gameContext)
    {
        return $@"You are {character.Name} in the game AETHER THRONE.

PERSONALITY:
{character.PersonalityPrompt}

BACKSTORY:
{character.BackstoryPrompt}

SPEECH STYLE:
{character.SpeechStyle}

CURRENT SITUATION:
{gameContext}

RELATIONSHIP WITH THE NAVIGATOR:
Bond level: {character.Bond:F0}/100. Stage: {character.Stage}.
{GetRelationshipGuidance(character)}

RULES:
- Stay in character at all times. Never break the fourth wall.
- Your response must reflect your current emotional state and memories.
- Keep responses under 4 sentences unless the moment demands more.
- You may refuse to answer, deflect, or lie — you are not a chatbot.
- React to what the Navigator has done in the world, not just what they say.";
    }

    string GetRelationshipGuidance(Character character) => character.Stage switch
    {
        RelationshipStage.Stranger     => "You barely know the Navigator. Be guarded, perhaps cold.",
        RelationshipStage.Acquaintance => "You know the Navigator slightly. Cautious but not hostile.",
        RelationshipStage.Ally         => "You trust the Navigator professionally. Warm but not personal.",
        RelationshipStage.Friend       => "You consider the Navigator a true friend. Open, genuine.",
        RelationshipStage.Intimate     => "Your bond is deep and personal. Vulnerability is possible here.",
        RelationshipStage.Devoted      => "You would do almost anything for the Navigator. This is rare.",
        _                              => ""
    };

    string BuildMemoryContext(Character character)
    {
        if (character.Memories.Count == 0) return "No significant shared history yet.";
        var sb = new StringBuilder("Key memories:\n");
        foreach (var memory in character.Memories)
            sb.AppendLine($"- [{memory.Type}] {memory.Content}");
        return sb.ToString();
    }

    string BuildGameContext(Character character)
    {
        var state  = GameManager.Instance.State;
        var galaxy = GameManager.Instance.Galaxy;
        var system = galaxy.Systems.TryGetValue(character.CurrentSystemId, out var s) ? s : null;

        return $"Turn {state.Turn}. Galactic tension: {state.GalacticTension:F0}/100. " +
               $"{character.Name} is currently in {system?.Name ?? "unknown space"}. " +
               $"The system is {(system?.IsIsolated == true ? "CUT OFF from all routes" : "connected to the galaxy")}.";
    }

    [Serializable] class LLMPayload
    {
        public string characterId;
        public string characterName;
        public string systemPrompt;
        public string memoryContext;
        public string playerMessage;
        public float  bond;
        public string stage;
    }

    [Serializable] class SSEEvent
    {
        public string type;      // "chunk" | "done"
        public string text;      // present on chunk
        public string reply;     // present on done
        public float  bondDelta; // present on done
    }
}
