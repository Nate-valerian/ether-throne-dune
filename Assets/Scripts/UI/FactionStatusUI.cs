using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Scrollable list panel showing all faction trust levels and their current power
public class FactionStatusUI : MonoBehaviour
{
    [SerializeField] Transform _listContainer;
    [SerializeField] GameObject _factionRowPrefab;
    [SerializeField] Button _toggleButton;

    private bool _visible;

    void Awake()
    {
        _toggleButton.onClick.AddListener(Toggle);
        EventBus.Subscribe<TurnAdvancedEvent>(_ => Rebuild());
        EventBus.Subscribe<RouteOpenedEvent>(_ => Rebuild());
        EventBus.Subscribe<BattleResolvedEvent>(_ => Rebuild());
        gameObject.SetActive(false);
    }

    void Toggle()
    {
        _visible = !_visible;
        gameObject.SetActive(_visible);
        if (_visible) Rebuild();
    }

    void Rebuild()
    {
        foreach (Transform child in _listContainer)
            Destroy(child.gameObject);

        foreach (var faction in GameManager.Instance.Factions.Factions.Values)
        {
            var row = Instantiate(_factionRowPrefab, _listContainer);
            var labels = row.GetComponentsInChildren<TextMeshProUGUI>();

            // Expected prefab: [0] Name, [1] Archetype, [2] Trust, [3] Military, [4] Influence
            labels[0].text = faction.Name;
            labels[1].text = faction.Archetype.ToString();
            labels[2].text = $"Trust {faction.NavigatorTrust:+0;-0;0}";
            labels[3].text = $"Military {faction.MilitaryPower:F0}";
            labels[4].text = $"Influence {faction.PoliticalInfluence:F0}";

            // Colour trust label
            labels[2].color = faction.NavigatorTrust switch
            {
                > 40f  => new Color(.3f, .9f, .5f),
                > 0f   => new Color(.85f, .85f, .3f),
                > -30f => new Color(.9f, .5f, .2f),
                _      => new Color(.9f, .2f, .2f)
            };
        }
    }
}
