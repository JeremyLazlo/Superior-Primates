using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private GameObject inventoryCellPrefab;
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private Transform gridTransform;
    [SerializeField] private Transform itemsTransform;
    [SerializeField] private float cellSize = 64f;





    private void Start()
    {
        Inventory inventory = inventoryController.Inventory;

        for (int y = 0; y < inventory.Height; y++)
        {
            for (int x = 0; x < inventory.Width; x++)
            {
                Instantiate(inventoryCellPrefab, gridTransform);
            }
        }

        foreach (ItemInstance item in inventory.Items)
        {
            GameObject itemObject = Instantiate(inventoryItemPrefab, itemsTransform);

            InventoryItemUI itemUI = itemObject.GetComponent<InventoryItemUI>();
            itemUI.SetItem(item);

            RectTransform itemRect = itemObject.GetComponent<RectTransform>();

            itemRect.anchoredPosition = new Vector2(
                item.inventoryPosition.x * cellSize,
                -item.inventoryPosition.y * cellSize
            );

            Vector2Int size = item.GetRotatedSize();

            itemRect.sizeDelta = new Vector2(
                size.x * cellSize,
                size.y * cellSize
            );
        }
    }

}


