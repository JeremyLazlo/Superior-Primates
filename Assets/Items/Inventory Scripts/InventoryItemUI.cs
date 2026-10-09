using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetItem(ItemInstance item)
    {
        image.sprite = item.def.itemIcon;
    }
}
