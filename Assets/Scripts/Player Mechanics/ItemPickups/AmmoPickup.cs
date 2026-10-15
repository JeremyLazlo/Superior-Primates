using UnityEngine;

// Fills the gun first. Whatever doesn't fit goes into the inventory as Inventory Item.
// Anything that still doesn't fit stays on the ground.
public class AmmoPickup : Pickup
{
    public int amount = 6;
    public ItemDefinition inventoryItem; // e.g. AmmoShotgun

    protected override bool OnPickup(GameObject player)
    {
        GunAmmo gunAmmo = player.GetComponentInChildren<GunAmmo>();
        int left = gunAmmo != null ? gunAmmo.AddAmmo(amount) : amount;

        if (left > 0)
            left = AddToInventory(inventoryItem, left);

        if (left < amount)
            Debug.Log("Picked up " + (amount - left) + " ammo");

        amount = left;
        return left == 0;
    }
}
