
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryItemUI : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private Image image;
    private ItemInstance item;
    private InventoryUI inventoryUI;
    private RectTransform itemRect;

    private Vector2 originalAnchoredPosition;
    private Vector2 dragOffset;

    private bool isDragging;

    private void Awake()
    {
        itemRect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        Debug.Log(
            $"InventoryItemUI Awake | " +
            $"GameObject: {gameObject.name} | " +
            $"RectTransform found: {itemRect != null} | " +
            $"Image found: {image != null}",
            this
        );
    }

    public void Initialize(ItemInstance item, InventoryUI inventoryUI)
    {
        if (image == null)
            image = GetComponentInChildren<Image>(true);

        if (item == null || item.def == null || image == null)
        {
            Debug.LogError(
                "InventoryItemUI could not initialize. " +
                "Check the item, definition, and Image component.",
                this
            );
            return;
        }

        this.item = item;
        this.inventoryUI = inventoryUI;

        image.sprite = item.def.itemIcon;
        image.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // Item selection and cell highlighting have been removed.
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (item == null || inventoryUI == null || itemRect == null)
            return;

        isDragging = true;
        inventoryUI.BeginItemDrag(item);

        originalAnchoredPosition = itemRect.anchoredPosition;

        RectTransform parentRect =
            itemRect.parent as RectTransform;

        if (parentRect != null &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointerPosition))
        {
            dragOffset = itemRect.anchoredPosition -
                         localPointerPosition;
        }
        else
        {
            dragOffset = Vector2.zero;
        }

        itemRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        RectTransform parentRect =
            itemRect.parent as RectTransform;

        if (parentRect == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointerPosition))
        {
            return;
        }

        // Calculate the proposed item position.
        Vector2 proposedPosition =
            localPointerPosition + dragOffset;

        // Convert the position to inventory grid coordinates.
        Vector2Int gridPosition =
            inventoryUI.GetGridPosition(proposedPosition);

        // Snap the item's top-left corner to the grid.
        itemRect.anchoredPosition = new Vector2(
            gridPosition.x * inventoryUI.CellSize,
            -gridPosition.y * inventoryUI.CellSize
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;
        inventoryUI.EndItemDrag(item);

        Vector2Int targetPosition =
            inventoryUI.GetGridPosition(itemRect.anchoredPosition);

        // Attempt to move the item in the actual inventory data.
        bool moved =
            inventoryUI.TryMoveItem(item, targetPosition);

        if (!moved)
        {
            // Invalid move: restore the original visual position.
            itemRect.anchoredPosition = originalAnchoredPosition;
        }
        else
        {
            // Successful move: refresh from inventory data.
            RefreshVisual(inventoryUI.CellSize);
        }
    }

    public void RefreshVisual(float cellSize)
    {
        // Ensure the component references are initialized.
        if (itemRect == null)
            itemRect = GetComponent<RectTransform>();

        if (image == null)
            image = GetComponent<Image>();

        if (item == null || item.def == null || itemRect == null)
        {
            Debug.LogError(
                "RefreshVisual failed: " +
                $"Item missing: {item == null}, " +
                $"Definition missing: {item == null || item.def == null}, " +
                $"RectTransform missing: {itemRect == null}",
                this
            );

            return;
        }

        Vector2Int size = item.GetRotatedSize();

        // Enforce the top-left coordinate convention.
        itemRect.anchorMin = new Vector2(0f, 1f);
        itemRect.anchorMax = new Vector2(0f, 1f);
        itemRect.pivot = new Vector2(0f, 1f);

        itemRect.sizeDelta = new Vector2(
            size.x * cellSize,
            size.y * cellSize
        );

        itemRect.anchoredPosition = new Vector2(
            item.inventoryPosition.x * cellSize,
            -item.inventoryPosition.y * cellSize
        );

        Debug.Log(
            $"RefreshVisual successful | " +
            $"Item: {item.def.itemName} | " +
            $"Inventory position: {item.inventoryPosition} | " +
            $"UI position: {itemRect.anchoredPosition} | " +
            $"Cell size: {cellSize}",
            this
        );
    }

    private void Update()
    {
        if (!isDragging)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            inventoryUI.RotateDraggedItem();
        }
    }

    public void RefreshSize(float cellSize)
    {
        if (item == null || itemRect == null)
            return;

        Vector2Int size = item.GetRotatedSize();

        itemRect.sizeDelta = new Vector2(
            size.x * cellSize,
            size.y * cellSize
        );
    }
}
