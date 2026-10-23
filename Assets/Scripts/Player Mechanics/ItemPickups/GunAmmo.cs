using System;
using System.Collections;
using UnityEngine;

// Attach to the same GameObject as Gun.
public class GunAmmo : MonoBehaviour
{
    [Min(1)] public int maxAmmo = 12;
    [Min(0)] public int currentAmmo = 6;
    public ItemDefinition ammoItem;
    public InventoryController inventoryController;
    [Min(0.01f)] public float reloadTime = 2f;

    public bool IsReloading { get; private set; }
    public event Action AmmoChanged;

    private Coroutine reloadRoutine;
    private bool shootingEnabled = true;

    void Awake()
    {
        maxAmmo = Mathf.Max(1, maxAmmo);
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
    }

    void Start()
    {
        if (inventoryController == null)
            inventoryController = FindAnyObjectByType<InventoryController>();

        if (ammoItem == null)
            Debug.LogWarning("GunAmmo: assign the shotgun ammo ItemDefinition to enable reloading.", this);
    }

    void Update()
    {
        // Also retries if spare ammo is picked up after the gun becomes empty.
        if (shootingEnabled && Time.timeScale > 0f && currentAmmo == 0 && !IsReloading)
            TryStartReload();
    }

    public void SetShootingEnabled(bool enabled)
    {
        shootingEnabled = enabled;
    }

    public bool TryUseRound()
    {
        if (IsReloading || currentAmmo <= 0)
            return false;

        currentAmmo--;
        AmmoChanged?.Invoke();
        return true;
    }

    public bool Uses(ItemDefinition item)
    {
        return ammoItem == null || ammoItem == item;
    }

    // Kept for compatibility with other scripts that explicitly load this gun.
    // AmmoPickup now stores spare ammo in the inventory instead.
    public int AddAmmo(int amount)
    {
        if (amount <= 0)
            return amount;

        int loaded = Mathf.Min(amount, Mathf.Max(0, maxAmmo - currentAmmo));
        currentAmmo += loaded;
        if (loaded > 0)
            AmmoChanged?.Invoke();
        return amount - loaded;
    }

    public int GetReserveAmmo()
    {
        if (ammoItem == null || inventoryController == null || inventoryController.Inventory == null)
            return 0;

        int total = 0;
        foreach (ItemInstance stack in inventoryController.Inventory.Items)
            if (stack.def == ammoItem)
                total += Mathf.Max(0, stack.stackQuantity);
        return total;
    }

    public bool TryStartReload()
    {
        if (!isActiveAndEnabled || !shootingEnabled || IsReloading ||
            currentAmmo >= maxAmmo || GetReserveAmmo() == 0)
            return false;

        IsReloading = true;
        reloadRoutine = StartCoroutine(Reload());
        return true;
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, reloadTime));

        // Read the live inventory now, so moving/dropping ammo during the delay
        // cannot create free shells or consume a stale item reference.
        int loaded = 0;
        if (ammoItem != null && inventoryController != null && inventoryController.Inventory != null)
        {
            Inventory inventory = inventoryController.Inventory;
            for (int i = inventory.Items.Count - 1; i >= 0 && currentAmmo + loaded < maxAmmo; i--)
            {
                ItemInstance stack = inventory.Items[i];
                if (stack.def != ammoItem || stack.stackQuantity <= 0)
                    continue;

                int used = Mathf.Min(maxAmmo - currentAmmo - loaded, stack.stackQuantity);
                stack.stackQuantity -= used;
                loaded += used;
                if (stack.stackQuantity == 0)
                    inventory.RemoveItem(stack);
            }
        }

        currentAmmo += loaded;
        IsReloading = false;
        reloadRoutine = null;
        AmmoChanged?.Invoke();

        if (loaded > 0)
        {
            InventoryUI ui = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
            if (ui != null)
                ui.RefreshItems();
        }
    }

    void OnDisable()
    {
        if (reloadRoutine != null)
            StopCoroutine(reloadRoutine);
        reloadRoutine = null;
        IsReloading = false;
    }
}

