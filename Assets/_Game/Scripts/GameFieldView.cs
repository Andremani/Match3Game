using System.Collections.Generic;
using Configuration;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameFieldView : MonoBehaviour
{
    [SerializeField] private Tilemap maskTilemap;
    [SerializeField] private Tilemap cellsTilemap;
    [SerializeField] private TileBase maskTile;
    [SerializeField] private TileBase cellTile;
    [SerializeField] private Transform piecesParent;

    private int fieldHeight;
    private int fieldWidth;

    public Transform PiecesParent => piecesParent;
    public Vector2 CellSize => maskTilemap.cellSize;

    public void PaintField(Dictionary<Vector2Int, Cell> cells, int fieldHeight, int fieldWidth)
    {
        this.fieldHeight = fieldHeight;
        this.fieldWidth = fieldWidth;

        maskTilemap.ClearAllTiles();
        cellsTilemap.ClearAllTiles();

        foreach (var cell in cells.Values)
        {
            maskTilemap.SetTile(ToCellPosition(cell.Coordinate), maskTile);
            cellsTilemap.SetTile(ToCellPosition(cell.Coordinate), cellTile);  
        }

        maskTilemap.CompressBounds();
        cellsTilemap.CompressBounds();
    }

    public Vector3 GetWorldPosition(Vector2Int coordinate) => maskTilemap.GetCellCenterWorld(ToCellPosition(coordinate));

    public void CenterCamera()
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            Debug.LogError("GameFieldView: no camera assigned and no MainCamera found in the scene, cannot center camera.");
            return;
        }

        BoundsInt cellBounds = maskTilemap.cellBounds;
        Vector3 fieldMin = maskTilemap.CellToWorld(new Vector3Int(cellBounds.xMin, cellBounds.yMin, 0));
        Vector3 fieldMax = maskTilemap.CellToWorld(new Vector3Int(cellBounds.xMax, cellBounds.yMax, 0));
        Vector3 fieldCenter = (fieldMin + fieldMax) * 0.5f;

        camera.transform.position = new Vector3(fieldCenter.x, fieldCenter.y, camera.transform.position.z);
    }

    private Vector3Int ToCellPosition(Vector2Int coordinates) => ToCellPosition(coordinates.x, coordinates.y);
    private Vector3Int ToCellPosition(int x, int y) => new(x, fieldHeight - 1 - y, 0);
}
