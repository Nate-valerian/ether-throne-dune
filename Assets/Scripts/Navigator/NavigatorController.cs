using UnityEngine;

public class NavigatorController : MonoBehaviour
{
    void Awake()
    {
        EventBus.Subscribe<TurnAdvancedEvent>(OnTurnAdvanced);
    }

    // Open a route — core Navigator power
    public bool OpenRoute(string routeId, string targetFactionId = null)
    {
        var state = GameManager.Instance.State;
        var galaxy = GameManager.Instance.Galaxy;

        if (!galaxy.Routes.TryGetValue(routeId, out var route))
        {
            Debug.LogWarning($"Route {routeId} not found.");
            return false;
        }

        if (state.NavigatorInfluence < route.InfluenceCost)
        {
            Debug.Log("Not enough influence to open this route.");
            return false;
        }

        state.NavigatorInfluence -= route.InfluenceCost;
        if (targetFactionId != null)
            route.PrimaryBeneficiaryFactionId = targetFactionId;

        state.OpenRoutesThisTurn.Add(routeId);
        return galaxy.OpenRoute(routeId, state.NavigatorName);
    }

    // Close a route — can starve a planet or end a war
    public bool CloseRoute(string routeId)
    {
        return GameManager.Instance.Galaxy.CloseRoute(routeId);
    }

    // Speak to a character using the LLM engine
    public void SpeakTo(
        string characterId,
        string message,
        System.Action<string> onDone,
        System.Action<string> onChunk = null)
    {
        var character = GameManager.Instance.Relationships.GetCharacter(characterId);
        if (character == null)
        {
            Debug.LogWarning($"Character {characterId} not found.");
            return;
        }

        LLMService.Instance.GetCharacterResponse(character, message, onDone, onChunk);
    }

    void OnTurnAdvanced(TurnAdvancedEvent evt)
    {
        // Restore influence each turn
        var state = GameManager.Instance.State;
        state.NavigatorInfluence = Mathf.Min(100f, state.NavigatorInfluence + 20f);
        state.OpenRoutesThisTurn.Clear();
    }
}
