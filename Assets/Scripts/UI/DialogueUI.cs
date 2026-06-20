using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Attach to a Canvas panel with: CharacterPortrait, CharacterName, BondBar,
// DialogueText, InputField, SendButton, CloseButton, HistoryScroll
public class DialogueUI : MonoBehaviour
{
    [Header("Portrait")]
    [SerializeField] Image _portrait;
    [SerializeField] TextMeshProUGUI _characterName;
    [SerializeField] TextMeshProUGUI _stageBadge;

    [Header("Bond")]
    [SerializeField] Slider _bondBar;
    [SerializeField] TextMeshProUGUI _bondLabel;

    [Header("Dialogue")]
    [SerializeField] Transform _historyContainer;  // vertical layout group
    [SerializeField] GameObject _bubblePlayerPrefab;
    [SerializeField] GameObject _bubbleCharacterPrefab;
    [SerializeField] ScrollRect _historyScroll;

    [Header("Input")]
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] Button _sendButton;
    [SerializeField] Button _closeButton;

    [Header("State")]
    [SerializeField] GameObject _thinkingIndicator;  // "..." spinner

    private Character _activeCharacter;
    private bool _waiting;
    private bool _introMode;
    private System.Action _onIntroDone;

    void Awake()
    {
        _sendButton.onClick.AddListener(OnSend);
        _closeButton.onClick.AddListener(Close);
        _inputField.onSubmit.AddListener(_ => OnSend());
        EventBus.Subscribe<RelationshipChangedEvent>(OnBondChanged);
        gameObject.SetActive(false);
    }

    public void Open(string characterId)
    {
        var character = GameManager.Instance.Relationships.GetCharacter(characterId);
        if (character == null) return;

        _activeCharacter = character;
        LoadPortrait(characterId);
        Refresh();

        gameObject.SetActive(true);
        _inputField.Select();
        _inputField.ActivateInputField();

        EventBus.Publish(new CharacterMetEvent(characterId));
    }

    // Called by IntroSequence — shows a scripted opening line then waits for one LLM exchange.
    public void OpenForIntro(Character character, string openingLine, System.Action onExchangeDone)
    {
        _activeCharacter = character;
        _introMode = true;
        _onIntroDone = onExchangeDone;

        LoadPortrait(character.Id);
        Refresh();

        _closeButton.interactable = false;
        gameObject.SetActive(true);

        AddBubble(openingLine, isPlayer: false);

        _inputField.Select();
        _inputField.ActivateInputField();

        EventBus.Publish(new CharacterMetEvent(character.Id));
    }

    void LoadPortrait(string characterId)
    {
        if (_portrait == null) return;
        var sprite = Resources.Load<Sprite>($"Portraits/{characterId}");
        _portrait.sprite  = sprite;
        _portrait.enabled = sprite != null;
    }

    void Refresh()
    {
        _characterName.text = _activeCharacter.Name;
        _stageBadge.text = _activeCharacter.Stage.ToString().ToUpper();
        _bondBar.value = (_activeCharacter.Bond + 100f) / 200f; // normalise -100..100 → 0..1
        _bondLabel.text = $"Bond {_activeCharacter.Bond:+0;-0;0}";

        // Stage badge colour
        _stageBadge.color = _activeCharacter.Stage switch
        {
            RelationshipStage.Stranger    => new Color(.6f, .6f, .6f),
            RelationshipStage.Acquaintance=> new Color(.8f, .7f, .3f),
            RelationshipStage.Ally        => new Color(.3f, .7f, .9f),
            RelationshipStage.Friend      => new Color(.3f, .8f, .5f),
            RelationshipStage.Intimate    => new Color(.9f, .5f, .7f),
            RelationshipStage.Devoted     => new Color(1f, .85f, .2f),
            _                             => Color.white
        };
    }

    void OnSend()
    {
        if (_waiting || _activeCharacter == null) return;

        var text = _inputField.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        _inputField.text = "";
        AddBubble(text, isPlayer: true);
        SetWaiting(true);

        // Create an empty character bubble immediately so chunks stream into it
        var streamBubble = AddBubble("", isPlayer: false);
        var streamLabel  = streamBubble.GetComponentInChildren<TextMeshProUGUI>();

        bool wasIntro = _introMode;
        _introMode = false; // consume flag before async callback fires

        GameManager.Instance.Navigator.SpeakTo(
            _activeCharacter.Id,
            text,
            onDone: reply =>
            {
                SetWaiting(false);
                streamLabel.text = reply;
                Refresh();
                StartCoroutine(ScrollToBottom());

                if (wasIntro)
                {
                    // Lock input — IntroSequence will close the panel after its delay
                    _inputField.interactable = false;
                    _sendButton.interactable = false;
                    _onIntroDone?.Invoke();
                    _onIntroDone = null;
                }
            },
            onChunk: chunk =>
            {
                streamLabel.text += chunk;
                StartCoroutine(ScrollToBottom());
            });
    }

    GameObject AddBubble(string text, bool isPlayer)
    {
        var prefab = isPlayer ? _bubblePlayerPrefab : _bubbleCharacterPrefab;
        var bubble = Instantiate(prefab, _historyContainer);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = text;
        StartCoroutine(ScrollToBottom());
        return bubble;
    }

    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        _historyScroll.verticalNormalizedPosition = 0f;
    }

    void SetWaiting(bool waiting)
    {
        _waiting = waiting;
        _sendButton.interactable = !waiting;
        _inputField.interactable = !waiting;
        if (_thinkingIndicator != null)
            _thinkingIndicator.SetActive(waiting);
    }

    void OnBondChanged(RelationshipChangedEvent evt)
    {
        if (_activeCharacter != null && evt.CharacterId == _activeCharacter.Id)
            Refresh();
    }

    public void Close()
    {
        _activeCharacter = null;
        _introMode = false;
        _onIntroDone = null;
        _closeButton.interactable = true;
        _inputField.interactable = true;
        _sendButton.interactable = true;
        gameObject.SetActive(false);
    }
}
