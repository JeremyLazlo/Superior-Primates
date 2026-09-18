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
}
