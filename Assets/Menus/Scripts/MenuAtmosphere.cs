using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Horror atmosphere drawn over a menu: drifting fog, a flickering ceiling light, a dark vignette and short dips in the light.
// Players can turn it off with "Horror Effects" in Options (GameSettings.HorrorEffects).
[RequireComponent(typeof(Canvas))]
public class MenuAtmosphere : MonoBehaviour
{
    [Tooltip("Tick for the in-game pause menu: only shows while the game is paused.")]
    [SerializeField] private bool onlyWhilePaused;

    [Header("Layers (0 = off)")]
    [SerializeField, Range(0f, 1f)] private float fog = 0.06f;
    [SerializeField, Range(0f, 1f)] private float lamp = 0.12f;
    [SerializeField, Range(0f, 1f)] private float vignette = 0.7f;

    [Header("Light")]
    [SerializeField] private Color lampColor = new Color(1f, 0.86f, 0.68f);

    [Tooltip("Where the light hangs: (0, 0) is the bottom-left of the screen, (1, 1) the top-right.")]
    [SerializeField] private Vector2 lampPosition = new Vector2(0.76f, 0.94f);

    [Tooltip("Seconds between flickers, picked at random between X and Y.")]
    [SerializeField] private Vector2 flickerEvery = new Vector2(6f, 14f);

    [Tooltip("How much the light drops when it flickers. 0 = no flicker.")]
    [SerializeField, Range(0f, 1f)] private float flickerDepth = 0.4f;

    [Tooltip("How dark the screen gets when the light drops.")]
    [SerializeField, Range(0f, 1f)] private float blackout = 0.25f;

    // 1 while the light is on, lower while it flickers. TextFlicker reads this to dim the title in sync.
    public static float LightLevel { get; private set; } = 1f;

    private Canvas canvas;
    private RawImage fogFar;
    private RawImage fogNear;
    private RawImage lampImage;
    private Image darkness;

    // Made once and kept for every menu that uses them
    private static Texture2D fogTexture;
    private static Texture2D glowTexture;
    private static Texture2D vignetteTexture;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        bool insideAnotherCanvas = transform.parent != null && transform.parent.GetComponentInParent<Canvas>() != null;
        if (insideAnotherCanvas)
        {
            canvas.overrideSorting = true; // use this canvas's Sort Order, so it draws over the menu
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        MakeTextures();

        // Children draw in order, so later layers are on top
        if (fog > 0f)
        {
            fogFar = Layer("Fog Far", fogTexture, new Color(0.7f, 0.72f, 0.76f, fog * 0.7f));
            fogNear = Layer("Fog Near", fogTexture, new Color(0.82f, 0.82f, 0.84f, fog));
        }

        if (lamp > 0f)
        {
            lampImage = Layer("Light", glowTexture, lampColor);
            RectTransform rect = lampImage.rectTransform;
            rect.anchorMin = lampPosition - new Vector2(0.38f, 0.62f);
            rect.anchorMax = lampPosition + new Vector2(0.38f, 0.62f);
        }

        if (vignette > 0f)
        {
            Layer("Vignette", vignetteTexture, new Color(0f, 0f, 0f, vignette));
        }

        GameObject dark = new GameObject("Blackout", typeof(RectTransform), typeof(Image));
        dark.transform.SetParent(transform, false);
        Stretch((RectTransform)dark.transform);
        darkness = dark.GetComponent<Image>();
        darkness.color = Color.clear;
        darkness.raycastTarget = false;
    }

    private void OnEnable()
    {
        StartCoroutine(Flicker());
    }

    private void OnDisable()
    {
        LightLevel = 1f;
    }

    private void Update()
    {
        bool show = GameSettings.HorrorEffects && (!onlyWhilePaused || PauseMenu.IsPaused);
        if (canvas.enabled != show)
        {
            canvas.enabled = show;
        }
        if (!show)
        {
            LightLevel = 1f;
            return;
        }

        float t = Time.unscaledTime;

        if (fogNear != null)
        {
            fogNear.uvRect = new Rect(t * 0.012f, t * 0.003f, 1.6f, 1f);
            fogFar.uvRect = new Rect(-t * 0.006f, 0.37f, 1f, 0.7f);
        }

        if (lampImage != null)
        {
            float hum = 0.88f + 0.12f * Mathf.PerlinNoise(t * 4f, 0.5f);
            Color c = lampColor;
            c.a = lamp * hum * LightLevel;
            lampImage.color = c;
        }

        darkness.color = new Color(0f, 0f, 0f, (1f - LightLevel) * blackout);
    }

