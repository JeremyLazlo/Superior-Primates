using UnityEngine;



public enum Rotation
{
    Rotation0,
    Rotation90,
    Rotation180,
    Rotation270
}



public class ItemInstance
{
    public ItemDefinition def;
    public Rotation inventoryRotation;
    public Vector2Int inventoryPosition;
    public int stackQuantity;

    public Vector2Int GetRotatedSize()
    {
        if (inventoryRotation == Rotation.Rotation90 ||
            inventoryRotation == Rotation.Rotation270)
        {
            return new Vector2Int(def.itemSize.y, def.itemSize.x);
        }

        return def.itemSize;
    }

    public void Rotate()
    {
        inventoryRotation = (Rotation)(((int)inventoryRotation + 1) % 4);
    }
}





