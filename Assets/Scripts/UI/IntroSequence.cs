using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroSequence : MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField] CanvasGroup _overlay;       // full-screen black panel
    [SerializeField] TextMeshProUGUI _loreText;

    [Header("References")]
    [SerializeField] DialogueUI _dialogueUI;

    [Header("Timing")]
    [SerializeField] float _textFadeIn   = 1.5f;
    [SerializeField] float _textHold     = 3.5f;
    [SerializeField] float _textFadeOut  = 1.0f;
    [SerializeField] float _overlayFadeOut = 1.8f;

    const string LoreText =
        "The Navigator is the only being in the known galaxy who can perceive the fold between star systems.\n\n" +
        "The great houses have always needed one.\n\n" +
        "They have always found a way to keep one.";

    const string VaelOpening =
        "Navigator. You are expected at the Conclave briefing in one hour.\n\n" +
        "I thought I would come ahead of them — to remind you of what you are to say. " +
        "And what you are not.\n\n" +
        "You look tired. The folding takes something from you each time, doesn't it. " +
        "They don't like to hear that.";

    public IEnumerator Run()
    {
        // Beat 1 — lore crawl on black
        _overlay.alpha = 1f;
        _overlay.gameObject.SetActive(true);
        _loreText.alpha = 0f;
        _loreText.text = LoreText;

        yield return FadeGraphic(_loreText, 0f, 1f, _textFadeIn);
        yield return new WaitForSeconds(_textHold);
        yield return FadeGraphic(_loreText, 1f, 0f, _textFadeOut);

        // Beat 2 — Vael's opening; wait for player's one exchange
        var vael = GameManager.Instance.Relationships.GetCharacter("vael");
        bool done = false;
        _dialogueUI.OpenForIntro(vael, VaelOpening, () => done = true);
        yield return new WaitUntil(() => done);
        yield return new WaitForSeconds(1.8f);
        _dialogueUI.Close();

        // Beat 3 — fire game start, fade out overlay to reveal galaxy map
        EventBus.Publish(new GameStartedEvent());
        yield return new WaitForEndOfFrame(); // let GalaxyUI.BuildMap() run
        yield return FadeGroup(_overlay, 1f, 0f, _overlayFadeOut);
        _overlay.gameObject.SetActive(false);
    }

    IEnumerator FadeGraphic(Graphic g, float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            g.color = new Color(g.color.r, g.color.g, g.color.b, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        g.color = new Color(g.color.r, g.color.g, g.color.b, to);
    }

    IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}