    // The light stutters a few times, then sometimes dips for a moment
    private IEnumerator Flicker()
    {
        while (true)
        {
            LightLevel = 1f;
            yield return new WaitForSecondsRealtime(Random.Range(flickerEvery.x, flickerEvery.y));

            if (canvas == null || !canvas.enabled)
            {
                continue;
            }

            int blinks = Random.Range(1, 4);
            for (int i = 0; i < blinks; i++)
            {
                LightLevel = 1f - Random.Range(0.5f, 1f) * flickerDepth;
                yield return new WaitForSecondsRealtime(Random.Range(0.03f, 0.1f));
                LightLevel = 1f;
                yield return new WaitForSecondsRealtime(Random.Range(0.05f, 0.2f));
            }

            // Now and then the light dips for a little longer
            if (Random.value < 0.15f)
            {
                LightLevel = 1f - flickerDepth;
                yield return new WaitForSecondsRealtime(Random.Range(0.15f, 0.35f));
            }
        }
    }

    private RawImage Layer(string layerName, Texture texture, Color color)
    {
        GameObject go = new GameObject(layerName, typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(transform, false);
        Stretch((RectTransform)go.transform);

        RawImage image = go.GetComponent<RawImage>();
        image.texture = texture;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // ---------------------------------------------------------------- generated textures

    private static void MakeTextures()
    {
        if (fogTexture == null)
        {
            // Soft clouds that tile, so they can scroll forever
            fogTexture = MakeTexture(256, TextureWrapMode.Repeat, FilterMode.Bilinear, (u, v) =>
            {
                float n = Noise(u, v, 4, 11) * 0.55f + Noise(u, v, 8, 23) * 0.3f + Noise(u, v, 16, 37) * 0.15f;
                float a = Mathf.Clamp01((n - 0.32f) * 1.9f);
                return new Color(1f, 1f, 1f, a * a);
            });
        }

        if (glowTexture == null)
        {
            glowTexture = MakeTexture(128, TextureWrapMode.Clamp, FilterMode.Bilinear, (u, v) =>
            {
                float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
                return new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f));
            });
        }

        if (vignetteTexture == null)
        {
            vignetteTexture = MakeTexture(256, TextureWrapMode.Clamp, FilterMode.Bilinear, (u, v) =>
            {
                float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
                return new Color(0f, 0f, 0f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1.35f, d)));
            });
        }
    }

    private static Texture2D MakeTexture(int size, TextureWrapMode wrap, FilterMode filter, System.Func<float, float, Color> pixel)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = wrap;
        texture.filterMode = filter;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                pixels[y * size + x] = pixel((x + 0.5f) / size, (y + 0.5f) / size);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    // Smooth random values on a grid of cells x cells that wraps around, so the texture tiles
    public static float Noise(float u, float v, int cells, int seed)
    {
        float x = u * cells;
        float y = v * cells;
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);
        float fx = x - x0;
        float fy = y - y0;
        fx = fx * fx * (3f - 2f * fx);
        fy = fy * fy * (3f - 2f * fy);

        float top = Mathf.Lerp(Hash(x0, y0, cells, seed), Hash(x0 + 1, y0, cells, seed), fx);
        float bottom = Mathf.Lerp(Hash(x0, y0 + 1, cells, seed), Hash(x0 + 1, y0 + 1, cells, seed), fx);
        return Mathf.Lerp(top, bottom, fy);
    }

    // A random but repeatable value from 0 to 1 for each grid cell
    public static float Hash(int x, int y, int cells, int seed)
    {
        x = ((x % cells) + cells) % cells;
        y = ((y % cells) + cells) % cells;
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 144665);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / 16777215f;
        }
    }
}
