using UnityEngine;

// Heals the player on contact. Stays on the ground if health is already full.
public class HealthPickup : Pickup
{
    public int healAmount = 25;

    protected override bool OnPickup(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        return playerHealth != null && playerHealth.Heal(healAmount);
    }
}
