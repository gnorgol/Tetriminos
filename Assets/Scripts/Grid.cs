using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-1)]
public class Grid : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    public UIManager uiManager;


    /// <summary>
    /// Gets the bounds of the game board.
    /// </summary>
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    /// <summary>
    /// Initializes the tilemap and active piece.
    /// </summary>
    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    /// <summary>
    /// Spawns the first piece at the start of the game.
    /// </summary>
    private void Start()
    {
        SpawnPiece();
    }

    /// <summary>
    /// Spawns a new piece at the spawn position.
    /// </summary>
    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }
    }

    /// <summary>
    /// Ends the game and clears the tilemap.
    /// </summary>
    public void GameOver()
    {
        tilemap.ClearAllTiles();
        if (uiManager != null)
        {
            uiManager.ResetUI();
        }
    }

    /// <summary>
    /// Sets the tiles for the given piece on the tilemap.
    /// </summary>
    /// <param name="piece">The piece to set.</param>
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    /// <summary>
    /// Clears the tiles for the given piece from the tilemap.
    /// </summary>
    /// <param name="piece">The piece to clear.</param>
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    /// <summary>
    /// Checks if the given position is valid for the piece.
    /// </summary>
    /// <param name="piece">The piece to check.</param>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is valid, false otherwise.</returns>
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Clears full lines from the board and adds score.
    /// </summary>
    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }
        // Add score for cleared lines
        if (linesCleared > 0 && uiManager != null)
        {
            uiManager.AddLine(linesCleared);
            uiManager.AddScore(linesCleared * 100);
        }
    }

    /// <summary>
    /// Checks if a line is full.
    /// </summary>
    /// <param name="row">The row to check.</param>
    /// <returns>True if the line is full, false otherwise.</returns>
    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Clears a line and shifts the rows above down.
    /// </summary>
    /// <param name="row">The row to clear.</param>
    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}

