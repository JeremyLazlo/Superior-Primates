using TMPro;
using UnityEngine;

// Makes a title look like it's printed on something old and lit by a failing light:
// the letters are worn, the text dims when MenuAtmosphere's light flickers, and now and then
// it glitches sideways with a red ghost behind it.
// Only runs in Play mode. Uses unscaled time, so it also works while the game is paused.
// Players can turn it off with "Horror Effects" in Options (GameSettings.HorrorEffects).
[RequireComponent(typeof(TMP_Text))]
public class TextFlicker : MonoBehaviour
{
    [Tooltip("How worn the letters look. 0 = clean.")]
    [SerializeField, Range(0f, 1f)] private float wear = 0.3f;

    [Tooltip("How much the text dims when the light flickers.")]
    [SerializeField, Range(0f, 1f)] private float dimWithLight = 0.5f;

    [Tooltip("Seconds between glitches, picked at random between X and Y.")]
    [SerializeField] private Vector2 glitchEvery = new Vector2(8f, 18f);

    [SerializeField] private Color ghostColor = new Color(0.8f, 0.05f, 0.05f, 0.9f);

    private static readonly int UnderlayColor = Shader.PropertyToID("_UnderlayColor");
    private static readonly int UnderlayOffsetX = Shader.PropertyToID("_UnderlayOffsetX");
    private static readonly int UnderlaySoftness = Shader.PropertyToID("_UnderlaySoftness");
    private static readonly int FaceTex = Shader.PropertyToID("_FaceTex");

    private TMP_Text text;
    private Material material;
    private Texture2D wornTexture;
    private Color homeColor;
    private Vector4 homeMargin;
    private Color shadowColor;
    private float shadowOffsetX;
    private float shadowSoftness;
    private float glitchUntil;
    private float nextGlitch;
    private bool glitching;
    private bool effectsOn;

    // Start rather than Awake, so the text component is fully set up first
    private void Start()
    {
        text = GetComponent<TMP_Text>();
        homeColor = text.color;
        homeMargin = text.margin;

        // A copy of the font material just for this text, so other text using the font is unchanged
        material = text.fontMaterial;
        material.EnableKeyword("UNDERLAY_ON");
        shadowColor = material.GetColor(UnderlayColor);
        shadowOffsetX = material.GetFloat(UnderlayOffsetX);
        shadowSoftness = material.GetFloat(UnderlaySoftness);

        if (wear > 0f)
        {
            wornTexture = MakeWornTexture(wear);
            material.SetTextureScale(FaceTex, new Vector2(3f, 1f));
            text.horizontalMapping = TextureMappingOptions.Paragraph;
            text.verticalMapping = TextureMappingOptions.Paragraph;
        }

        text.UpdateMeshPadding();
        SetEffects(GameSettings.HorrorEffects);
    }

    private void OnDisable()
    {
        if (material != null)
        {
            EndGlitch();
            text.color = homeColor;
        }
    }

    private void OnDestroy()
    {
        if (wornTexture != null)
        {
            Destroy(wornTexture);
        }
    }

    private void Update()
    {
        if (material == null)
        {
            return;
        }

        // Follow the Horror Effects option, even while it's being changed in Options
        if (effectsOn != GameSettings.HorrorEffects)
        {
            SetEffects(GameSettings.HorrorEffects);
        }
        if (!effectsOn)
        {
            return;
        }

        float t = Time.unscaledTime;

        // Dim along with the flickering light
        Color c = homeColor;
        c.a *= Mathf.Lerp(1f, MenuAtmosphere.LightLevel, dimWithLight);

        if (!glitching && t >= nextGlitch)
        {
            glitching = true;
            glitchUntil = t + Random.Range(0.12f, 0.35f);
        }

        if (glitching)
        {
            // Shake sideways with a red copy behind, and stutter
            text.margin = homeMargin + new Vector4(Random.Range(-8f, 8f), 0f, 0f, 0f);
            material.SetColor(UnderlayColor, ghostColor);
            material.SetFloat(UnderlayOffsetX, Random.Range(-0.6f, 0.6f));
            material.SetFloat(UnderlaySoftness, 0f);
            if (Random.value < 0.35f)
            {
                c.a *= 0.3f;
            }

            if (t >= glitchUntil)
            {
                EndGlitch();
                nextGlitch = t + Random.Range(glitchEvery.x, glitchEvery.y);
            }
        }

        if (text.color != c)
        {
            text.color = c;
        }
    }

    // On: worn letters, dimming and glitches. Off: plain text.
    private void SetEffects(bool on)
    {
        effectsOn = on;
        EndGlitch();
        text.color = homeColor;
        material.SetTexture(FaceTex, on && wornTexture != null ? wornTexture : Texture2D.whiteTexture);
        nextGlitch = Time.unscaledTime + Random.Range(2f, glitchEvery.y);
    }

    private void EndGlitch()
    {
        glitching = false;
        text.margin = homeMargin;
        if (material != null)
        {
            material.SetColor(UnderlayColor, shadowColor);
            material.SetFloat(UnderlayOffsetX, shadowOffsetX);
            material.SetFloat(UnderlaySoftness, shadowSoftness);
        }
    }

    // White with see-through blotches and scratches, laid over the letters so they look worn
    private static Texture2D MakeWornTexture(float strength)
    {
        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;
                float v = (y + 0.5f) / size;

                float blotch = MenuAtmosphere.Noise(u, v, 6, 7) * 0.6f + MenuAtmosphere.Noise(u, v, 24, 13) * 0.4f;
                float eaten = Mathf.Clamp01((blotch - 0.58f) * 4f);

                float speck = MenuAtmosphere.Hash(x, y, size, 29) > 0.985f ? 1f : 0f;

                // thin, slightly slanted scratches
                float line = MenuAtmosphere.Noise(u + v * 0.04f, 0.5f, 64, 53);
                float scratch = Mathf.Clamp01((line - 0.82f) * 10f) * MenuAtmosphere.Noise(u, v, 5, 61);

                float loss = Mathf.Clamp01(Mathf.Max(eaten, Mathf.Max(speck, scratch))) * strength;
                pixels[y * size + x] = new Color(1f, 1f, 1f, 1f - loss);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }
}
