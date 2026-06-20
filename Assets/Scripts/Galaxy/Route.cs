using UnityEngine;

[System.Serializable]
public class Route
{
    public string Id { get; set; }
    public string FromSystemId { get; set; }
    public string ToSystemId { get; set; }
    public bool IsOpen { get; set; } = false;
    public string OpenedByNavigator { get; set; }

    // Who benefits from this route
    public string PrimaryBeneficiaryFactionId { get; set; }

    // Cost to open / maintain
    public float InfluenceCost { get; set; } = 10f;
    public int TurnsOpen { get; set; } = 0;

    // Consequences — populated after opening
    public float TradeValue { get; set; }
    public float MilitaryValue { get; set; }
}
