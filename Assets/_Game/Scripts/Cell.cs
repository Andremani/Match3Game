using Pieces;
using UnityEngine;

public class Cell
{
    public Vector2Int Coordinate { get; set; }
    public Piece Piece { get; set; }

    public Cell(Vector2Int coordinates)
    {
        this.Coordinate = coordinates;
    }
}