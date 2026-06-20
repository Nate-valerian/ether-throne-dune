using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Side panel shown when the player clicks a star system on the map
public class SystemInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _systemName;
    [SerializeField] TextMeshProUGUI _systemType;
    [SerializeField] TextMeshProUGUI _controllingFaction;
    [SerializeField] TextMeshProUGUI _population;
    [SerializeField] TextMeshProUGUI _spiceYield;
    [SerializeField] TextMeshProUGUI _militaryStrength;
    [SerializeField] GameObject _isolatedWarning;
    [SerializeField] Transform _characterButtonContainer;
    [SerializeField] GameObject _characterButtonPrefab;
    [SerializeField] DialogueUI _dialogueUI;

    public void Show(StarSystem system)
    {
        gameObject.SetActive(true);

        _systemName.text = system.Name;
        _systemType.text = system.SystemType.ToString().ToUpper();
        _population.text = $"Pop. {system.Population:N0}";
        _spiceYield.text = $"Spice {system.SpiceYield:F0}";
        _militaryStrength.text = $"Military {system.MilitaryStrength:F0}";
        _isolatedWarning.SetActive(system.IsIsolated);

        var factions = GameManager.Instance.Factions.Factions;
        _controllingFaction.text = factions.TryGetValue(system.ControllingFactionId, out var f)
            ? f.Name
            : "Unclaimed";

        BuildCharacterButtons(system);
    }

    void BuildCharacterButtons(StarSystem system)
    {
        foreach (Transform child in _characterButtonContainer)
            Destroy(child.gameObject);

        foreach (var characterId in system.CharacterIds)
        {
            var character = GameManager.Instance.Relationships.GetCharacter(characterId);
            if (character == null || !character.IsAlive) continue;

            var btn = Instantiate(_characterButtonPrefab, _characterButtonContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = $"Speak to {character.Name}";

            var id = characterId;
            btn.GetComponent<Button>().onClick.AddListener(() => _dialogueUI.Open(id));
        }
    }

    public void Hide() => gameObject.SetActive(false);
}
