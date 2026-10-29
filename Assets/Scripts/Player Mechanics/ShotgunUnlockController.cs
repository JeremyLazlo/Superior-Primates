using UnityEngine;

public class ShotgunUnlockController : MonoBehaviour
{
    [Header("Shotgun References")]
    [SerializeField] private Gun gun;
    [SerializeField] private GameObject shotgunVisual;

    private bool isUnlocked;

    private void Awake()
    {
        // The shotgun is unavailable at the start.
        isUnlocked = false;

        if (gun != null)
            gun.SetShootingEnabled(false);

        if (shotgunVisual != null)
            shotgunVisual.SetActive(false);
    }

    public void UnlockShotgun()
    {
        if (isUnlocked)
            return;

        isUnlocked = true;

        if (shotgunVisual != null)
            shotgunVisual.SetActive(true);

        if (gun != null)
            gun.SetShootingEnabled(true);

        Debug.Log("Shotgun unlocked!");
    }
}