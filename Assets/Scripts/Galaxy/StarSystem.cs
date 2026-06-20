using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StarSystem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Vector2 Position { get; set; } // position on galaxy map
    public string ControllingFactionId { get; set; }
    public SystemType SystemType { get; set; }

    public float Population { get; set; }   // millions
    public float SpiceYield { get; set; }   // resource output per turn
    public float MilitaryStrength { get; set; }

    public bool IsIsolated => ConnectedRoutes.Count == 0;
    public List<string> ConnectedRoutes { get; set; } = new();
    public List<string> CharacterIds { get; set; } = new(); // characters present here
}

public enum SystemType
{
    Capital,
    Industrial,
    Agricultural,
    Military,
    Frontier
}
