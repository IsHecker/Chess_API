using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityHelper.Utilities;

public class ChessBoard : MonoBehaviour
{
    [SerializeField] private GameObject[] whitePieces;
    [SerializeField] private GameObject[] blackPieces;

    [SerializeField] private GameObject whiteNotation;
    [SerializeField] private GameObject blackNotation;

    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform piecesParent;
    [SerializeField] private Transform tilesParent;

    [Header("Modifications")]
    [SerializeField] private bool puzzleBoard = true;
    [SerializeField] private bool modifyHistory = false;


    public static Action<ChessPiece, Vector2Int> OnPieceMove;
    public static Action<int> OnHistoryChanged;

    public List<ChessPieceMemento> MovesHistory { get; private set; } = new List<ChessPieceMemento>();
    public int HistoryIndex { get; private set; } = -1;
    public bool IsHistoryBeingBrowsed { get; private set; }

    private Transform tilesTransform;

    private Camera currentCamera;

    private ChessPiece draggedPiece;

    /// <summary>
    /// Holds Reference to the Pieces on the Board.
    /// </summary>
    private static readonly ChessPiece[,] chessPieces = new ChessPiece[8, 8];
    private List<Vector2Int> legalMoves = null;

    private ChessTeam teamTurn = ChessTeam.White;
    public ChessTeam PlayerTeam
    {
        get => puzzleBoard ? PuzzleManager.Instance.GetPlayerTeam() : teamTurn;
        set => teamTurn = value;
    }



