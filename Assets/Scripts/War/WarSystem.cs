using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WarSystem : MonoBehaviour
{
    private readonly List<PendingBattle> _pendingBattles = new();

    void Awake()
    {
        EventBus.Subscribe<RouteOpenedEvent>(OnRouteOpened);
    }

    // Opening a military route may trigger war
    void OnRouteOpened(RouteOpenedEvent evt)
    {
        var factions = GameManager.Instance.Factions;
        var military = factions.Factions.Values
            .Where(f => f.Archetype == FactionArchetype.Military && f.Id != evt.FactionId)
            .ToList();

        foreach (var aggressor in military)
        {
            float warChance = Mathf.InverseLerp(0f, 100f, aggressor.MilitaryPower - 40f);
            if (Random.value < warChance * 0.3f)
            {
                DeclareWar(aggressor.Id, evt.FactionId, evt.ToSystemId);
                break;
            }
        }
    }

    public void DeclareWar(string attackerId, string defenderId, string systemId)
    {
        _pendingBattles.Add(new PendingBattle
        {
            AttackerId = attackerId,
            DefenderId = defenderId,
            SystemId = systemId
        });

        GameManager.Instance.State.GalacticTension =
            Mathf.Min(100f, GameManager.Instance.State.GalacticTension + 15f);

        EventBus.Publish(new WarDeclaredEvent(attackerId, defenderId));
        Debug.Log($"War declared: {attackerId} vs {defenderId} over {systemId}");
    }

    public void ResolvePendingBattles()
    {
        foreach (var battle in _pendingBattles.ToList())
        {
            ResolveBattle(battle);
            _pendingBattles.Remove(battle);
        }
    }

    void ResolveBattle(PendingBattle battle)
    {
        var factions = GameManager.Instance.Factions.Factions;
        if (!factions.TryGetValue(battle.AttackerId, out var attacker)) return;
        if (!factions.TryGetValue(battle.DefenderId, out var defender)) return;

        // Routes affect battle outcome — isolation weakens defender
        var galaxy = GameManager.Instance.Galaxy;
        bool defenderIsolated = galaxy.Systems.TryGetValue(battle.SystemId, out var sys) && sys.IsIsolated;
        float defenderStrength = defender.MilitaryPower * (defenderIsolated ? 0.5f : 1f);

        string winnerId = attacker.MilitaryPower > defenderStrength ? battle.AttackerId : battle.DefenderId;
        string loserId = winnerId == battle.AttackerId ? battle.DefenderId : battle.AttackerId;

        EventBus.Publish(new BattleResolvedEvent(winnerId, loserId, battle.SystemId));
    }

    private class PendingBattle
    {
        public string AttackerId;
        public string DefenderId;
        public string SystemId;
    }
}
