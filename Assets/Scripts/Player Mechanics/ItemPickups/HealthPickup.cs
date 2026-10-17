using UnityEngine;

// Health drop. Look at it and press E (PlayerInteraction handles the key and the prompt)
// to heal straight away. It never goes into the inventory: the Item Definition
// (Health Pack) only gives the prompt its name.
public class HealthPickup : WorldItemPickup
{
    [SerializeField] private int healAmount = 25;

    public override bool TryPickup(InventoryController inventoryController)
    {
        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth == null)
            return false;

        if (!playerHealth.Heal(healAmount))
        {
            Debug.Log("Health is already full, so " + name + " stays on the ground");
            return false;
        }

        Destroy(gameObject);
        return true;
    }
}
