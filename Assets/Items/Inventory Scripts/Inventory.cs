
using UnityEngine;
using System.Collections.Generic;

public class Inventory
{
    private readonly List<ItemInstance> items = new List<ItemInstance>();
    public IReadOnlyList<ItemInstance> Items => items;

    private readonly ItemInstance[,] grid;

    public int Width => grid.GetLength(0);
    public int Height => grid.GetLength(1);

    public Inventory(int width, int height)
    {
        grid = new ItemInstance[width, height];
    }

    public ItemInstance GetPos(Vector2Int position)
    {
        if (!IsInsideGrid(position))
            return null;

        return grid[position.x, position.y];
    }

    private bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < Width &&
               position.y >= 0 && position.y < Height;
    }

    public bool CanPlace(
        ItemInstance item,
        Vector2Int position,
        ItemInstance ignoreItem = null)
    {
        if (item == null || item.def == null)
            return false;

        Vector2Int size = item.GetRotatedSize();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = position + new Vector2Int(x, y);

                if (!IsInsideGrid(cell))
                    return false;

                ItemInstance occupant = grid[cell.x, cell.y];

                if (occupant != null && occupant != ignoreItem)
                    return false;
            }
        }

        return true;
    }

    public bool PlaceItem(ItemInstance item, Vector2Int position)
    {
        if (item == null || items.Contains(item))
            return false;

        if (!CanPlace(item, position))
            return false;

        item.inventoryPosition = position;

        WriteCells(item);
        items.Add(item);

        return true;
    }

    public bool AddItem(ItemInstance item)
    {
        if (item == null || item.def == null || items.Contains(item))
            return false;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector2Int position = new Vector2Int(x, y);

                if (PlaceItem(item, position))
                    return true;
            }
        }

        return false;
    }

    private void WriteCells(ItemInstance item)
    {
        Vector2Int size = item.GetRotatedSize();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell =
                    item.inventoryPosition + new Vector2Int(x, y);

                grid[cell.x, cell.y] = item;
            }
        }
    }

    private void ClearCells(ItemInstance item)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] == item)
                    grid[x, y] = null;
            }
        }
    }

    public void RemoveItem(ItemInstance item)
    {
        if (item == null)
            return;

        ClearCells(item);
        items.Remove(item);
    }

    public bool MoveItem(ItemInstance item, Vector2Int newPosition)
    {
        if (item == null || !items.Contains(item))
            return false;

        // Check placement while ignoring this item's current cells.
        if (!CanPlace(item, newPosition, item))
            return false;

        ClearCells(item);

        item.inventoryPosition = newPosition;
        WriteCells(item);

        return true;
    }

    public bool RotateItem(ItemInstance item)
    {
        if (item == null || !items.Contains(item))
            return false;

        Rotation originalRotation = item.inventoryRotation;

        // Try the next 90-degree orientation.
        item.Rotate();

        if (CanPlace(item, item.inventoryPosition, item))
        {
            ClearCells(item);
            WriteCells(item);
            return true;
        }

        // Rotation is invalid; restore the original orientation.
        item.inventoryRotation = originalRotation;
        return false;
    }
}
