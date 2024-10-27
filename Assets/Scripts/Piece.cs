using UnityEngine;

public class Piece : MonoBehaviour
{
    public Grid board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;

    public UIManager uiManager;

    /// <summary>
    /// Initializes the piece with the specified board, position, and tetromino data.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="position">The initial position of the piece.</param>
    /// <param name="data">The tetromino data.</param>
    public void Initialize(Grid board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        if (cells == null)
        {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    /// <summary>
    /// Updates the piece state every frame.
    /// </summary>
    private void Update()
    {
        board.Clear(this);

        // We use a timer to allow the player to make adjustments to the piece
        // before it locks in place
        lockTime += Time.deltaTime;

        // Handle rotation
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(1);
        }

        // Handle hard drop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        // Allow the player to hold movement keys but only after a move delay
        // so it does not move too fast
        if (Time.time > moveTime)
        {
            HandleMoveInputs();
        }

        // Advance the piece to the next row every stepDelay seconds
        if (Time.time > stepTime)
        {
            Step();
        }

        board.Set(this);
    }

    /// <summary>
    /// Handles the movement inputs for the piece.
    /// </summary>
    private void HandleMoveInputs()
    {
        // Soft drop movement
        if (Input.GetKey(KeyCode.S))
        {
            if (Move(Vector2Int.down))
            {
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
                IncrementDropScore();
            }
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.A))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Move(Vector2Int.right);
        }
    }

    private void IncrementDropScore()
    {
        if (uiManager != null)
        {
            uiManager.AddScore(1);
        }
    }

    /// <summary>
    /// Advances the piece to the next row.
    /// </summary>
    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (lockTime >= lockDelay)
        {
            Lock();
        }
    }

    /// <summary>
    /// Performs a hard drop of the piece.
    /// </summary>
    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            IncrementDropScore();
            continue;
        }

        Lock();
    }

    /// <summary>
    /// Locks the piece in place and spawns a new piece.
    /// </summary>
    private void Lock()
    {
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
    }

    /// <summary>
    /// Moves the piece by the specified translation.
    /// </summary>
    /// <param name="translation">The translation vector.</param>
    /// <returns>True if the move is valid, false otherwise.</returns>
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0f; // reset
        }

        return valid;
    }

    /// <summary>
    /// Rotates the piece by the specified direction.
    /// </summary>
    /// <param name="direction">The rotation direction.</param>
    private void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = rotationIndex;

        // Rotate all of the cells using a rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    /// <summary>
    /// Applies the rotation matrix to the piece cells.
    /// </summary>
    /// <param name="direction">The rotation direction.</param>
    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    /// <summary>
    /// Tests the wall kicks for the piece rotation.
    /// </summary>
    /// <param name="rotationIndex">The rotation index.</param>
    /// <param name="rotationDirection">The rotation direction.</param>
    /// <returns>True if a valid wall kick is found, false otherwise.</returns>
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the wall kick index for the piece rotation.
    /// </summary>
    /// <param name="rotationIndex">The rotation index.</param>
    /// <param name="rotationDirection">The rotation direction.</param>
    /// <returns>The wall kick index.</returns>
    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    /// <summary>
    /// Wraps the input value between the specified minimum and maximum values.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The wrapped value.</returns>
    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}
