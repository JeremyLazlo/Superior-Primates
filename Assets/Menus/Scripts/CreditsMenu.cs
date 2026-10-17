using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Rolls the credits up the screen, built from a CreditsData asset.
// The mouse wheel scrolls by hand. Uses unscaled time, so it works even if the game is paused.
public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private CreditsData credits;

    [Tooltip("The visible area. Text outside it is hidden.")]
    [SerializeField] private RectTransform viewport;

    [Tooltip("The text inside the viewport that the credits are written into.")]
    [SerializeField] private TMP_Text creditsText;

    [Header("Scrolling")]
    [SerializeField] private float scrollSpeed = 70f;    // pixels per second
    [SerializeField] private float wheelStep = 90f;      // pixels per mouse wheel notch
    [SerializeField] private bool loop = true;

    [Header("Look")]
    [SerializeField] private Color headingColor = new Color(0.8f, 0.18f, 0.16f);
    [SerializeField] private Color mutedColor = new Color(0.62f, 0.6f, 0.56f);

    private float scrolled;     // how far the text has moved up, in pixels
    private float textHeight;

    private void OnEnable()
    {
        // Hides the text outside the viewport (added here so the scene doesn't need it)
        if (viewport != null && viewport.GetComponent<RectMask2D>() == null)
        {
            viewport.gameObject.AddComponent<RectMask2D>();
        }

        Rebuild();
        scrolled = 0f;
        MoveText();
    }

    private void Update()
    {
        if (creditsText == null || viewport == null)
        {
            return;
        }

        scrolled += scrollSpeed * Time.unscaledDeltaTime;

        float wheel = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        if (wheel != 0f)
        {
            scrolled -= Mathf.Sign(wheel) * wheelStep; // wheel up goes back
        }

        float end = textHeight + viewport.rect.height;
        if (scrolled > end)
        {
            scrolled = loop ? 0f : end;
        }
        scrolled = Mathf.Max(scrolled, 0f);

        MoveText();
    }

    // Right-click the component > Rebuild Credits Text to preview the credits in the editor
    [ContextMenu("Rebuild Credits Text")]
    public void Rebuild()
    {
        if (creditsText == null)
        {
            return;
        }

        creditsText.text = BuildText();

        float width = viewport != null ? viewport.rect.width : creditsText.rectTransform.rect.width;
        textHeight = creditsText.GetPreferredValues(creditsText.text, width, 0f).y;
        creditsText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
    }

    // scrolled = 0: the top of the text sits at the bottom of the viewport, then it moves up
    private void MoveText()
    {
        if (creditsText == null || viewport == null)
        {
            return;
        }

        RectTransform text = creditsText.rectTransform;
        text.anchoredPosition = new Vector2(text.anchoredPosition.x, scrolled - viewport.rect.height);
    }

    private string BuildText()
    {
        if (credits == null)
        {
            return "Drag a Credits asset into Credits Menu > Credits.";
        }

        string heading = "#" + ColorUtility.ToHtmlStringRGB(headingColor);
        string muted = "#" + ColorUtility.ToHtmlStringRGB(mutedColor);
        StringBuilder text = new StringBuilder();

        text.Append("<size=170%><b><cspace=0.12em>").Append((credits.title ?? "").ToUpper()).Append("</cspace></b></size>\n\n\n");

        foreach (CreditsData.Section section in credits.sections)
        {
            if (section == null)
            {
                continue;
            }

            text.Append("<size=75%><color=").Append(heading).Append("><cspace=0.3em>")
                .Append((section.heading ?? "").ToUpper()).Append("</cspace></color></size>\n");

            if (section.names != null)
            {
                foreach (string person in section.names)
                {
                    text.Append(person).Append('\n');
                }
            }

            text.Append("\n\n");
        }

        if (!string.IsNullOrEmpty(credits.closingLine))
        {
            text.Append("\n<color=").Append(muted).Append('>').Append(credits.closingLine).Append("</color>");
        }

        return text.ToString();
    }
}
