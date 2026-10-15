using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private InventoryOverlay inventoryOverlay;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private Gun gun;

    private void Start()
    {
        inventoryUI.SetActive(false);
        inventoryOverlay.Hide();

        Time.timeScale = 1f;

        mouseLook.SetLookEnabled(true);
        gun.SetShootingEnabled(true);

        LockCursor();
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        bool opening = !inventoryUI.activeSelf;

        if (opening)
        {
            inventoryUI.SetActive(true);
            inventoryOverlay.Show();

            Time.timeScale = 0f;

            mouseLook.SetLookEnabled(false);
            gun.SetShootingEnabled(false);

            UnlockCursor();
        }
        else
        {
            inventoryUI.SetActive(false);
            inventoryOverlay.Hide();

            Time.timeScale = 1f;

            mouseLook.SetLookEnabled(true);
            gun.SetShootingEnabled(true);

            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
