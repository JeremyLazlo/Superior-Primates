
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Key interactionKey = Key.E;
    [SerializeField] private TMP_Text promptText;

    [Header("Inventory")]
    [SerializeField] private InventoryController inventoryController;

    private WorldItemPickup currentPickup;

    private void Start()
    {
        HidePrompt();
    }

    private void Update()
    {
        if (playerCamera == null || inventoryController == null)
        {
            HidePrompt();
            return;
        }

        // Do not interact while the game is paused.
        if (Time.timeScale == 0f)
        {
            HidePrompt();
            return;
        }

        if (Keyboard.current == null)
        {
            HidePrompt();
            return;
        }

        UpdateTarget();

        if (currentPickup != null &&
            Keyboard.current[interactionKey].wasPressedThisFrame)
        {
            currentPickup.TryPickup(inventoryController);
        }
    }

    private void UpdateTarget()
    {
        currentPickup = null;

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            currentPickup =
                hit.collider.GetComponentInParent<WorldItemPickup>();
        }

        if (currentPickup != null)
        {
            ShowPrompt(currentPickup.ItemDefinition);
        }
        else
        {
            HidePrompt();
        }
    }

    private void ShowPrompt(ItemDefinition itemDefinition)
    {
        if (promptText == null)
            return;

        string itemName = itemDefinition != null
            ? itemDefinition.itemName
            : "Item";

        promptText.text = $"Press E to Pick Up {itemName}";
        promptText.gameObject.SetActive(true);
    }

    private void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }
}
