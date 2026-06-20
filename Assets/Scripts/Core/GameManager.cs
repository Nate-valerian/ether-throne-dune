using UnityEngine;

// Single entry point for all game systems.
// Attach this to a "GameManager" GameObject in the Main scene.
// All child systems (GalaxyMap, FactionManager, etc.) should be child GameObjects.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; }

    // Systems — resolved via GetComponentInChildren so Unity wires them automatically
    public GalaxyMap Galaxy { get; private set; }
    public FactionManager Factions { get; private set; }
    public NavigatorController Navigator { get; private set; }
    public WarSystem War { get; private set; }
    public RelationshipSystem Relationships { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Galaxy        = GetComponentInChildren<GalaxyMap>();
        Factions      = GetComponentInChildren<FactionManager>();
        Navigator     = GetComponentInChildren<NavigatorController>();
        War           = GetComponentInChildren<WarSystem>();
        Relationships = GetComponentInChildren<RelationshipSystem>();

        if (Galaxy == null || Factions == null || Navigator == null || War == null || Relationships == null)
            Debug.LogError("[GameManager] One or more child systems are missing. Check the scene hierarchy.");

        State = new GameState();
    }

    public void AdvanceTurn()
    {
        State.Turn++;
        War.ResolvePendingBattles();
        Factions.UpdateRelationships();
        Relationships.DecayOverTime();
        EventBus.Publish(new TurnAdvancedEvent(State.Turn));
        SaveSystem.Save();
    }
}
