using UnityEngine;

// Ammo drop. Look at it and press E (PlayerInteraction handles the key and the prompt).
// Loads the gun first. Only what doesn't fit goes into the inventory, as this pickup's
// Item Definition (e.g. AmmoShotgun). Anything that still doesn't fit stays on the ground.
public class AmmoPickup : WorldItemPickup
{
    [SerializeField] private int amount = 6;

    public override bool TryPickup(InventoryController inventoryController)
    {
        int left = amount;

        if (left > 0)
            left = AddToInventory(inventoryController, left);

        if (left == amount)
        {
            Debug.Log("No room for more ammo");
            return false;
        }

        Debug.Log("Picked up " + (amount - left) + " ammo");
        amount = left;

        if (amount == 0)
            Destroy(gameObject);

        return true;
    }

    // Tops up stacks of this ammo already in the inventory, then uses free space.
    // Returns how many didn't fit.
    private int AddToInventory(InventoryController inventoryController, int count)
    {
        if (inventoryController == null || ItemDefinition == null)
            return count;

        Inventory inventory = inventoryController.Inventory;
        int maxStack = Mathf.Max(1, ItemDefinition.itemStack);

        foreach (ItemInstance stack in inventory.Items)
        {
            if (stack.def != ItemDefinition)
                continue;

            int added = Mathf.Min(count, maxStack - stack.stackQuantity);
            if (added > 0)
            {
                stack.stackQuantity += added;
                count -= added;
            }
        }

        while (count > 0)
        {
            int added = Mathf.Min(count, maxStack);
            if (!inventory.AddItem(new ItemInstance { def = ItemDefinition, stackQuantity = added }))
                break; // inventory is full

            count -= added;
        }

        // Show the new stacks in the inventory screen, like InventoryController.TryAddItem does
        InventoryUI inventoryUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (inventoryUI != null)
            inventoryUI.RefreshItems();

        return count;
    }
}