    private void Awake()
    {
        tilesTransform = transform.GetChild(0);
        currentCamera = Camera.main;
        GenerateTiles();
        PuzzleManager.OnRestart += RestartPuzzle;
        //SetupPieces(new List<PieceData>()
        //{
        //    new PieceData('K', new Vector2Int(2,0), ChessTeam.White),
        //    new PieceData('Q', new Vector2Int(4,5), ChessTeam.White),
        //    new PieceData('N', new Vector2Int(3,2), ChessTeam.White),
        //    new PieceData('P', new Vector2Int(5,2), ChessTeam.White),
        //    new PieceData('P', new Vector2Int(5,6), ChessTeam.Black),
        //    new PieceData('B', new Vector2Int(4,3), ChessTeam.Black),
        //});
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            RotateBoard(ChessTeam.White);
        else if (Input.GetKeyDown(KeyCode.B))
            RotateBoard(ChessTeam.Black);

        // if in browsing history moves stop the board.
        if (IsHistoryBeingBrowsed && !modifyHistory)
            return;

        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, LayerMask.GetMask("Tile")))
        {
            if (!IsDraggingOrSelectingPiece(hitInfo.transform.gameObject.name))
                return;

            if (IsPieceReleasedOrPlayed(hitInfo))
                return;

            DragPiece();
        }
    }

    private bool IsDraggingOrSelectingPiece(string tileName)
    {
        if (draggedPiece == null && Input.GetMouseButtonDown(0))
        {
            var tilePosition = LookUpTilePosition(tileName);
            draggedPiece = GetPieceAt(tilePosition);

            if (draggedPiece == null)
                return false;

            draggedPiece.SpriteRenderer.sortingOrder = 1;
            ShowLegalMoves(draggedPiece);
        }
        return draggedPiece;
    }
    private bool IsPieceReleasedOrPlayed(RaycastHit hitInfo)
    {
        if (!Input.GetMouseButtonUp(0))
            return false;

        Vector2Int tilePosition = LookUpTilePosition(hitInfo.transform.gameObject.name);


        draggedPiece.SpriteRenderer.sortingOrder = 0;

        if (!IsInLegalMoves(legalMoves, tilePosition) || !MovePieceTo(draggedPiece, tilePosition, smooth: true))
        {
            draggedPiece.SetPosition(draggedPiece.origin, true);
            HideLegalMoves();
            draggedPiece = null;
            return true;
        }

        HideLegalMoves();
        draggedPiece = null;
        return true;
    }

    /// <summary>
    /// Returns true when the tile is empty or have a piece of different team.
    /// </summary>
    public static bool IsTileFree(ChessPiece currentPiece, Vector2Int tilePosition, bool withCapturing = true)
    {
        var pieceOnTile = GetPieceAt(tilePosition);
        return pieceOnTile == null || (withCapturing && pieceOnTile.team != currentPiece.team);
    }

    private void DragPiece()
    {
        Vector2 mousePosition = currentCamera.ScreenToWorldPoint(Input.mousePosition);
        draggedPiece.SetPosition(mousePosition, false);
    }
    private void GenerateTiles()
    {
        int y = 0;
        for (int x = 0; x < tilesTransform.childCount; x++)
        {
            if (x != 0 && x % 8 == 0)
            {
                y++;
            }
            var tile = tilesTransform.GetChild(x);
            tile.gameObject.layer = LayerMask.NameToLayer("Tile");
            tile.name = $"{x % 8},{y}";
        }
    }
    private void ShowLegalMoves(ChessPiece selectedPiece)
    {
        if (!IsPlayerTurn(selectedPiece))
            return;

        legalMoves = selectedPiece.GetLegalMoves(chessPieces);

        foreach (var move in legalMoves)
        {
            var tile = GetTileAt(move);

            if (GetPieceAt(new Vector2Int((int)tile.position.x, (int)tile.position.y)))
            {
                tile.GetChild(1).gameObject.SetActive(true);
                continue;
            }

            tile.GetChild(0).gameObject.SetActive(true);
        }
    }
    private void HideLegalMoves()
    {
        if (legalMoves == null)
            return;

        foreach (var move in legalMoves)
        {
            var tile = GetTileAt(move);

            tile.GetChild(0).gameObject.SetActive(false);
            tile.GetChild(1).gameObject.SetActive(false);

            tile.gameObject.layer = LayerMask.NameToLayer("Tile");
        }

        legalMoves.Clear();
    }

    // Operations
    private bool IsInLegalMoves(List<Vector2Int> moves, Vector2Int targetPosition)
    {
        return moves != null && moves.Contains(targetPosition);
    }

    public bool MovePieceTo(ChessPiece piece, Vector2Int targetTilePosition, bool smooth, bool addToHistory = true)
    {
        if (piece.origin == targetTilePosition || (HistoryIndex < -1 && !IsPlayerTurn(piece)))
            return false;

        if (!IsTileFree(piece, targetTilePosition))
            return false;

        // if there is a piece on the tile to capture!
        ChessPiece pieceToCapture = GetPieceAt(targetTilePosition);
        if (pieceToCapture)
            piece.Capture(pieceToCapture);

        piece.hasMoved = true;

        if (addToHistory)
            AddHistoryMove(piece, pieceToCapture);

        // fire event before actually update the piece position to new Position.
        OnPieceMove?.Invoke(piece, targetTilePosition);

        UpdatePiecePosition(targetTilePosition, piece, smooth);
        SwitchTeamTurn();

        return true;
    }

    private void AddHistoryMove(ChessPiece piece, ChessPiece capturedPiece)
    {
        CheckIfHistoryModified();

        var memento = new ChessPieceMemento(piece, piece.origin, capturedPiece);
        // moves stored as stack.
        MovesHistory.Insert(0, memento);
    }

    private void CheckIfHistoryModified()
    {
        if (!modifyHistory || HistoryIndex <= -1)
            return;

        MovesHistory.RemoveRange(0, HistoryIndex + 1);
        OnHistoryChanged?.Invoke(HistoryIndex);

        HistoryIndex = -1;
        IsHistoryBeingBrowsed = false;
    }

    public void UndoMove(bool smooth)
    {
        if (HistoryIndex >= MovesHistory.Count - 1)
            return;

        IsHistoryBeingBrowsed = true;

        PerformHistoryMove(MovesHistory[++HistoryIndex], true, smooth);
    }

    public void RedoMove(bool smooth)
    {
        if (HistoryIndex < 0)
            return;

        PerformHistoryMove(MovesHistory[HistoryIndex--], false, smooth);

        if (HistoryIndex < 0)
            IsHistoryBeingBrowsed = false;
    }

    public void UndoAllMoves(bool smooth)
    {
        while (HistoryIndex < MovesHistory.Count - 1)
        {
            UndoMove(smooth);
        }
    }

    private void PerformHistoryMove(ChessPieceMemento moveRecord, bool undo, bool smoothMove)
    {
        var piece = moveRecord.Piece;

        // holds position before preforming undo/redo moves.
        Vector2Int origin = moveRecord.Piece.origin;

        MovePieceTo(piece, moveRecord.Position, smoothMove, addToHistory: false);

        if (moveRecord.CapturedPiece != null)
        {
            var cp = moveRecord.CapturedPiece;
            if (undo)
            {
                piece.UnCapture(cp);
                chessPieces[cp.origin.x, cp.origin.y] = cp;
            }
            else
            {
                piece.Capture(cp);
                chessPieces[cp.origin.x, cp.origin.y] = piece;
            }
        }

        // update the lastPosition with the position of the piece before it has moved.
        moveRecord.Position = origin;
    }

    public void ClearHistory()
    {
        if (MovesHistory.Count == 0)
            return;

        MovesHistory.Clear();
        HistoryIndex = -1;
        IsHistoryBeingBrowsed = false;
    }

    public static bool CheckIfCoordsOnBoard(Vector2Int target)
    {
        return target.x >= 0 && target.y >= 0 && target.x < 8 && target.y < 8;
    }
    public static ChessPiece GetPieceAt(Vector2Int position)
    {
        return CheckIfCoordsOnBoard(position) ? chessPieces[position.x, position.y] : null;
    }

    /// <summary>
    /// Update the the Position of a piece on the grid and it's new origin.
    /// </summary>
    public static void UpdatePiecePosition(Vector2Int position, ChessPiece piece, bool smooth)
    {
        if (piece.origin != -Vector2Int.one)    // checks if the piece just has spawned.
            chessPieces[piece.origin.x, piece.origin.y] = null;

        chessPieces[position.x, position.y] = CheckIfCoordsOnBoard(position) ? piece : null;

        piece.SetOrigin(position, smooth);
    }

    public void RemovePiece(ChessPiece piece)
    {
        chessPieces[piece.origin.x, piece.origin.y] = null;
    }

    public Transform GetTileAt(Vector2Int position)
    {
        int x = position.x;
        int y = position.y * 8;
        return CheckIfCoordsOnBoard(position) ? tilesTransform.GetChild(x + y) : null;
    }

    /// <summary>
    /// Rotates the board relative to player team.
    /// </summary>
    public void RotateBoard(ChessTeam team)
    {
        if (team == ChessTeam.White)
        {
            whiteNotation.SetActive(true);
            blackNotation.SetActive(false);

            cameraHolder.SetLocalPositionAndRotation(new Vector3(6.3f, 3.5f, -10), Quaternion.identity);
        }
        else
        {

            blackNotation.SetActive(true);
            whiteNotation.SetActive(false);

            cameraHolder.SetLocalPositionAndRotation(new Vector3(0.7f, 3.5f, -10), new Quaternion(0f, 0f, 180f, 0f));
        }
        RotatePieces();
    }

    private void RotatePieces()
    {
        var rotation = cameraHolder.localRotation;
        for (int i = 0; i < piecesParent.childCount; i++)
        {
            piecesParent.GetChild(i).localRotation = rotation;
        }
    }

    public static Vector2Int LookUpTilePosition(string hitInfo)
    {
        int x = hitInfo[0] - '0';
        int y = hitInfo[2] - '0';
        return !string.IsNullOrEmpty(hitInfo) ? new Vector2Int(x, y) : -Vector2Int.one;
    }

    private bool IsPlayerTurn(ChessPiece piece)
    {
        return (!puzzleBoard && piece.team == teamTurn) || (piece.team == PlayerTeam && PlayerTeam == teamTurn);
    }

    private void SwitchTeamTurn()
    {
        teamTurn = teamTurn == ChessTeam.White ? ChessTeam.Black : ChessTeam.White;
    }

    public void SetupBoard(List<PieceData> piecesPositions)
    {
        if (piecesPositions == null)
            return;

        piecesParent.DeleteChildren();
        SpawnPiecesInPosition(piecesPositions, piecesParent);
        teamTurn = PlayerTeam;
        RotateBoard(PlayerTeam);
        ClearHistory();
    }

    public void SpawnPiecesInPosition(List<PieceData> piecesPositions, Transform piecesParent)
    {
        if (piecesPositions == null)
            return;

        foreach (var piece in piecesPositions)
        {
            SpawnPieceInPosition(piece, piecesParent);
        }
    }

    public ChessPiece SpawnPieceInPosition(PieceData pieceData, Transform piecesParent)
    {
        if (piecesParent == null)
            piecesParent = this.piecesParent;

        var piecePrefab = pieceData.team == ChessTeam.White ?
            whitePieces.Find(obj => obj.name[1] == pieceData.pieceName) :
            blackPieces.Find(obj => obj.name[1] == pieceData.pieceName);

        var piece = Instantiate(piecePrefab, piecesParent).GetComponent<ChessPiece>();
        piece.transform.localRotation = cameraHolder.transform.localRotation;
        UpdatePiecePosition(pieceData.position, piece, false);
        return piece;
    }

    public void RestartPuzzle()
    {
        UndoAllMoves(false);
        ClearHistory();
    }

    public void RemoveLastMove()
    {
        MovesHistory.RemoveAt(0);
        HistoryIndex = -1;
        IsHistoryBeingBrowsed = false;
    }
}