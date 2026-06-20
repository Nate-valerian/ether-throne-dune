using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save()
    {
        var data = new SaveData();
        var gm = GameManager.Instance;

        data.state = gm.State;

        foreach (var (id, route) in gm.Galaxy.Routes)
            data.routes.Add(new RouteSaveEntry { id = id, isOpen = route.IsOpen, primaryFactionId = route.PrimaryBeneficiaryFactionId });

        foreach (var character in gm.Relationships.GetAllCharacters())
            data.characters.Add(new CharacterSaveEntry
            {
                id             = character.Id,
                bond           = character.Bond,
                stage          = character.Stage,
                currentSystemId = character.CurrentSystemId,
                isAlive        = character.IsAlive,
                memories       = character.Memories
            });

        foreach (var (id, faction) in gm.Factions.Factions)
            data.factions.Add(new FactionSaveEntry
            {
                id               = id,
                navigatorTrust   = faction.NavigatorTrust,
                militaryPower    = faction.MilitaryPower,
                politicalInfluence = faction.PoliticalInfluence,
                wealth           = faction.Wealth,
                grievances       = faction.Grievances,
                debts            = faction.Debts
            });

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
        Debug.Log($"[SaveSystem] Saved to {SavePath}");
    }

    public static bool Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveSystem] No save file found.");
            return false;
        }

        var json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<SaveData>(json);
        var gm   = GameManager.Instance;

        // Restore state
        gm.State.Turn                 = data.state.Turn;
        gm.State.NavigatorName        = data.state.NavigatorName;
        gm.State.NavigatorSanity      = data.state.NavigatorSanity;
        gm.State.NavigatorInfluence   = data.state.NavigatorInfluence;
        gm.State.GalacticTension      = data.state.GalacticTension;
        gm.State.ActiveWars           = data.state.ActiveWars;
        gm.State.DestroyedSystems     = data.state.DestroyedSystems;

        // Restore routes
        foreach (var entry in data.routes)
        {
            if (!gm.Galaxy.Routes.TryGetValue(entry.id, out var route)) continue;
            if (entry.isOpen && !route.IsOpen)
                gm.Galaxy.OpenRoute(entry.id, gm.State.NavigatorName);
            else if (!entry.isOpen && route.IsOpen)
                gm.Galaxy.CloseRoute(entry.id);
            route.PrimaryBeneficiaryFactionId = entry.primaryFactionId;
        }

        // Restore characters
        foreach (var entry in data.characters)
        {
            var character = gm.Relationships.GetCharacter(entry.id);
            if (character == null) continue;
            character.Bond            = entry.bond;
            character.Stage           = entry.stage;
            character.CurrentSystemId = entry.currentSystemId;
            character.IsAlive         = entry.isAlive;
            character.Memories        = entry.memories ?? new();
        }

        // Restore factions
        foreach (var entry in data.factions)
        {
            if (!gm.Factions.Factions.TryGetValue(entry.id, out var faction)) continue;
            faction.NavigatorTrust     = entry.navigatorTrust;
            faction.MilitaryPower      = entry.militaryPower;
            faction.PoliticalInfluence = entry.politicalInfluence;
            faction.Wealth             = entry.wealth;
            faction.Grievances         = entry.grievances ?? new();
            faction.Debts              = entry.debts      ?? new();
        }

        EventBus.Publish(new SaveLoadedEvent());
        Debug.Log($"[SaveSystem] Loaded turn {gm.State.Turn}.");
        return true;
    }

    public static bool SaveExists() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        Debug.Log("[SaveSystem] Save deleted.");
    }
}

// ── Save data containers ────────────────────────────────────────────────────

[Serializable]
class SaveData
{
    public GameState state = new();
    public List<RouteSaveEntry>     routes     = new();
    public List<CharacterSaveEntry> characters = new();
    public List<FactionSaveEntry>   factions   = new();
}

[Serializable]
class RouteSaveEntry
{
    public string id;
    public bool   isOpen;
    public string primaryFactionId;
}

[Serializable]
class CharacterSaveEntry
{
    public string            id;
    public float             bond;
    public RelationshipStage stage;
    public string            currentSystemId;
    public bool              isAlive;
    public List<MemoryEntry> memories;
}

[Serializable]
class FactionSaveEntry
{
    public string       id;
    public float        navigatorTrust;
    public float        militaryPower;
    public float        politicalInfluence;
    public float        wealth;
    public List<string> grievances;
    public List<string> debts;
}
