
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Grid mainBoard;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }

    /// <summary>  
    /// Initializes the tilemap and cells array.  
    /// </summary>  
    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector3Int[4];
    }

    /// <summary>  
    /// Updates the ghost piece position and appearance.  
    /// </summary>  
    private void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    /// <summary>  
    /// Clears the ghost piece from the tilemap.  
    /// </summary>  
    private void Clear()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    /// <summary>  
    /// Copies the cells from the tracking piece to the ghost piece.  
    /// </summary>  
    private void Copy()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = trackingPiece.cells[i];
        }
    }

    /// <summary>  
    /// Drops the ghost piece to the lowest valid position on the board.  
    /// </summary>  
    private void Drop()
    {
        Vector3Int position = trackingPiece.position;

        int current = position.y;
        int bottom = -mainBoard.boardSize.y / 2 - 1;

        mainBoard.Clear(trackingPiece);

        for (int row = current; row >= bottom; row--)
        {
            position.y = row;

            if (mainBoard.IsValidPosition(trackingPiece, position))
            {
                this.position = position;
            }
            else
            {
                break;
            }
        }

        mainBoard.Set(trackingPiece);
    }

    /// <summary>  
    /// Sets the ghost piece on the tilemap.  
    /// </summary>  
    private void Set()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, tile);
        }
    }
}
