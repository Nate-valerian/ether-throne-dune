using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public Dictionary<string, Faction> Factions { get; private set; } = new();

    void Awake()
    {
        SeedFactions();
        EventBus.Subscribe<RouteOpenedEvent>(OnRouteOpened);
        EventBus.Subscribe<RouteClosedEvent>(OnRouteClosed);
        EventBus.Subscribe<BattleResolvedEvent>(OnBattleResolved);
    }

    void SeedFactions()
    {
        var factions = new[]
        {
            // The imperial bloodline — they found the Navigator as a child, shaped everything about them. They believe they own the Navigator.
            new Faction { Id = "house-vaethyr",  Name = "House Vaethyr",     Archetype = FactionArchetype.Political, Wealth = 800f,  MilitaryPower = 60f, PoliticalInfluence = 90f, NavigatorTrust = 10f  },
            // The trade empire — they need Fold-Routes to survive. The Navigator is their lifeline. They are the most generous and the most desperate.
            new Faction { Id = "fold-compact",   Name = "The Fold Compact",  Archetype = FactionArchetype.Merchant,  Wealth = 1200f, MilitaryPower = 20f, PoliticalInfluence = 50f, NavigatorTrust = 20f  },
            // The warlords — they see the Navigator as the greatest strategic asset in history and want exclusive control.
            new Faction { Id = "iron-covenant",  Name = "The Iron Covenant", Archetype = FactionArchetype.Military,  Wealth = 400f,  MilitaryPower = 95f, PoliticalInfluence = 30f, NavigatorTrust = -10f },
            // The dispossessed — people whose worlds were destroyed by routes the Navigator opened. They resist all fold-travel through their space.
            new Faction { Id = "the-unbound",    Name = "The Unbound",       Archetype = FactionArchetype.Rebel,     Wealth = 200f,  MilitaryPower = 40f, PoliticalInfluence = 20f, NavigatorTrust = 30f  },
        };

        foreach (var f in factions)
            Factions[f.Id] = f;
    }

    public void UpdateRelationships()
    {
        float tension = GameManager.Instance.State.GalacticTension;
        if (tension > 70f)
        {
            var military = Factions.Values.Where(f => f.Archetype == FactionArchetype.Military).ToList();
            foreach (var f in military)
                f.MilitaryPower += 2f;
        }

        GenerateOffers();
    }

    void GenerateOffers()
    {
        var closedRoutes = GameManager.Instance.Galaxy.Routes.Values
            .Where(r => !r.IsOpen).ToList();

        if (closedRoutes.Count == 0) return;

        foreach (var faction in Factions.Values)
        {
            // Each faction has a 30% chance to propose an offer per turn if none pending
            if (faction.PendingOffers.Count > 0) continue;
            if (Random.value > 0.3f) continue;

            var route = closedRoutes[Random.Range(0, closedRoutes.Count)];

            float bribe = faction.Archetype switch
            {
                FactionArchetype.Merchant  => Random.Range(40f, 80f),
                FactionArchetype.Military  => Random.Range(10f, 30f),
                FactionArchetype.Political => Random.Range(20f, 50f),
                _                          => Random.Range(5f, 25f)
            };

            bool deceptive = faction.Archetype == FactionArchetype.Military && Random.value < 0.4f;

            faction.PendingOffers.Add(new FactionOffer
            {
                FactionId         = faction.Id,
                RouteId           = route.Id,
                WealthOffered     = bribe,
                InfluenceOffered  = bribe * 0.5f,
                IsDeceptive       = deceptive,
                SecretCondition   = deceptive ? $"{faction.Name} will use this Fold-Route to move troops — the offer is a cover." : null
            });
        }
    }

    void OnRouteOpened(RouteOpenedEvent evt)
    {
        // Beneficiary gains trust, rivals lose trust
        foreach (var faction in Factions.Values)
        {
            if (faction.Id == evt.FactionId)
                faction.NavigatorTrust = Mathf.Clamp(faction.NavigatorTrust + 15f, -100f, 100f);
            else
                faction.NavigatorTrust = Mathf.Clamp(faction.NavigatorTrust - 5f, -100f, 100f);
        }
    }

    void OnRouteClosed(RouteClosedEvent evt)
    {
        // Closing a route breeds resentment
        foreach (var faction in Factions.Values)
            faction.NavigatorTrust = Mathf.Clamp(faction.NavigatorTrust - 8f, -100f, 100f);
    }

    void OnBattleResolved(BattleResolvedEvent evt)
    {
        if (Factions.TryGetValue(evt.LoserId, out var loser))
        {
            loser.MilitaryPower *= 0.7f;
            loser.Grievances.Add($"Lost battle in {evt.SystemId}");
        }
        if (Factions.TryGetValue(evt.WinnerId, out var winner))
            winner.MilitaryPower += 10f;
    }

    public Faction GetStrongestFaction() =>
        Factions.Values.OrderByDescending(f => f.MilitaryPower + f.PoliticalInfluence).First();
}
