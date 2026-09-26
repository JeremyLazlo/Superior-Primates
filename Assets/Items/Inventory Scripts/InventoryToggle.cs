using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }
}