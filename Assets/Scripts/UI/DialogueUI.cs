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
        if (_historyScroll != null && _historyContainer != null)
            _historyScroll.content = _historyContainer as RectTransform;

        BootstrapInputField();

        _sendButton.onClick.AddListener(OnSend);
        _closeButton.onClick.AddListener(Close);
        _inputField.onSubmit.AddListener(_ => OnSend());
        EventBus.Subscribe<RelationshipChangedEvent>(OnBondChanged);
    }

    // TMP_InputField added via API has no child Text/Placeholder objects — create them here.
    void BootstrapInputField()
    {
        if (_inputField == null || _inputField.textComponent != null) return;

        var textAreaGO = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textAreaGO.transform.SetParent(_inputField.transform, false);
        var areaRT = textAreaGO.GetComponent<RectTransform>();
        areaRT.anchorMin = Vector2.zero;
        areaRT.anchorMax = Vector2.one;
        areaRT.offsetMin = new Vector2(10, 6);
        areaRT.offsetMax = new Vector2(-10, -6);

        var phGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        phGO.transform.SetParent(textAreaGO.transform, false);
        SetFullStretch(phGO.GetComponent<RectTransform>());
        var ph = phGO.GetComponent<TextMeshProUGUI>();
        ph.text = "Say something...";
        ph.color = new Color(.5f, .5f, .5f, .6f);
        ph.fontSize = 16;

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(textAreaGO.transform, false);
        SetFullStretch(textGO.GetComponent<RectTransform>());
        var inputText = textGO.GetComponent<TextMeshProUGUI>();
        inputText.color = Color.white;
        inputText.fontSize = 16;

        _inputField.textViewport = areaRT;
        _inputField.textComponent = inputText;
        _inputField.placeholder = ph;
    }

    static void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
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

        // Force bubble root to fill container width
        var bubbleRT = bubble.GetComponent<RectTransform>();
        bubbleRT.anchorMin = new Vector2(0, 0);
        bubbleRT.anchorMax = new Vector2(1, 0);
        bubbleRT.pivot     = new Vector2(0.5f, 1f);

        var img = bubble.GetComponent<Image>();
        if (img != null)
            img.color = isPlayer
                ? new Color(0.18f, 0.32f, 0.55f, 0.95f)
                : new Color(0.10f, 0.10f, 0.16f, 0.95f);

        // Force TMP child to fill bubble with padding
        var label = bubble.GetComponentInChildren<TextMeshProUGUI>();
        var labelRT = label.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(10, 8);
        labelRT.offsetMax = new Vector2(-10, -8);

        label.text = text;
        label.enableWordWrapping = true;
        label.overflowMode = TextOverflowModes.Overflow;
        label.color = Color.white;
        label.fontSize = 16;
        label.autoSizeTextContainer = false;

        // LayoutElement so VLG gives it preferred height
        var le = bubble.GetComponent<LayoutElement>() ?? bubble.AddComponent<LayoutElement>();
        le.flexibleWidth  = 1;
        le.preferredHeight = label.preferredHeight + 16f;

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
