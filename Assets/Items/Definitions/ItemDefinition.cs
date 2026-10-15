using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Items/ItemDefinition")]
public class ItemDefinition : ScriptableObject
{
    public string itemID;
    [Header("Visual")]
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;

    [Header("Inventory")]
    public Vector2Int itemSize = Vector2Int.one;
    public int itemStack;
}
