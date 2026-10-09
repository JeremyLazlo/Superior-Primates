using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private int width = 12;
    [SerializeField] private int height = 8;

    private Inventory inventory;

    public Inventory Inventory => inventory;

    private void Awake()
    {
        inventory = new Inventory(width, height);
    }
}