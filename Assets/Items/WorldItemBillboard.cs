
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WorldItemBillboard : MonoBehaviour
{
    [Header("Billboard Settings")]
    [SerializeField] private Camera targetCamera;

    [Min(0.01f)]
    [SerializeField] private float rotationSpeed = 12f;

    private SpriteRenderer spriteRenderer;
    private WorldItemPickup worldItemPickup;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        worldItemPickup = GetComponentInParent<WorldItemPickup>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (worldItemPickup != null &&
            worldItemPickup.ItemDefinition != null)
        {
            spriteRenderer.sprite =
                worldItemPickup.ItemDefinition.itemIcon;
        }

        if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning(
                $"No item sprite assigned to {gameObject.name}.",
                gameObject
            );
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        // Keep the sprite parallel to the camera's view plane.
        Quaternion targetRotation = targetCamera.transform.rotation;

        // Smoothly follow camera rotation.
        transform.rotation = targetRotation;
    }
}
