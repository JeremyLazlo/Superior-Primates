using UnityEngine;

// Base class for anything the player picks up by walking into it.
// The object needs a collider with "Is Trigger" ticked (same idea as the Door triggers).
// To add a new item later: make a script that inherits from Pickup and fill in OnPickup.
public abstract class Pickup : MonoBehaviour
{
    // Return true if the pickup was used up (it then disappears),
    // or false to leave it on the ground (e.g. health is already full).
    protected abstract bool OnPickup(GameObject player);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && OnPickup(other.gameObject))
            Destroy(gameObject);
    }

    // Puts items into the inventory, topping up existing stacks first.
    // Returns how many didn't fit.
    protected int AddToInventory(ItemDefinition item, int amount)
    {
        InventoryController controller = FindAnyObjectByType<InventoryController>();
        if (controller == null || item == null)
            return amount;

        Inventory inventory = controller.Inventory;
        int maxStack = Mathf.Max(1, item.itemStack);

        foreach (ItemInstance stack in inventory.Items)
        {
            if (stack.def != item)
                continue;

            int added = Mathf.Min(amount, maxStack - stack.stackQuantity);
            if (added > 0)
            {
                stack.stackQuantity += added;
                amount -= added;
            }
        }

        while (amount > 0)
        {
            int added = Mathf.Min(amount, maxStack);
            if (!inventory.AddItem(new ItemInstance { def = item, stackQuantity = added }))
                break; // no room left

            amount -= added;
        }

        return amount;
    }
}
