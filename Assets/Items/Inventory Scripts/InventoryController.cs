using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private int width = 12;
    [SerializeField] private int height = 8;
    [SerializeField] private ItemDefinition testItemDefinition;

    private Inventory inventory;

    public Inventory Inventory => inventory;

    private void Awake()
    {
        inventory = new Inventory(width, height);
        ItemInstance testItem = new ItemInstance
        {
            def = testItemDefinition,
            stackQuantity = 1,
            inventoryRotation = Rotation.Rotation0
        };

        inventory.AddItem(testItem);
    }
}

