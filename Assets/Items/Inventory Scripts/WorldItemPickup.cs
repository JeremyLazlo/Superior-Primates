using UnityEngine;

public class WorldItemPickup : MonoBehaviour
{
    [SerializeField] private ItemDefinition itemDefinition;

    [Header("Weapon Unlock")]
    [SerializeField] private bool unlocksShotgun;
    [SerializeField] private ShotgunUnlockController shotgunUnlockController;

    public ItemDefinition ItemDefinition => itemDefinition;

    public virtual bool TryPickup(InventoryController inventoryController)
    {
        if (inventoryController == null)
        {
            Debug.LogWarning(
                "Cannot pick up item: InventoryController is missing."
            );
            return false;
        }

        if (itemDefinition == null)
        {
            Debug.LogWarning(
                $"Cannot pick up {gameObject.name}: No ItemDefinition assigned."
            );
            return false;
        }

        // Attempt to add the item to the inventory first.
        bool success = inventoryController.TryAddItem(itemDefinition);

        Debug.Log($"Pickup result for {gameObject.name}: {success}");

        if (!success)
            return false;

        // Unlock the shotgun only after a successful inventory addition.
        if (unlocksShotgun)
        {
            if (shotgunUnlockController == null)
            {
                shotgunUnlockController =
                    FindAnyObjectByType<ShotgunUnlockController>();
            }

            if (shotgunUnlockController != null)
            {
                shotgunUnlockController.UnlockShotgun();
            }
            else
            {
                Debug.LogError(
                    "Shotgun was added to inventory, but no " +
                    "ShotgunUnlockController was found in the scene."
                );
            }
        }

        Destroy(gameObject);
        return true;
    }
}