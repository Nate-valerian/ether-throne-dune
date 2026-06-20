using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel shown when the player clicks a route line on the map
public class RoutePanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _routeHeader;
    [SerializeField] TextMeshProUGUI _statusLabel;
    [SerializeField] TextMeshProUGUI _costLabel;
    [SerializeField] TextMeshProUGUI _tradeValue;
    [SerializeField] TextMeshProUGUI _militaryValue;
    [SerializeField] TextMeshProUGUI _influenceWarning;
    [SerializeField] Button _openButton;
    [SerializeField] Button _closeButton;

    private Route _activeRoute;

    void Awake()
    {
        _openButton.onClick.AddListener(OnOpen);
        _closeButton.onClick.AddListener(OnClose);
        gameObject.SetActive(false);
    }

    public void Show(Route route)
    {
        _activeRoute = route;
        gameObject.SetActive(true);
        Refresh();
    }

    void Refresh()
    {
        var galaxy = GameManager.Instance.Galaxy;
        var from = galaxy.Systems.TryGetValue(_activeRoute.FromSystemId, out var f) ? f.Name : "?";
        var to   = galaxy.Systems.TryGetValue(_activeRoute.ToSystemId,   out var t) ? t.Name : "?";

        _routeHeader.text = $"{from}  →  {to}";
        _statusLabel.text = _activeRoute.IsOpen ? "OPEN" : "CLOSED";
        _statusLabel.color = _activeRoute.IsOpen
            ? new Color(.3f, .9f, .5f)
            : new Color(.6f, .6f, .6f);

        _costLabel.text    = $"Influence cost  {_activeRoute.InfluenceCost:F0}";
        _tradeValue.text   = $"Trade value  {_activeRoute.TradeValue:F0}";
        _militaryValue.text = $"Military value  {_activeRoute.MilitaryValue:F0}";

        float influence = GameManager.Instance.State.NavigatorInfluence;
        bool canAfford  = influence >= _activeRoute.InfluenceCost;

        _openButton.gameObject.SetActive(!_activeRoute.IsOpen);
        _closeButton.gameObject.SetActive(_activeRoute.IsOpen);
        _openButton.interactable = canAfford;

        _influenceWarning.gameObject.SetActive(!canAfford && !_activeRoute.IsOpen);
        _influenceWarning.text = $"Need {_activeRoute.InfluenceCost:F0} influence (you have {influence:F0})";
    }

    void OnOpen()
    {
        if (_activeRoute == null) return;
        GameManager.Instance.Navigator.OpenRoute(_activeRoute.Id);
        Refresh();
    }

    void OnClose()
    {
        if (_activeRoute == null) return;
        GameManager.Instance.Navigator.CloseRoute(_activeRoute.Id);
        Refresh();
    }

    public void Hide() => gameObject.SetActive(false);
}
