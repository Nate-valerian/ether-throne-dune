using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Local LLM Service that replaces the Claude API with a local model implementation
/// Uses Oobabooga's text-generation-webui API compatible endpoint
/// </summary>
public class LocalLLMService : MonoBehaviour
{
    public static LocalLLMService Instance { get; private set; }

    [Header("Local LLM Configuration")]
    [SerializeField] private string _localApiUrl = "http://localhost:5000"; // Default for text-generation-webui
    [SerializeField] private float _timeout = 60f;
    [SerializeField] private int _maxTokens = 300;
    [SerializeField] private float _temperature = 0.7f;
    [SerializeField] private float _topP = 0.9f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Get a character response using the local LLM
    /// </summary>
    public void GetCharacterResponse(
        Character character,
        string playerMessage,
        Action<string> onDone,
        Action<string> onChunk = null,
        Action<string> onError = null)
    {
        StartCoroutine(LocalStreamRequest(character, playerMessage, onDone, onChunk, onError));
    }

    IEnumerator LocalStreamRequest(
        Character character,
        string playerMessage,
        Action<string> onDone,
        Action<string> onChunk,
        Action<string> onError)
    {
        var payload = BuildLocalPayload(character, playerMessage);
        var json = JsonUtility.ToJson(payload);
        var bodyRaw = Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest($"{_localApiUrl}/api/v1/generate", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LocalLLM] Request failed: {request.error}");
            onError?.Invoke(request.error);
            yield break;
        }

        var responseText = request.downloadHandler?.text ?? "";
        
        try
        {
            var response = JsonUtility.FromJson<LocalAPIResponse>(responseText);
            if (string.IsNullOrEmpty(response?.results?[0]?.text))
            {
                throw new Exception("Empty response from local LLM");
            }

            var fullReply = response.results[0].text.Trim();
            
            // Calculate a simple bond delta based on keywords (as a replacement for the Claude Haiku classifier)
            float bondDelta = CalculateBondDelta(fullReply, character.Bond, character.Stage.ToString());
            
            // Update relationship in the game
            GameManager.Instance.Relationships.UpdateBond(character.Id, bondDelta);

            character.AddMemory(
                $"Player said: \"{playerMessage}\" — I replied: \"{fullReply}\"",
                MemoryType.SharedMoment, 0.4f);

            onDone?.Invoke(fullReply);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LocalLLM] Failed to parse response: {ex.Message}");
            onError?.Invoke(ex.Message);
        }
    }

    LocalLLMPayload BuildLocalPayload(Character character, string playerMessage)
    {
        return new LocalLLMPayload
        {
            prompt = BuildSystemPrompt(character, BuildGameContext(character), playerMessage),
            max_new_tokens = _maxTokens,
            temperature = _temperature,
            top_p = _topP,
            do_sample = true,
            seed = -1,
            stopping_strings = new List<string> { "\nPlayer:", "\nNavigator:", "Player said:", "Navigator said:" },
            add_bos_token = true,
            ban_eos_token = false,
            skip_special_tokens = true
        };
    }

    string BuildSystemPrompt(Character character, string gameContext, string playerMessage)
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

PLAYER MESSAGE TO RESPOND TO:
{playerMessage}

RULES:
- Stay in character at all times. Never break the fourth wall.
- Your response must reflect your current emotional state and memories.
- Keep responses under 4 sentences unless the moment demands more.
- You may refuse to answer, deflect, or lie — you are not a chatbot.
- React to what the Navigator has done in the world, not just what they say.

{character.Name}:";
    }

    string GetRelationshipGuidance(Character character) => character.Stage switch
    {
        RelationshipStage.Stranger => "You barely know the Navigator. Be guarded, perhaps cold.",
        RelationshipStage.Acquaintance => "You know the Navigator slightly. Cautious but not hostile.",
        RelationshipStage.Ally => "You trust the Navigator professionally. Warm but not personal.",
        RelationshipStage.Friend => "You consider the Navigator a true friend. Open, genuine.",
        RelationshipStage.Intimate => "Your bond is deep and personal. Vulnerability is possible here.",
        RelationshipStage.Devoted => "You would do almost anything for the Navigator. This is rare.",
        _ => ""
    };

    string BuildGameContext(Character character)
    {
        var state = GameManager.Instance.State;
        var galaxy = GameManager.Instance.Galaxy;
        var system = galaxy.Systems.TryGetValue(character.CurrentSystemId, out var s) ? s : null;

        return $"Turn {state.Turn}. Galactic tension: {state.GalacticTension:F0}/100. " +
               $"{character.Name} is currently in {system?.Name ?? "unknown space"}. " +
               $"The system is {(system?.IsIsolated == true ? "CUT OFF from all routes" : "connected to the galaxy")}.";
    }

    float CalculateBondDelta(string reply, float currentBond, string stage)
    {
        // Simple keyword-based sentiment analysis as replacement for Claude Haiku classifier
        var positiveWords = new[] { "happy", "glad", "good", "great", "wonderful", "amazing", "pleased", "delighted", "thrilled", "excited", "thank", "appreciate", "love", "care", "trust", "friend", "ally" };
        var negativeWords = new[] { "angry", "hate", "terrible", "awful", "horrible", "disgusted", "annoyed", "frustrated", "mad", "upset", "worried", "concerned", "fear", "scared", "disappointed", "sorry", "apologize" };
        
        int positiveCount = 0, negativeCount = 0;
        var lowerReply = reply.ToLower();
        
        foreach (var word in positiveWords)
            if (lowerReply.Contains(word)) positiveCount++;
        
        foreach (var word in negativeWords)
            if (lowerReply.Contains(word)) negativeCount++;

        // Base score between -10 and +10
        float score = (positiveCount - negativeCount) * 2.0f;
        
        // Clamp to reasonable bounds
        score = Mathf.Clamp(score, -10f, 10f);
        
        // Adjust based on current relationship stage (more impact when closer)
        float relationshipMultiplier = 0.8f + (currentBond / 100f) * 0.4f; // 0.8 to 1.2 multiplier
        
        return score * relationshipMultiplier;
    }

    [Serializable] class LocalLLMPayload
    {
        public string prompt;
        public int max_new_tokens;
        public float temperature;
        public float top_p;
        public bool do_sample;
        public int seed;
        public List<string> stopping_strings;
        public bool add_bos_token;
        public bool ban_eos_token;
        public bool skip_special_tokens;
    }

    [Serializable] class LocalAPIResponse
    {
        public List<LocalResult> results;
    }

    [Serializable] class LocalResult
    {
        public string text;
    }
}