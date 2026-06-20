using System.Collections.Generic;

[System.Serializable]
public class Faction
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public FactionArchetype Archetype { get; set; }

    public float Wealth { get; set; }
    public float MilitaryPower { get; set; }
    public float PoliticalInfluence { get; set; }

    // Relationship with the Navigator (-100 to 100)
    public float NavigatorTrust { get; set; } = 0f;

    // Pending offers to the Navigator
    public List<FactionOffer> PendingOffers { get; set; } = new();

    // Memory of Navigator's past decisions
    public List<string> Grievances { get; set; } = new();
    public List<string> Debts { get; set; } = new();
}

public enum FactionArchetype
{
    Military,       // wants war routes, will backstab when strong enough
    Merchant,       // wants trade routes, reliable but greedy
    Political,      // wants influence, plays long games
    Religious,      // wants pilgrimage routes, unpredictable and dangerous
    Rebel           // wants to disrupt, enemy of stability
}

[System.Serializable]
public class FactionOffer
{
    public string FactionId { get; set; }
    public string RouteId { get; set; }
    public float WealthOffered { get; set; }
    public float InfluenceOffered { get; set; }
    public string SecretCondition { get; set; } // revealed only after acceptance
    public bool IsDeceptive { get; set; }
}
