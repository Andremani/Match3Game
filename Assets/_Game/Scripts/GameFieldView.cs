using UnityEngine;
using UnityEngine.Tilemaps;

public class GameFieldView : MonoBehaviour
{
    private const char EmptySymbol = '-';
    private const char EmptySymbolAlt = ' ';

    [SerializeField] private Tilemap maskTilemap;
    [SerializeField] private Tilemap cellsTilemap;
    [SerializeField] private TileBase maskTile;
    [SerializeField] private TileBase cellTile;
    [SerializeField] private Transform piecesParent;

    private int fieldWidth;
    private int fieldHeight;

    public Transform PiecesParent => piecesParent;
    public Vector2 CellSize => maskTilemap.cellSize;

    private void Awake()
    {
        if (piecesParent == null)
        {
            Debug.LogError("GameFieldView: piecesParent is not assigned.");
        }
    }

    public void PaintField(char[,] map)
    {
        maskTilemap.ClearAllTiles();
        cellsTilemap.ClearAllTiles();

        fieldWidth = map.GetLength(0);
        fieldHeight = map.GetLength(1);

        for (int y = 0; y < fieldHeight; y++)
        {
            for (int x = 0; x < fieldWidth; x++)
            {
                if (IsEmptySymbol(map[x, y])) continue;

                maskTilemap.SetTile(ToCellPosition(x, y), maskTile);
                cellsTilemap.SetTile(ToCellPosition(x, y), cellTile);
            }
        }

        maskTilemap.CompressBounds();
        cellsTilemap.CompressBounds();
    }

    public Vector3 GetWorldPosition(Vector2Int coordinate) => maskTilemap.GetCellCenterWorld(ToCellPosition(coordinate.x, coordinate.y));

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

    private Vector3Int ToCellPosition(int x, int y) => new(x, fieldHeight - 1 - y, 0);

    private static bool IsEmptySymbol(char symbol) => symbol == EmptySymbol || symbol == EmptySymbolAlt;
}
