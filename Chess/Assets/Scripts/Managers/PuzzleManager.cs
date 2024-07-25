using System;
using System.Collections;
using System.Collections.Generic;
using Unity_Helper.UnityHelper.Utilities;
using UnityEngine;
using UnityHelper.Templates;

public class PuzzleManager : SingletonMono<PuzzleManager>
{
    [SerializeField] private GameObject wrongSign;
    [SerializeField] private GameObject rightSign;
    [SerializeField] private GameObject puzzleWindow;


    // fires when the puzzle is restarting.
    public static event Action OnRestart;

    // holds array of all puzzle tiles that holds PuzzleData scripts.
    public PuzzleTile[] AllPuzzles { get; set; }
    public PuzzleData CurrentPuzzleData { get; private set; }



    private List<(Vector2Int origin, Vector2Int destination)> solution;
    private ChessBoard board;

    private bool hasWon = false;

    private int solutionIndex = 0;

    private Transform cameraHolder;

    private void Start()
    {
        ChessBoard.OnPieceMove = ValidateMove;
        board = FindAnyObjectByType<ChessBoard>();
        cameraHolder = Camera.main.transform.parent;
    }


    public ChessTeam GetPlayerTeam() => CurrentPuzzleData.PlayerTeam;

    private void ValidateMove(ChessPiece piece, Vector2Int newPosition)
    {
        if (board.IsHistoryBeingBrowsed)
            return;

        if (piece.team != CurrentPuzzleData.PlayerTeam)
            return;

        StartCoroutine(ValidationProcess(piece, newPosition));
    }

    private IEnumerator ValidationProcess(ChessPiece piece, Vector2Int newPosition)
    {
        var (origin, destination) = solution[solutionIndex++];

        if (piece.origin != origin || newPosition != destination)
        {
            WrongHandler(newPosition);
            yield break;
        }

        if (solutionIndex >= solution.Count)
        {
            WinHandler(newPosition);
            yield break;
        }

        yield return CoroutineUtils.WaitFor(0.25f);

        (origin, destination) = solution[solutionIndex++];
        var otherPiece = ChessBoard.GetPieceAt(origin);
        board.MovePieceTo(otherPiece, destination, true);
    }

    private void WrongHandler(Vector2Int piecePosition)
    {
        Debug.Log("Failed!");
        wrongSign.SetActive(true);
        Vector2 targetPosition = new Vector3(piecePosition.x + 0.45f, piecePosition.y + 0.45f);
        if (GetPlayerTeam() == ChessTeam.Black)
            targetPosition -= Vector2.one;

        wrongSign.transform.localPosition = targetPosition;
    }

    private void WinHandler(Vector2Int piecePosition)
    {
        StartCoroutine(Process());
        IEnumerator Process()
        {
            Debug.Log("You Won!!!");
            hasWon = true;
            board.enabled = false;

            rightSign.SetActive(true);

            Vector2 targetPosition = new Vector3(piecePosition.x + 0.45f, piecePosition.y + 0.45f);

            if (GetPlayerTeam() == ChessTeam.Black)
                targetPosition -= Vector2.one;

            rightSign.transform.SetLocalPositionAndRotation(targetPosition, cameraHolder.localRotation);
            UpdatePlayerSolvedPuzzles();

            yield return CoroutineUtils.WaitFor(2f);
            PostProcessingAnimation.Instance.Blur();
            puzzleWindow.SetActive(true);
        }
    }

    private void UpdatePlayerSolvedPuzzles()
    {
        if (!CurrentPuzzleData.IsSolvedByPlayer)
        {
            CurrentPuzzleData.SolvedBy++;
        }

        // adds the solved puzzle to the user's solved puzzles
        StartCoroutine(APIHandler.SendRequest("http://localhost:3000/api/users/addpuzzle", "PATCH", $"{{\"_id\": \"{CurrentPuzzleData.Id}\"}}"));

        // increment the puzzle's solvedBy field
        StartCoroutine(APIHandler.SendRequest("http://localhost:3000/api/puzzles", "PATCH", $"{{\"_id\": \"{CurrentPuzzleData.Id}\", \"solvedBy\": {CurrentPuzzleData.SolvedBy}}}"));
    }

    private void ClearCurrentPuzzle()
    {
        StopAllCoroutines();
        board.enabled = true;
        solutionIndex = 0;
        hasWon = false;

        rightSign.SetActive(false);
        wrongSign.SetActive(false);
    }

    public void Restart()
    {
        if (!hasWon)
        {
            board.UndoMove(true);
            board.RemoveLastMove();
            solutionIndex--;
            wrongSign.SetActive(false);
            return;
        }

        ClearCurrentPuzzle();
        OnRestart?.Invoke();
    }

    public void Setup(PuzzleData puzzleData)
    {
        ClearCurrentPuzzle();
        CurrentPuzzleData = puzzleData;
        solution = CurrentPuzzleData.Solution;
        board.SetupBoard(CurrentPuzzleData.PiecesPosition);
    }

    public PuzzleData GetPuzzle(int puzzleIndex)
    {
        var puzzleData = AllPuzzles[puzzleIndex].PuzzleData;
        Setup(puzzleData);
        return puzzleData;
    }
}
