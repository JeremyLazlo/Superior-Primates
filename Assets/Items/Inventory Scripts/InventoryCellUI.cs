
using UnityEngine;

public class InventoryCellUI : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    public void Initialize(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }
}
