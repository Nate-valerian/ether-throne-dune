using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Manages the galaxy map panel.
// Wire up: SystemNodePrefab, RouteLinePrefab, SystemInfoPanel, RoutePanel,
// TensionBar, TurnLabel, InfluenceLabel, SanityLabel, EndTurnButton
public class GalaxyUI : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] RectTransform _mapContainer;
    [SerializeField] GameObject _systemNodePrefab;    // circle + system name
    [SerializeField] GameObject _routeLinePrefab;     // line renderer or image

    [Header("HUD")]
    [SerializeField] TextMeshProUGUI _turnLabel;
    [SerializeField] TextMeshProUGUI _influenceLabel;
    [SerializeField] TextMeshProUGUI _sanityLabel;
    [SerializeField] Slider _tensionBar;
    [SerializeField] TextMeshProUGUI _tensionLabel;

    [Header("Panels")]
    [SerializeField] SystemInfoPanel _systemInfoPanel;
    [SerializeField] RoutePanel _routePanel;

    [Header("Controls")]
    [SerializeField] Button _endTurnButton;

    private readonly Dictionary<string, GameObject> _systemNodes = new();
    private readonly Dictionary<string, GameObject> _routeLines = new();

    void Awake()
    {
        _endTurnButton.onClick.AddListener(OnEndTurn);

        EventBus.Subscribe<TurnAdvancedEvent>(OnTurnAdvanced);
        EventBus.Subscribe<RouteOpenedEvent>(_ => RefreshRoutes());
        EventBus.Subscribe<RouteClosedEvent>(_ => RefreshRoutes());
        EventBus.Subscribe<WarDeclaredEvent>(OnWarDeclared);
        EventBus.Subscribe<GameStartedEvent>(_ => BuildMap());
    }

    void BuildMap()
    {
        foreach (var (id, system) in GameManager.Instance.Galaxy.Systems)
        {
            var node = Instantiate(_systemNodePrefab, _mapContainer);
            node.GetComponent<RectTransform>().anchoredPosition = system.Position * 300f;
            node.GetComponentInChildren<TextMeshProUGUI>().text = system.Name;

            var btn = node.GetComponent<Button>();
            btn.onClick.AddListener(() => OnSystemClicked(id));

            _systemNodes[id] = node;
        }

        RefreshRoutes();
        RefreshHUD();
    }

    void RefreshRoutes()
    {
        // Clear old lines
        foreach (var line in _routeLines.Values)
            Destroy(line);
        _routeLines.Clear();

        foreach (var (id, route) in GameManager.Instance.Galaxy.Routes)
        {
            if (!_systemNodes.TryGetValue(route.FromSystemId, out var fromNode)) continue;
            if (!_systemNodes.TryGetValue(route.ToSystemId, out var toNode)) continue;

            var line = Instantiate(_routeLinePrefab, _mapContainer);
            line.transform.SetAsFirstSibling(); // draw behind nodes

            var img = line.GetComponent<Image>();
            img.color = route.IsOpen
                ? new Color(.3f, .9f, .5f, .8f)   // green — open
                : new Color(.4f, .4f, .5f, .4f);   // grey — closed

            PositionLine(line.GetComponent<RectTransform>(),
                fromNode.GetComponent<RectTransform>().anchoredPosition,
                toNode.GetComponent<RectTransform>().anchoredPosition);

            var routeId = id;
            line.GetComponent<Button>()?.onClick.AddListener(() => OnRouteClicked(routeId));
            _routeLines[id] = line;
        }
    }

    void PositionLine(RectTransform rt, Vector2 from, Vector2 to)
    {
        var dir = to - from;
        var dist = dir.magnitude;
        rt.sizeDelta = new Vector2(dist, 3f);
        rt.anchoredPosition = (from + to) * 0.5f;
        rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    void OnSystemClicked(string systemId)
    {
        var system = GameManager.Instance.Galaxy.Systems[systemId];
        _systemInfoPanel?.Show(system);
        _routePanel?.Hide();
    }

    void OnRouteClicked(string routeId)
    {
        var route = GameManager.Instance.Galaxy.Routes[routeId];
        _routePanel?.Show(route);
        _systemInfoPanel?.Hide();
    }

    void OnEndTurn()
    {
        _endTurnButton.interactable = false;
        GameManager.Instance.AdvanceTurn();
    }

    void OnTurnAdvanced(TurnAdvancedEvent evt)
    {
        RefreshHUD();
        _endTurnButton.interactable = true;
    }

    void OnWarDeclared(WarDeclaredEvent evt)
    {
        // Flash tension bar red briefly
        _tensionBar.GetComponent<Image>().color = new Color(1f, .2f, .2f);
        Invoke(nameof(ResetTensionColor), 1.2f);
    }

    void ResetTensionColor() =>
        _tensionBar.GetComponent<Image>().color = new Color(.9f, .5f, .1f);

    void RefreshHUD()
    {
        var state = GameManager.Instance.State;
        _turnLabel.text = $"TURN {state.Turn}";
        _influenceLabel.text = $"Influence  {state.NavigatorInfluence:F0}";
        _sanityLabel.text = $"Sanity  {state.NavigatorSanity:F0}";
        _tensionBar.value = state.GalacticTension / 100f;
        _tensionLabel.text = $"Galactic Tension  {state.GalacticTension:F0}%";
    }
}
