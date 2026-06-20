using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GalaxyMap : MonoBehaviour
{
    public Dictionary<string, StarSystem> Systems { get; private set; } = new();
    public Dictionary<string, Route> Routes { get; private set; } = new();

    void Awake()
    {
        GenerateGalaxy();
    }

    void GenerateGalaxy()
    {
        // Seed star systems
        var systems = new[]
        {
            new StarSystem { Id = "aethar-prime", Name = "Aethar Prime",  SystemType = SystemType.Capital,      Position = new Vector2( 0f,    .5f), Population = 12000f, SpiceYield = 5f, MilitaryStrength = 80f,  ControllingFactionId = "house-vaethyr",  CharacterIds = new() { "vael" } },
            new StarSystem { Id = "kethara",      Name = "Kethara",        SystemType = SystemType.Industrial,   Position = new Vector2( .6f,   .2f), Population =  4200f, SpiceYield = 2f, MilitaryStrength = 40f,  ControllingFactionId = "fold-compact",   CharacterIds = new() },
            new StarSystem { Id = "dravath",      Name = "Dravath",        SystemType = SystemType.Military,     Position = new Vector2(-.6f,   .1f), Population =   800f, SpiceYield = 1f, MilitaryStrength = 95f,  ControllingFactionId = "iron-covenant",  CharacterIds = new() { "kael" } },
            new StarSystem { Id = "edenos",       Name = "Edenos",         SystemType = SystemType.Agricultural, Position = new Vector2( .3f,  -.5f), Population =  6000f, SpiceYield = 8f, MilitaryStrength = 20f,  ControllingFactionId = "the-unbound",    CharacterIds = new() },
            new StarSystem { Id = "the-null",     Name = "The Null",       SystemType = SystemType.Frontier,     Position = new Vector2(-.2f,  -.4f), Population =   120f, SpiceYield = 0f, MilitaryStrength = 10f,  ControllingFactionId = "neutral",        CharacterIds = new() { "lyra" } },
        };

        foreach (var s in systems)
            Systems[s.Id] = s;

        // Possible routes (all closed by default — Navigator opens them)
        CreateRoute("aethar-prime", "kethara",  tradeValue: 40f, militaryValue: 20f);
        CreateRoute("aethar-prime", "dravath",  tradeValue: 10f, militaryValue: 80f);
        CreateRoute("kethara",      "edenos",   tradeValue: 60f, militaryValue: 5f);
        CreateRoute("dravath",      "the-null", tradeValue: 5f,  militaryValue: 30f);
        CreateRoute("edenos",       "the-null", tradeValue: 20f, militaryValue: 10f);
    }

    void CreateRoute(string from, string to, float tradeValue, float militaryValue)
    {
        var id = $"{from}--{to}";
        Routes[id] = new Route
        {
            Id = id,
            FromSystemId = from,
            ToSystemId = to,
            TradeValue = tradeValue,
            MilitaryValue = militaryValue
        };
    }

    public bool OpenRoute(string routeId, string navigatorName)
    {
        if (!Routes.TryGetValue(routeId, out var route)) return false;
        if (route.IsOpen) return false;

        route.IsOpen = true;
        route.OpenedByNavigator = navigatorName;

        Systems[route.FromSystemId].ConnectedRoutes.Add(routeId);
        Systems[route.ToSystemId].ConnectedRoutes.Add(routeId);

        EventBus.Publish(new RouteOpenedEvent(route.FromSystemId, route.ToSystemId, route.PrimaryBeneficiaryFactionId));
        return true;
    }

    public bool CloseRoute(string routeId)
    {
        if (!Routes.TryGetValue(routeId, out var route)) return false;
        if (!route.IsOpen) return false;

        route.IsOpen = false;
        Systems[route.FromSystemId].ConnectedRoutes.Remove(routeId);
        Systems[route.ToSystemId].ConnectedRoutes.Remove(routeId);

        EventBus.Publish(new RouteClosedEvent(route.FromSystemId, route.ToSystemId));
        return true;
    }

    public List<StarSystem> GetIsolatedSystems() =>
        Systems.Values.Where(s => s.IsIsolated).ToList();
}
