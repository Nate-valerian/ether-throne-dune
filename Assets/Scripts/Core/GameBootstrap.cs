using UnityEngine;

// Attach this to an empty "Bootstrap" GameObject in your Main scene.
// It wires together every system at runtime so the scene has no hidden
// cross-object dependencies — everything flows through GameManager.Instance.
[DefaultExecutionOrder(-100)]
public class GameBootstrap : MonoBehaviour
{
    [Header("References — drag from scene hierarchy")]
    [SerializeField] GameManager gameManager;
    [SerializeField] LLMService llmService;

    [Header("UI Panels — drag from Canvas hierarchy")]
    [SerializeField] GalaxyUI galaxyUI;
    [SerializeField] DialogueUI dialogueUI;
    [SerializeField] FactionStatusUI factionStatusUI;

    void Start()
    {
        // Validate critical references
        if (gameManager == null)
        {
            Debug.LogError("[Bootstrap] GameManager not assigned. Create a 'GameManager' GameObject and assign it.");
            return;
        }

        if (llmService == null)
        {
            Debug.LogError("[Bootstrap] LLMService not assigned.");
            return;
        }

        EventBus.Publish(new GameStartedEvent());

        if (SaveSystem.SaveExists())
            SaveSystem.Load();

        Debug.Log("[Bootstrap] AetherThrone initialised.");
        PrintSystemSummary();
    }

    void PrintSystemSummary()
    {
        var galaxy = GameManager.Instance.Galaxy;
        Debug.Log($"[Galaxy] {galaxy.Systems.Count} systems, {galaxy.Routes.Count} routes.");

        foreach (var s in galaxy.Systems.Values)
            Debug.Log($"  System: {s.Name} ({s.SystemType}) — {(s.IsIsolated ? "ISOLATED" : "connected")}");

        var factions = GameManager.Instance.Factions.Factions;
        Debug.Log($"[Factions] {factions.Count} factions.");

        var characters = GameManager.Instance.Relationships.GetAllCharacters();
        Debug.Log($"[Characters] {characters.Count} seeded: {string.Join(", ", characters.ConvertAll(c => c.Name))}");
    }
}
