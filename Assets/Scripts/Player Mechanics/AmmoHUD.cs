using UnityEngine;
using UnityEngine.UI;

public class AmmoHUD : MonoBehaviour
{
    public GunAmmo gunAmmo;
    [Tooltip("One Image per shell, ordered left to right. Use the orange segment sprite on each Image.")]
    public Image[] segments;
    [Tooltip("Assign the included AmmoEmptySegment shader from the Project window.")]
    public Shader emptySegmentShader;

    private Material emptyMaterial;
    private Material[] originalMaterials;

    void Awake()
    {
        if (segments == null)
            segments = new Image[0];

        originalMaterials = new Material[segments.Length];
        for (int i = 0; i < segments.Length; i++)
            if (segments[i] != null)
                originalMaterials[i] = segments[i].material;

        if (emptySegmentShader != null)
            emptyMaterial = new Material(emptySegmentShader);
        else
            Debug.LogError("AmmoHUD: assign AmmoEmptySegment.shader to Empty Segment Shader.", this);
    }

    void OnEnable()
    {
        if (gunAmmo != null)
            gunAmmo.AmmoChanged += Refresh;
        Refresh();
    }

    void Start()
    {
        // All Awake calls have completed, including GunAmmo's ammo clamping.
        Refresh();
        if (gunAmmo != null && segments.Length != gunAmmo.maxAmmo)
            Debug.LogWarning("AmmoHUD: use one segment Image for each Max Ammo slot.", this);
    }

    public void Refresh()
    {
        if (gunAmmo == null || originalMaterials == null)
            return;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null)
                continue;

            // Full shells remain on the left; the rightmost orange shell empties first.
            segments[i].material = i < gunAmmo.currentAmmo || emptyMaterial == null
                ? originalMaterials[i] : emptyMaterial;
        }
    }

    void OnDisable()
    {
        if (gunAmmo != null)
            gunAmmo.AmmoChanged -= Refresh;
    }

    void OnDestroy()
    {
        if (originalMaterials != null)
            for (int i = 0; i < segments.Length; i++)
                if (segments[i] != null)
                    segments[i].material = originalMaterials[i];
        if (emptyMaterial != null)
            Destroy(emptyMaterial);
    }
}
