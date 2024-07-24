using System;
using System.Collections;
using System.Collections.Generic;
using Unity_Helper.UnityHelper.Utilities;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private GameObject wrongSign;
    [SerializeField] private GameObject rightSign;


    // fires when the puzzle is restarting.
    public static event Action OnRestart;

    // holds array of all puzzle tiles that holds PuzzleData scripts.
    public static PuzzleTile[] AllPuzzles { get; set; }
    public static PuzzleData CurrentPuzzleData { get; private set; }



    private static List<(Vector2Int origin, Vector2Int destination)> solution;
    private static ChessBoard board;
    private static int solutionIndex = 0;



    private void Start()
    {
        ChessBoard.OnPieceMove = ValidateMove;
    }


    public static ChessTeam GetPlayerTeam() => CurrentPuzzleData.PlayerTeam;

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
        //if (piece.name[1] != pieceName || piece.origin != destination)
        //{
        //    WrongHandler(piece);
        //    yield break;
        //}

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

        yield return CoroutineUtils.WaitFor(0.5f);

        (origin, destination) = solution[solutionIndex++];
        var otherPiece = ChessBoard.GetPieceAt(origin);
        board.MovePieceTo(otherPiece, destination, true);
    }

    private void WrongHandler(Vector2Int piecePosition)
    {
        Debug.Log("Failed!");
        wrongSign.SetActive(true);
        wrongSign.transform.localPosition = new Vector3(piecePosition.x + 0.45f, piecePosition.y + 0.45f);
    }

    private void WinHandler(Vector2Int piecePosition)
    {
        Debug.Log("You Won!!!");
        board.enabled = false;

        rightSign.SetActive(true);
        rightSign.transform.localPosition = new Vector3(piecePosition.x + 0.45f, piecePosition.y + 0.45f);
        UpdateUserPuzzles();
    }

    private void UpdateUserPuzzles()
    {
        if (!CurrentPuzzleData.IsSolvedByPlayer)
        {
            CurrentPuzzleData.SolvedBy++;
        }

        StartCoroutine(APIHandler.SendRequest("http://localhost:3000/api/users/addpuzzle", "PATCH", 
            
            $"{{\"_id\": \"{CurrentPuzzleData.Id}\"}}"));
        StartCoroutine(APIHandler.SendRequest("http://localhost:3000/api/puzzles", "PATCH",  $"{{\"name\": \"{CurrentPuzzleData.PuzzleName}\", \"solvedBy\": {CurrentPuzzleData.SolvedBy}}}"));
    }

    private void FlushCurrentPuzzle()
    {
        wrongSign.SetActive(false);
        rightSign.SetActive(false);


    }

    public void Restart()
    {
        StopAllCoroutines();
        board.enabled = true;
        solutionIndex = 0;

        rightSign.SetActive(false);
        wrongSign.SetActive(false);

        OnRestart?.Invoke();
    }

    public static void Setup(PuzzleData puzzleData)
    {
        CurrentPuzzleData = puzzleData;
        var pieces = ChessNotation.ParseToPiecePosition(CurrentPuzzleData.PiecesPosition);
        solution = ChessNotation.ParseSolution(CurrentPuzzleData.Solution);

        FindObjectOfType<ChessBoard>().SetupPieces(pieces);
        board = FindAnyObjectByType<ChessBoard>();
    }

    public static PuzzleData GetPuzzle(int puzzleIndex)
    {
        var puzzleData = AllPuzzles[puzzleIndex].PuzzleData;
        Setup(puzzleData);
        return puzzleData;
    }
}
