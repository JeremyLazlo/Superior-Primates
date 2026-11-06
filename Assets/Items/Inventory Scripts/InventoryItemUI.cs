
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
        itemRect = transform as RectTransform;

        image = GetComponentInChildren<Image>(true);
    }

    public void Initialize(ItemInstance item, InventoryUI inventoryUI)
    {
        itemRect = GetComponent<RectTransform>();

        if (itemRect == null)
        {
            return;
        }

        if (image == null)
            image = GetComponentInChildren<Image>(true);

        if (item == null || item.def == null || image == null)
        {
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
        Vector2 proposedPosition =
            localPointerPosition + dragOffset;
        Vector2Int gridPosition =
            inventoryUI.GetGridPosition(proposedPosition);
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

        bool moved =
            inventoryUI.TryMoveItem(item, targetPosition);

        if (!moved)
        {
            itemRect.anchoredPosition = originalAnchoredPosition;
        }
        else
        {

            RefreshVisual(inventoryUI.CellSize);
        }
    }

    public void RefreshVisual(float cellSize)
    {

        if (item == null || itemRect == null || image == null)
            return;

        Vector2Int rotatedSize = item.GetRotatedSize();

        itemRect.sizeDelta = new Vector2(
            rotatedSize.x * cellSize,
            rotatedSize.y * cellSize
        );


        itemRect.anchoredPosition = new Vector2(
            item.inventoryPosition.x * cellSize,
            -item.inventoryPosition.y * cellSize
        );


        RectTransform imageRect = image.rectTransform;

        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;


        Vector2Int originalSize = item.def.itemSize;

        imageRect.sizeDelta = new Vector2(
            originalSize.x * cellSize,
            originalSize.y * cellSize
        );


        imageRect.localRotation = Quaternion.Euler(
            0f,
            0f,
            -(int)item.inventoryRotation * 90f
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
        if (item == null || itemRect == null || image == null)
            return;

        Vector2Int rotatedSize = item.GetRotatedSize();
        itemRect.sizeDelta = new Vector2(
            rotatedSize.x * cellSize,
            rotatedSize.y * cellSize
        );

        RectTransform imageRect = image.rectTransform;

        Vector2Int originalSize = item.def.itemSize;

        imageRect.sizeDelta = new Vector2(
            originalSize.x * cellSize,
            originalSize.y * cellSize
        );

        imageRect.localRotation = Quaternion.Euler(
            0f,
            0f,
            -(int)item.inventoryRotation * 90f
        );
    }
}
