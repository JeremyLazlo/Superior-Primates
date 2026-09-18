using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Items/ItemDefinition")]
public class ItemDefinition : ScriptableObject
{
    public string itemID;
    public string itemName;
    public string itemDescription;
    public Vector2Int itemSize = Vector2Int.one;
    public Sprite itemIcon;
    public int itemStack;
}
