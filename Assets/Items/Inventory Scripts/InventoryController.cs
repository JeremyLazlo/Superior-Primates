
using System;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private int width = 12;
    [SerializeField] private int height = 8;
    [SerializeField] private InventoryUI inventoryUI;

    private Inventory inventory;

    public Inventory Inventory => inventory;

    private void Awake()
    {
        inventory = new Inventory(width, height);
    }

    public bool TryAddItem(ItemDefinition itemDefinition)
    {
        if (itemDefinition == null)
        {
            Debug.LogWarning("Cannot add item: ItemDefinition is missing.");
            return false;
        }

        ItemInstance newItem = new ItemInstance
        {
            def = itemDefinition,
            inventoryRotation = Rotation.Rotation0,
            stackQuantity = 1
        };

        bool success = inventory.AddItem(newItem);

        if (success)
        {
            Debug.Log($"Added {itemDefinition.itemName} to inventory.");
            inventoryUI.RefreshItems();
        }
        else
        {
            Debug.Log($"Inventory is full. Could not add {itemDefinition.itemName}.");
        }

        return success;
    }

    //Returns true if item with name 'name' is found in inventory
    public bool FindItem(String name)
    {
        return inventory.HasItem(name);
    }
}
