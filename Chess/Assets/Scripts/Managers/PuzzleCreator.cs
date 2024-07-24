using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityHelper.Templates;
using UnityHelper.Utilities;

public class PuzzleCreator : SingletonMono<PuzzleCreator>
{
    [SerializeField] private GameObject saveWindow;
    [SerializeField] private GameObject puzzlesWindow;
    [SerializeField] private GameObject pieceImage;


    [SerializeField] private Transform piecesParent;
    [SerializeField] private Transform whitePieces;
    [SerializeField] private Transform blackPieces;

    [SerializeField] private TMP_InputField puzzleNameInput;

    [SerializeField] private ToggleGroup difficultyRadio;
    [SerializeField] private ToggleGroup playerTeamRadio;

    public List<(Vector2Int, Vector2Int)> recordedSolution = new List<(Vector2Int, Vector2Int)>();
    public List<string> RECORDEDSOLUTION = new List<string>();
    public Dictionary<ChessPiece, Vector2Int> piecesInitialPositions = new Dictionary<ChessPiece, Vector2Int>();
    public List<ChessPiece> PIECES = new List<ChessPiece>();
    public List<Vector2Int> POSITIONS = new List<Vector2Int>();

    public bool IsNewPuzzle { get; set; } = true;
    public bool IsRecording { get; set; } = false;

    private ChessPiece DraggedPiece { get; set; } = null;
    private PuzzleData PuzzleData { get; set; } = null;

    private bool fillingHistory = false;
    //private GameObject draggedPiece;

    private string selectedPieceName = null;
    private Camera mainCamera;
    private ChessBoard board;


    private void Start()
    {
        PuzzleData = new PuzzleData();

        ChessBoard.OnPieceMove -= RecordMove;
        ChessBoard.OnPieceMove += RecordMove;
        ChessBoard.OnHistoryChanged = CheckIfHistoryChanged;


        mainCamera = UnityUtils.Camera;
        board = FindAnyObjectByType<ChessBoard>();
    }

    private void Update()
    {
        if (IsRecording)
            return;

        var mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        pieceImage.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);


