using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel that shows pending faction offers at the start of each turn.
// Hierarchy: FactionOfferUI (Panel, active:false)
//   ├── Title (TextMeshProUGUI)
//   ├── OfferContainer (VerticalLayoutGroup)
//   │     └── [OfferRowPrefab instances]
//   └── CloseButton (Button)
//
// OfferRowPrefab: HorizontalLayoutGroup
//   ├── FactionLabel  (TextMeshProUGUI)
//   ├── DescLabel     (TextMeshProUGUI)
//   ├── RewardLabel   (TextMeshProUGUI)
//   ├── AcceptButton  (Button)
//   └── DeclineButton (Button)
public class FactionOfferUI : MonoBehaviour
{
    [SerializeField] Transform _offerContainer;
    [SerializeField] GameObject _offerRowPrefab;
    [SerializeField] Button _closeButton;

    void Awake()
    {
        _closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        EventBus.Subscribe<TurnAdvancedEvent>(_ => CheckForOffers());
        gameObject.SetActive(false);
    }

    void CheckForOffers()
    {
        var allOffers = new List<(Faction faction, FactionOffer offer)>();
        foreach (var faction in GameManager.Instance.Factions.Factions.Values)
            foreach (var offer in faction.PendingOffers)
                allOffers.Add((faction, offer));

        if (allOffers.Count == 0) return;

        BuildRows(allOffers);
        gameObject.SetActive(true);
    }

    void BuildRows(List<(Faction faction, FactionOffer offer)> offers)
    {
        foreach (Transform child in _offerContainer)
            Destroy(child.gameObject);

        foreach (var (faction, offer) in offers)
        {
            var row    = Instantiate(_offerRowPrefab, _offerContainer);
            var labels = row.GetComponentsInChildren<TextMeshProUGUI>();
            var btns   = row.GetComponentsInChildren<Button>();

            var galaxy = GameManager.Instance.Galaxy;
            string routeDesc = galaxy.Routes.TryGetValue(offer.RouteId, out var route)
                ? $"{galaxy.Systems[route.FromSystemId].Name} → {galaxy.Systems[route.ToSystemId].Name}"
                : offer.RouteId;

            // [0] faction  [1] description  [2] reward
            labels[0].text = faction.Name;
            labels[1].text = $"Open route: {routeDesc}";
            labels[2].text = $"+{offer.WealthOffered:F0} wealth  +{offer.InfluenceOffered:F0} influence";

            if (offer.IsDeceptive)
                labels[1].color = new Color(1f, .4f, .2f); // orange hint

            var capturedFaction = faction;
            var capturedOffer   = offer;

            // [0] Accept  [1] Decline
            btns[0].onClick.AddListener(() => Accept(capturedFaction, capturedOffer));
            btns[1].onClick.AddListener(() => Decline(capturedFaction, capturedOffer));
        }
    }

    void Accept(Faction faction, FactionOffer offer)
    {
        var state = GameManager.Instance.State;
        state.NavigatorInfluence += offer.InfluenceOffered;

        GameManager.Instance.Navigator.OpenRoute(offer.RouteId, faction.Id);
        faction.Debts.Add($"Navigator opened {offer.RouteId} for us.");
        faction.PendingOffers.Remove(offer);

        if (offer.IsDeceptive)
            Debug.Log($"[Offers] {faction.Name}'s hidden condition: {offer.SecretCondition}");

        Rebuild();
    }

    void Decline(Faction faction, FactionOffer offer)
    {
        faction.NavigatorTrust -= 5f;
        faction.PendingOffers.Remove(offer);
        faction.Grievances.Add($"Navigator refused our offer for {offer.RouteId}.");
        Rebuild();
    }

    void Rebuild()
    {
        var allOffers = new List<(Faction faction, FactionOffer offer)>();
        foreach (var faction in GameManager.Instance.Factions.Factions.Values)
            foreach (var o in faction.PendingOffers)
                allOffers.Add((faction, o));

        if (allOffers.Count == 0) { gameObject.SetActive(false); return; }
        BuildRows(allOffers);
    }
}
