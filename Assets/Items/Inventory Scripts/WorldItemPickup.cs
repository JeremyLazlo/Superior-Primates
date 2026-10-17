using UnityEngine;

public class WorldItemPickup : MonoBehaviour
{
    [SerializeField] private ItemDefinition itemDefinition;

    public ItemDefinition ItemDefinition => itemDefinition;

    public bool TryPickup(InventoryController inventoryController)
    {
        if (inventoryController == null)
        {
            Debug.LogWarning("Cannot pick up item: InventoryController is missing.");
            return false;
        }

        if (itemDefinition == null)
        {
            Debug.LogWarning(
                $"Cannot pick up {gameObject.name}: No ItemDefinition assigned."
            );
            return false;
        }

        bool success = inventoryController.TryAddItem(itemDefinition);
        Debug.Log($"Pickup result for {gameObject.name}: {success}");

        if (success)
        {
            Destroy(gameObject);
        }

        return success;
    }
}
