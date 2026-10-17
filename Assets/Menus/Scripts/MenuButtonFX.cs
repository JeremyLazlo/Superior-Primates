using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Hover effect for menu buttons: the label slides right, stutters for a moment and brightens,
// while the Button's highlight color shows as a red bar that fades out to the right.
// Uses unscaled time, so it still animates while the game is paused.
public class MenuButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float slide = 18f;
    [SerializeField] private Color hoverTextColor = Color.white;
    [SerializeField] private float speed = 14f;

    [Tooltip("Gives the button's background a bar that fades out to the right (only if it has no sprite).")]
    [SerializeField] private bool fadingBar = true;

    private static Sprite barSprite;

    private TMP_Text label;
    private RectTransform labelRect;
    private Vector2 labelHome;
    private Color normalTextColor;
    private float offset;
    private float stutterUntil;
    private bool hovered;

    private void Awake()
    {
        label = GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            labelRect = label.rectTransform;
            labelHome = labelRect.anchoredPosition;
            normalTextColor = label.color;
        }

        Image background = GetComponent<Image>();
        if (fadingBar && background != null && background.sprite == null)
        {
            background.sprite = BarSprite();
        }
    }

    // Snap back if the panel is hidden while the mouse is over the button
    private void OnDisable()
    {
        hovered = false;
        offset = 0f;
        if (label != null)
        {
            labelRect.anchoredPosition = labelHome;
            label.color = normalTextColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        stutterUntil = Time.unscaledTime + 0.12f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

    private void Update()
    {
        if (label == null)
        {
            return;
        }

        float blend = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);

        offset = Mathf.Lerp(offset, hovered ? slide : 0f, blend);
        labelRect.anchoredPosition = labelHome + new Vector2(offset, 0f);

        Color target = hovered ? hoverTextColor : normalTextColor;
        Color c = Color.Lerp(label.color, target, blend);
        c.a = target.a;
        if (hovered && GameSettings.HorrorEffects && Time.unscaledTime < stutterUntil && Random.value < 0.5f)
        {
            c.a *= 0.25f; // quick flicker as the button lights up
        }
        label.color = c;
    }

    // Solid on the left, fading to nothing on the right
    private static Sprite BarSprite()
    {
        if (barSprite == null)
        {
            const int width = 256;
            Texture2D texture = new Texture2D(width, 1, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.HideAndDontSave;
            for (int x = 0; x < width; x++)
            {
                float u = x / (width - 1f);
                float alpha = u < 0.015f ? 1f : Mathf.Lerp(0.85f, 0f, Mathf.Pow(u, 0.8f));
                texture.SetPixel(x, 0, new Color(1f, 1f, 1f, alpha));
            }
            texture.Apply(false, true);

            barSprite = Sprite.Create(texture, new Rect(0, 0, width, 1), new Vector2(0.5f, 0.5f), 100f);
            barSprite.hideFlags = HideFlags.HideAndDontSave;
        }
        return barSprite;
    }
}
