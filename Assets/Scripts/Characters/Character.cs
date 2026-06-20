using System.Collections.Generic;

[System.Serializable]
public class Character
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string FactionId { get; set; }
    public CharacterRole Role { get; set; }
    public string CurrentSystemId { get; set; }

    // Personality — shapes how LLM responds
    public string PersonalityPrompt { get; set; }
    public string BackstoryPrompt { get; set; }
    public string SpeechStyle { get; set; }

    // Relationship with Navigator (-100 hate → 100 love)
    public float Bond { get; set; } = 0f;
    public RelationshipStage Stage { get; set; } = RelationshipStage.Stranger;

    // Memory — sent to LLM as context
    public List<MemoryEntry> Memories { get; set; } = new();

    public bool IsAlive { get; set; } = true;

    public void AddMemory(string content, MemoryType type, float emotionalWeight)
    {
        Memories.Add(new MemoryEntry
        {
            Content = content,
            Type = type,
            EmotionalWeight = emotionalWeight,
            Turn = GameManager.Instance?.State.Turn ?? 0
        });

        // Keep only the most emotionally significant memories (top 20)
        if (Memories.Count > 30)
            Memories.Sort((a, b) => b.EmotionalWeight.CompareTo(a.EmotionalWeight));
        if (Memories.Count > 20)
            Memories.RemoveRange(20, Memories.Count - 20);
    }
}

public enum CharacterRole
{
    Diplomat,
    Warlord,
    Merchant,
    Lover,
    Spy,
    Exile
}

public enum RelationshipStage
{
    Stranger,
    Acquaintance,
    Ally,
    Friend,
    Intimate,
    Devoted
}

[System.Serializable]
public class MemoryEntry
{
    public string Content { get; set; }
    public MemoryType Type { get; set; }
    public float EmotionalWeight { get; set; } // 0-1
    public int Turn { get; set; }
}

public enum MemoryType
{
    SharedMoment,
    Betrayal,
    Gift,
    Loss,
    Promise,
    Conflict,
    Intimacy
}
