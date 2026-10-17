using UnityEngine;

// Healing for PlayerHealth
// This works because PlayerHealth.cs is partial
public partial class PlayerHealth
{
    // Adds health, never above maxHealth. Returns false if health was already full.
    public bool Heal(int amount)
    {
        if (health >= maxHealth)
            return false;

        health = Mathf.Min(health + amount, maxHealth);
        Debug.Log("Player healed to " + health + "/" + maxHealth);
        return true;
    }
}