        if (Input.GetMouseButtonUp(0))
            pieceImage.SetActive(false);


        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, LayerMask.GetMask("Tile")))
        {
            if (Input.GetMouseButtonUp(0))
                DropSelectedPiece(hitInfo);

            if (!DragPiece(hitInfo))
                return;

            if (DropPiece(hitInfo))
                return;

            DraggedPiece.SetPosition(mousePosition, false);
        }
    }

    private void RecordMove(ChessPiece piece, Vector2Int newPosition)
    {
        if (board.IsHistoryBeingBrowsed || fillingHistory)
            return;


        recordedSolution.Add((piece.origin, newPosition));
        RECORDEDSOLUTION.Add($"{piece.origin}{newPosition}");
    }

    /// <summary>
    /// removes the recored moves with the size of deleted moves from history.
    /// it's called when the history is modified.
    /// </summary>
    /// <param name="deletedRange">Deleted range from history of moves</param>
    private void CheckIfHistoryChanged(int deletedRange)
    {
        recordedSolution.RemoveRange(recordedSolution.Count - 1 - deletedRange, deletedRange + 1);
        RECORDEDSOLUTION.RemoveRange(RECORDEDSOLUTION.Count - 1 - deletedRange, deletedRange + 1);
    }

    /// <summary>
    /// destorys all the captured objects when the history is empty.
    /// Helps in filtering initial pieces positions from unnecessary pieces.
    /// </summary>
    private void DestroyAllCapturedPieces()
    {
        var tempList = new List<ChessPiece>(piecesInitialPositions.Keys);
        foreach (var piece in tempList)
        {
            if (!piece.IsCaputred)
                continue;

            piecesInitialPositions.Remove(piece);
            POSITIONS.RemoveAt(PIECES.IndexOf(piece));
            PIECES.Remove(piece);
            Destroy(piece.gameObject);
        }
    }

    private void UpdateInitialPositions()
    {
        foreach (var piece in piecesInitialPositions.Keys.ToList())
        {
            piecesInitialPositions[piece] = piece.origin;
        }
    }

    private void DropSelectedPiece(RaycastHit hitInfo)
    {
        if (selectedPieceName == null)
            return;

        Vector2Int tilePosition = ChessBoard.LookUpTilePosition(hitInfo.transform.gameObject.name);
        if (ChessBoard.IsTileFree(null, tilePosition, false))
        {
            ChessTeam color = selectedPieceName[0] == 'W' ? ChessTeam.White : ChessTeam.Black;
            char pieceName = selectedPieceName[1];
            board.PlayerTeam = PuzzleData.PlayerTeam;


            //// apply this process when IsHistoryBeingBrowsed is true to reduce unnecessary calculations.
            //if (board.IsHistoryBeingBrowsed)
            //{
            //    UpdateInitialPositions();
            //}

            board.SetupPieces(new List<PieceData>()
            {
                new PieceData(pieceName, tilePosition, color),
            }, true);

            board.ClearHistory();
            recordedSolution.Clear();
            RECORDEDSOLUTION.Clear();

            ChessPiece piece = piecesParent.GetChild(piecesParent.childCount - 1).GetComponent<ChessPiece>();
            if (piecesInitialPositions.TryAdd(piece, tilePosition))
            {
                PIECES.Add(piece);
                POSITIONS.Add(tilePosition);
            }

            DestroyAllCapturedPieces();
            UpdateInitialPositions();
        }
        selectedPieceName = null;
    }

    private bool DragPiece(RaycastHit hitInfo)
    {
        if (DraggedPiece == null && Input.GetMouseButtonDown(0))
        {
            var tilePosition = ChessBoard.LookUpTilePosition(hitInfo.transform.gameObject.name);
            DraggedPiece = ChessBoard.GetPieceAt(tilePosition);

            if (DraggedPiece == null)
                return false;

            DraggedPiece.SpriteRenderer.sortingOrder = 1;
        }

        return DraggedPiece;
    }

    private bool DropPiece(RaycastHit hitInfo)
    {
        if (!Input.GetMouseButtonUp(0))
            return false;

        Vector2Int tilePosition = ChessBoard.LookUpTilePosition(hitInfo.transform.gameObject.name);
        DraggedPiece.SpriteRenderer.sortingOrder = 0;


        if (DraggedPiece.origin == tilePosition || !ChessBoard.IsTileFree(null, tilePosition, false))
        {
            DraggedPiece.SetPosition(DraggedPiece.origin);
            DraggedPiece = null;
            return true;
        }

        board.ClearHistory();
        recordedSolution.Clear();
        RECORDEDSOLUTION.Clear();

        piecesInitialPositions[DraggedPiece] = tilePosition;
        POSITIONS[PIECES.IndexOf(DraggedPiece)] = tilePosition;

        ChessBoard.UpdatePiecePosition(tilePosition, DraggedPiece, false);
        DraggedPiece = null;

        DestroyAllCapturedPieces();
        UpdateInitialPositions();
        return true;
    }

    private void PiecesButtons(bool enable)
    {
        for (int i = 0; i < whitePieces.childCount; i++)
        {
            whitePieces.GetChild(i).GetComponent<Button>().interactable = enable;
            blackPieces.GetChild(i).GetComponent<Button>().interactable = enable;
        }
    }

    private void AddToPiecesPosition()
    {
        for (int i = 0; i < piecesParent.childCount; i++)
        {
            ChessPiece piece = piecesParent.GetChild(i).GetComponent<ChessPiece>();

            if (piecesInitialPositions.TryAdd(piece, piece.origin))
            {
                PIECES.Add(piece);
                POSITIONS.Add(piece.origin);
            }
        }
    }

    private void FillPositionsAndSolution()
    {
        JSONArray piecesPosition = new JSONArray();

        // preparing setupPosition
        foreach (var (piece, position) in piecesInitialPositions)
        {
            var team = piece.team;
            piecesPosition.Add(ChessNotation.ConvertToNotation(new PieceData(piece.name[1], position, team)));
        }

        PuzzleData.PiecesPosition = piecesPosition;
        // assigning recordedMoves to solution
        PuzzleData.Solution = ChessNotation.ConvertToNotation(recordedSolution);
    }

    private void FillHistoryMoves()
    {
        // fills the history moves with the solution(recordedMoves) retrived from DB.
        fillingHistory = true;
        foreach (var (origin, destination) in recordedSolution)
        {
            board.MovePieceTo(ChessBoard.GetPieceAt(origin), destination, true);
        }
        fillingHistory = false;
    }

    private void DisplayDataOnUI()
    {
        // name input field
        puzzleNameInput.text = PuzzleData.PuzzleName;

        // Difficulty Radio
        foreach (var item in difficultyRadio.transform)
        {
            Toggle radio = ((Transform)item).GetComponent<Toggle>();

            if (radio.name != PuzzleData.Difficulty)
                continue;

            radio.Select();
            break;
        }

        // Color Radio
        if (PuzzleData.PlayerTeam == ChessTeam.White)
            playerTeamRadio.transform.GetChild(0).GetComponent<Toggle>().Select();
        else
            playerTeamRadio.transform.GetChild(1).GetComponent<Toggle>().Select();
    }

    private void FlushAllData()
    {
        recordedSolution.Clear();
        RECORDEDSOLUTION.Clear();
        PIECES.Clear();
        POSITIONS.Clear();
        piecesInitialPositions.Clear();
        piecesParent.DeleteChildren();
    }

    private IEnumerator SaveOperation()
    {
        FillPositionsAndSolution();

        JSONObject body = new JSONObject();
        body.Add("name", puzzleNameInput.text);
        body.Add("difficulty", PuzzleData.Difficulty);
        body.Add("color", PuzzleData.PlayerTeam.ToString());
        body.Add("piecesPosition", PuzzleData.PiecesPosition);
        body.Add("solution", PuzzleData.Solution);
        body.Add("createdBy", User.Username);

        yield return APIHandler.SendRequest("http://localhost:3000/api/puzzles/", "POST",
            body.ToString());

        CloseSaveWindow();
        OpenPuzzlesWindow();
    }

    private IEnumerator UpdatePuzzle()
    {
        FillPositionsAndSolution();

        JSONObject body = new JSONObject();
        body.Add("_id", PuzzleData.Id);
        body.Add("name", puzzleNameInput.text);
        body.Add("difficulty", PuzzleData.Difficulty);
        body.Add("color", PuzzleData.PlayerTeam.ToString());
        body.Add("piecesPosition", PuzzleData.PiecesPosition);
        body.Add("solution", PuzzleData.Solution);

        yield return APIHandler.SendRequest("http://localhost:3000/api/puzzles/", "PATCH", body.ToString());

        CloseSaveWindow();
        OpenPuzzlesWindow();
    }



    public void InitBoard()
    {
        board.SetupPieces(new List<PieceData>()
        {
            new PieceData('K', new Vector2Int(4,0), ChessTeam.White),
            new PieceData('K', new Vector2Int(4,7), ChessTeam.Black)
        });

        AddToPiecesPosition();
    }

    public void Setup(PuzzleData puzzleData)
    {
        PuzzleData = puzzleData;

        IsNewPuzzle = false;
        board.enabled = false;
        board.PlayerTeam = puzzleData.PlayerTeam;
        DisplayDataOnUI();

        recordedSolution = ChessNotation.ParseSolution(puzzleData.Solution);
        foreach (var (origin, dest) in recordedSolution)
        {
            RECORDEDSOLUTION.Add($"{origin}{dest}");
        }

        board.SetupPieces(ChessNotation.ParseToPiecePosition(puzzleData.PiecesPosition));
        AddToPiecesPosition();
        FillHistoryMoves();
    }

    public void DragPiece(string pieceName)
    {
        if (IsRecording)
            return;


        pieceImage.SetActive(true);
        selectedPieceName = pieceName;

        var pieceSprite = UnityUtils.GetPressedUI().GetComponent<Image>().sprite;

        pieceImage.GetComponent<Image>().sprite = pieceSprite;
    }

    public void StartStopRecording()
    {
        IsRecording = !IsRecording;
        PiecesButtons(!IsRecording);

        board.enabled = IsRecording;

        UnityUtils.GetPressedUI().transform.GetChild(0).GetComponent<TMP_Text>().text = !IsRecording ? "Record Solution" : "Stop Recording";

        if (!IsRecording)
        {
            board.UndoAllMoves(true);
        }
    }

    public void SaveOrUpdate()
    {
        if (IsNewPuzzle)
        {
            if (puzzleNameInput.text == string.Empty || string.IsNullOrWhiteSpace(puzzleNameInput.text))
                return;

            StartCoroutine(SaveOperation());

            return;
        }

        StartCoroutine(UpdatePuzzle());
    }

    public void OpenPuzzlesWindow()
    {
        FlushAllData();
        puzzlesWindow.SetActive(true);
        board.enabled = false;
        PostProcessingAnimation.Instance.Blur();
    }

    public void SwitchPlayerTeam()
    {
        PuzzleData.PlayerTeam = ChessNotation.GetPlayerTeam(playerTeamRadio.GetFirstActiveToggle().name[0]);
        board.PlayerTeam = PuzzleData.PlayerTeam;
        board.RotateBoard(PuzzleData.PlayerTeam);
    }

    public void SwitchPuzzleDifficulty()
    {
        PuzzleData.Difficulty = difficultyRadio.GetFirstActiveToggle().name;
    }

    public void CloseSaveWindow()
    {
        saveWindow.SetActive(false);
        PostProcessingAnimation.Instance.UnBlur();
    }
}