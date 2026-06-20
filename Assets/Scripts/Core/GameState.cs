using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public int Turn { get; set; } = 1;

    public string NavigatorName { get; set; } = "Navigator";
    public float NavigatorSanity { get; set; } = 100f;
    public float NavigatorInfluence { get; set; } = 50f;

    public List<string> OpenRoutesThisTurn { get; set; } = new();

    public float GalacticTension { get; set; } = 30f;

    public List<string> ActiveWars { get; set; } = new();
    public List<string> DestroyedSystems { get; set; } = new();

    public bool NavigatorDead => NavigatorSanity <= 0f;
}
