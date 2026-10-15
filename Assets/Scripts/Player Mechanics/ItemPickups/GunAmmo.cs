using UnityEngine;

// Gives the Gun limited ammo. Add it to the same object as the Gun.
// Without this component the gun keeps unlimited ammo, so nothing changes for anyone else.
public class GunAmmo : MonoBehaviour
{
    public int maxAmmo = 12;
    public int currentAmmo = 6;

    // Called from Gun.Fire(). Returns false when empty, which cancels the shot.
    public bool TryUseRound()
    {
        if (currentAmmo <= 0)
            return false;

        currentAmmo--;
        return true;
    }

    // Loads as much as fits in the gun. Returns how many were left over.
    public int AddAmmo(int amount)
    {
        int loaded = Mathf.Min(amount, Mathf.Max(0, maxAmmo - currentAmmo));
        currentAmmo += loaded;
        return amount - loaded;
    }
}
