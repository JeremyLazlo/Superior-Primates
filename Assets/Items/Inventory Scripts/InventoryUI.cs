
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private GameObject inventoryCellPrefab;
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private Transform gridTransform;
    [SerializeField] private Transform itemsTransform;
    [SerializeField] private float cellSize = 64f;

    private Inventory inventory;
    private InventoryCellUI[,] cells;

    private readonly Dictionary<ItemInstance, InventoryItemUI> itemUIs =
        new Dictionary<ItemInstance, InventoryItemUI>();

    private bool initialized;
    private ItemInstance itemBeingDragged;

    public float CellSize => cellSize;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (initialized)
            return;

        if (inventoryController == null)
        {
            Debug.LogError("InventoryUI is missing InventoryController.", this);
            return;
        }

        inventory = inventoryController.Inventory;

        GenerateGrid();
        initialized = true;

        RefreshItems();
    }

    private void GenerateGrid()
    {
        cells = new InventoryCellUI[
            inventory.Width,
            inventory.Height
        ];

        for (int y = 0; y < inventory.Height; y++)
        {
            for (int x = 0; x < inventory.Width; x++)
            {
                GameObject cellObject = Instantiate(
                    inventoryCellPrefab,
                    gridTransform
                );

                InventoryCellUI cellUI =
                    cellObject.GetComponent<InventoryCellUI>();

                if (cellUI == null)
                {
                    Debug.LogError(
                        "InventoryCell prefab is missing InventoryCellUI.",
                        cellObject
                    );
                    continue;
                }

                cellUI.Initialize(new Vector2Int(x, y));
                cells[x, y] = cellUI;
            }
        }
    }

    public void RefreshItems()
    {
        if (!initialized)
        {
            InitializeUI();
            return;
        }

        foreach (ItemInstance item in inventory.Items)
        {
            if (itemUIs.TryGetValue(item, out InventoryItemUI existingUI))
            {
                existingUI.RefreshVisual(cellSize);
                continue;
            }

            GameObject itemObject = Instantiate(
                inventoryItemPrefab,
                itemsTransform
            );

            InventoryItemUI itemUI =
                itemObject.GetComponent<InventoryItemUI>();

            if (itemUI == null)
            {
                Debug.LogError(
                    "Inventory item prefab is missing InventoryItemUI.",
                    itemObject
                );

                Destroy(itemObject);
                continue;
            }

            itemUI.Initialize(item, this);
            itemUI.RefreshVisual(cellSize);

            itemUIs.Add(item, itemUI);
        }

        List<ItemInstance> removedItems = new List<ItemInstance>();

        foreach (KeyValuePair<ItemInstance, InventoryItemUI> entry in itemUIs)
        {
            if (!inventory.Items.Contains(entry.Key))
            {
                Destroy(entry.Value.gameObject);
                removedItems.Add(entry.Key);
            }
        }

        foreach (ItemInstance item in removedItems)
        {
            itemUIs.Remove(item);
        }
    }

    public Vector2Int GetGridPosition(Vector2 anchoredPosition)
    {
        int x = Mathf.RoundToInt(anchoredPosition.x / cellSize);
        int y = Mathf.RoundToInt(-anchoredPosition.y / cellSize);

        return new Vector2Int(x, y);
    }

    public bool CanPlaceAt(ItemInstance item, Vector2Int position)
    {
        if (inventory == null || item == null)
            return false;

        bool canPlace = inventory.CanPlace(item, position, item);

        Debug.Log(
            $"Placement preview: {item.def.itemName} at {position} = {canPlace}"
        );

        return canPlace;
    }

    public bool TryMoveItem(ItemInstance item, Vector2Int position)
    {
        if (inventory == null || item == null)
            return false;

        bool success = inventory.MoveItem(item, position);

        if (success)
            RefreshItems();

        return success;
    }

    public void BeginItemDrag(ItemInstance item)
    {
        itemBeingDragged = item;
    }

    public void EndItemDrag(ItemInstance item)
    {
        if (itemBeingDragged == item)
            itemBeingDragged = null;
    }

    public void RotateDraggedItem()
    {
        if (inventory == null || itemBeingDragged == null)
            return;

        bool success = inventory.RotateItem(itemBeingDragged);

        if (success)
        {
            if (itemUIs.TryGetValue(
                itemBeingDragged,
                out InventoryItemUI itemUI))
            {
                // Update the dimensions without resetting
                // the item's current drag position.
                itemUI.RefreshSize(cellSize);
            }
        }
    }
}
