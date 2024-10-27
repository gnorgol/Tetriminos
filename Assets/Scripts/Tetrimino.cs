using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Enumeration of Tetromino types.
/// </summary>
public enum Tetromino
{
    I, J, L, O, S, T, Z
}

/// <summary>
/// Struct representing data for a Tetromino piece.
/// </summary>
[System.Serializable]
public struct TetrominoData
{
    /// <summary>
    /// The tile associated with the Tetromino.
    /// </summary>
    public Tile tile;

    /// <summary>
    /// The type of Tetromino.
    /// </summary>
    public Tetromino tetromino;

    /// <summary>
    /// The cells occupied by the Tetromino.
    /// </summary>
    public Vector2Int[] cells { get; private set; }

    /// <summary>
    /// The wall kick data for the Tetromino.
    /// </summary>
    public Vector2Int[,] wallKicks { get; private set; }

    /// <summary>
    /// Initializes the Tetromino data by setting the cells and wall kicks.
    /// </summary>
    public void Initialize()
    {
        cells = Data.Cells[tetromino];
        wallKicks = Data.WallKicks[tetromino];
    }
}
