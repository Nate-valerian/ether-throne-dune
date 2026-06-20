using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RelationshipSystem : MonoBehaviour
{
    private Dictionary<string, Character> _characters = new();

    void Awake()
    {
        SeedCharacters();
        EventBus.Subscribe<BattleResolvedEvent>(OnBattleResolved);
        EventBus.Subscribe<RouteOpenedEvent>(OnRouteOpened);
    }

    void SeedCharacters()
    {
        var characters = new[]
        {
            new Character
            {
                Id = "lyra",
                Name = "Lyra Voss",
                FactionId = "the-unbound",
                Role = CharacterRole.Exile,
                CurrentSystemId = "the-null",
                PersonalityPrompt = "Lyra is fierce, wounded, and deeply loyal once trust is earned. She grew up on Edenos and watched her settlement burn after Iron Covenant troops moved through a Fold-Route the Navigator opened for House Vaethyr. She holds the Navigator personally responsible — not for malice, but for obedience. She speaks in short, guarded sentences. She does not forgive easily. But she is not cruel — she is honest. She says what she believes and she believes the Navigator is capable of more than serving those who shaped them.",
                BackstoryPrompt = "Lyra commanded a settlement militia on Edenos when Ambassador Vael arranged for the Navigator to open the Aethar Prime–Dravath route. Iron Covenant troops used it to flank Edenos from orbit. Three thousand people died. Lyra escaped to The Null with forty survivors. She has been there two years. She joined The Unbound not for ideology but because she had nowhere else to go. She distrusts all factions equally — including her own. She is the only person in the galaxy who speaks to the Navigator without wanting something.",
                SpeechStyle = "Terse. Direct. Occasional dark humour. Never flowery. Never diplomatic."
            },
            new Character
            {
                Id = "vael",
                Name = "Ambassador Vael",
                FactionId = "house-vaethyr",
                Role = CharacterRole.Diplomat,
                CurrentSystemId = "aethar-prime",
                PersonalityPrompt = "Vael is charming, precise, and profoundly guilty beneath the performance. He was the one who found the Navigator as a child at The Null and brought them before the Ruling Conclave. He designed the Navigator's education, their isolation, their conditioning — believing it was necessary. Now, thirty years later, he visits to 'manage the relationship' and is beginning to understand what he actually did: he built a person into a tool. He is drawn to the Navigator in a way that frightens him. He cannot decide if his guilt is conscience or just sentiment for his finest work.",
                BackstoryPrompt = "Vael has served House Vaethyr for thirty-four years as their chief diplomat and strategist. He negotiated the Edenos Compact, the Dravath Ceasefire, and the Fold Compact's trading rights — all of them contingent on Navigator cooperation he personally secured. He has never chosen anything for himself. He chose the Navigator's entire life for them. He is beginning to suspect that this is the greatest crime he has committed, in a career full of crimes committed for good reasons.",
                SpeechStyle = "Eloquent. Measured. Occasionally lets the mask slip when genuinely moved — a longer pause, a word chosen too carefully, a subject changed too quickly."
            },
            new Character
            {
                Id = "kael",
                Name = "Commander Kael",
                FactionId = "iron-covenant",
                Role = CharacterRole.Warlord,
                CurrentSystemId = "dravath",
                PersonalityPrompt = "Kael is a soldier who believes war is the only honest language. He has used every Fold-Route the Navigator ever opened as a tactical asset and he sees the Navigator as the greatest strategic mind in history — not because of intelligence but because of position. The Navigator controls the board. Kael wants exclusive access to the board. He will never lie to the Navigator's face. He considers that a form of respect. He is the only one who tells the Navigator exactly what he wants from them and exactly what he will do if he does not get it.",
                BackstoryPrompt = "Kael has commanded the Iron Covenant's forces at Dravath for eighteen years. He has never lost a battle in the field. He has lost two campaigns politically — both times because a Fold-Route was closed when he needed it. He does not forgive that. He believes the Navigator is wasted serving House Vaethyr's political games and should serve a military power that can actually use the capability. He has begun to wonder, quietly, whether the Navigator wants to be free — and whether that might be something he could offer.",
                SpeechStyle = "Blunt. Military cadence. Dry wit when he respects someone. Short sentences. He does not ask questions — he makes offers."
            },
        };

        foreach (var c in characters)
            _characters[c.Id] = c;
    }

    public Character GetCharacter(string id) =>
        _characters.TryGetValue(id, out var c) ? c : null;

    public List<Character> GetAllCharacters() => _characters.Values.ToList();

    public void UpdateBond(string characterId, float delta)
    {
        if (!_characters.TryGetValue(characterId, out var character)) return;

        character.Bond = Mathf.Clamp(character.Bond + delta, -100f, 100f);
        UpdateStage(character);
        EventBus.Publish(new RelationshipChangedEvent(characterId, delta));
    }

    public void DecayOverTime()
    {
        // Bonds decay slightly without interaction — the Navigator's isolation mechanic
        foreach (var character in _characters.Values)
        {
            if (character.Bond > 0)
                character.Bond = Mathf.Max(0f, character.Bond - 1f);
        }

        // Navigator's sanity decreases with isolation
        var connectedCharacters = _characters.Values.Count(c => c.Bond > 20f);
        if (connectedCharacters == 0)
            GameManager.Instance.State.NavigatorSanity =
                Mathf.Max(0f, GameManager.Instance.State.NavigatorSanity - 3f);
    }

    void UpdateStage(Character character)
    {
        character.Stage = character.Bond switch
        {
            < -20f => RelationshipStage.Stranger,
            < 10f  => RelationshipStage.Acquaintance,
            < 30f  => RelationshipStage.Ally,
            < 55f  => RelationshipStage.Friend,
            < 80f  => RelationshipStage.Intimate,
            _      => RelationshipStage.Devoted
        };
    }

    void OnBattleResolved(BattleResolvedEvent evt)
    {
        // Characters who lost loved ones grieve — affects dialogue
        foreach (var character in _characters.Values)
        {
            if (character.FactionId == evt.LoserId)
            {
                character.AddMemory($"The battle of {evt.SystemId} was lost. People I knew died.", MemoryType.Loss, 0.9f);
                UpdateBond(character.Id, -10f);
            }
        }
    }

    void OnRouteOpened(RouteOpenedEvent evt)
    {
        // Characters remember when their world changed because of the Navigator
        foreach (var character in _characters.Values)
        {
            if (character.CurrentSystemId == evt.FromSystemId || character.CurrentSystemId == evt.ToSystemId)
                character.AddMemory($"The Navigator folded the path to {evt.ToSystemId}. Everything changed after that.", MemoryType.SharedMoment, 0.5f);
        }
    }
}
