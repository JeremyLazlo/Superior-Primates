using UnityEngine;
using System.Collections.Generic;

public class Inventory
{
    private List<ItemInstance> items = new List<ItemInstance>();
    public IReadOnlyList<ItemInstance> Items => items;
    private ItemInstance[,] grid;
    public int Width => grid.GetLength(0);
    public int Height => grid.GetLength(1);


    public Inventory(int width, int height)
    {
        grid = new ItemInstance[width, height];
    }

    public ItemInstance GetPos(Vector2Int position)
    {
        return grid[position.x, position.y];
    }

    public bool CanPlace(ItemInstance item, Vector2Int position)
    {
        Vector2Int size = item.GetRotatedSize();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int gridPosition = position + new Vector2Int(x, y);
                if (gridPosition.x < 0 || gridPosition.x >= grid.GetLength(0))
                    return false;
                if (gridPosition.y < 0 || gridPosition.y >= grid.GetLength(1))
                    return false;
                if (grid[gridPosition.x, gridPosition.y] != null)
                    return false;
            }
        }
        return true;
    }
    public bool PlaceItem(ItemInstance item, Vector2Int position)
    {
        if (!CanPlace(item, position))
            return false;
        Vector2Int size = item.GetRotatedSize();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int gridPosition = position + new Vector2Int(x, y);
                grid[gridPosition.x, gridPosition.y] = item;
            }
        }
        item.inventoryPosition = position;
        items.Add(item);
        return true;
    }

    public bool AddItem(ItemInstance item)
    {
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                Vector2Int position = new Vector2Int(x, y);

                if (CanPlace(item, position))
                {
                    PlaceItem(item, position);
                    return true;
                }
            }
        }

        return false;
    }

    public void RemoveItem(ItemInstance item)
    {
        Vector2Int size = item.GetRotatedSize();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int gridPosition = item.inventoryPosition + new Vector2Int(x, y);

                if (gridPosition.x >= 0 && gridPosition.x < grid.GetLength(0) &&
                    gridPosition.y >= 0 && gridPosition.y < grid.GetLength(1))
                {
                    if (grid[gridPosition.x, gridPosition.y] == item)
                    {
                        grid[gridPosition.x, gridPosition.y] = null;
                    }
                }
                if (items.Contains(item))
                {
                    items.Remove(item);
                }
            }
        }
    }

    public bool RotateItem(ItemInstance item)
    {
        RemoveItem(item);

        item.Rotate();

        if (CanPlace(item, item.inventoryPosition))
        {
            PlaceItem(item, item.inventoryPosition);
            return true;
        }

        item.Rotate();
        item.Rotate();
        item.Rotate();

        PlaceItem(item, item.inventoryPosition);

        return false;
    }
}