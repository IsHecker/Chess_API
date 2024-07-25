using UnityEngine;
using TMPro;
using SimpleJSON;
using UnityEngine.UI;
public class UIDetailsManager : MonoBehaviour
{
    [SerializeField] private Button nextPuzzleButton;
    [SerializeField] private Button prevPuzzleButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private Button redoButton;

    [SerializeField] private TMP_Text puzzleName;
    [SerializeField] private TMP_Text creatorName;
    [SerializeField] private TMP_Text difficulty;
    [SerializeField] private TMP_Text date;
    [SerializeField] private TMP_Text solvedBy;

    [SerializeField] private GameObject puzzleWindow;

    private ChessBoard board;
    private int currentPuzzleIndex = 0;

    private void Start()
    {
        board = FindObjectOfType<ChessBoard>();
    }

    public void UndoMove()
    {
        board.UndoMove(true);

        //if (board.HistoryIndex >= board.MovesHistory.Count - 1)
        //{
        //    undoButton.interactable = false;
        //    return;
        //}

        //if (!redoButton.interactable)
        //    redoButton.interactable = true;
    }

    public void RedoMove()
    {
        board.RedoMove(true);

        //if (board.HistoryIndex < 0)
        //{
        //    redoButton.interactable = false;
        //    return;
        //}

        

        //if (!undoButton.interactable)
        //    undoButton.interactable = true;
    }

    public void Restart()
    {
        board.RestartPuzzle();
    }

    public void NextPuzzle()
    {
        prevPuzzleButton.interactable = true;

        SetData(PuzzleManager.Instance.GetPuzzle(++currentPuzzleIndex));

        if (currentPuzzleIndex >= PuzzleManager.Instance.AllPuzzles.Length - 1)
            nextPuzzleButton.interactable = false;
    }

    public void PrevPuzzle()
    {
        nextPuzzleButton.interactable = true;

        SetData(PuzzleManager.Instance.GetPuzzle(--currentPuzzleIndex));

        if (currentPuzzleIndex < 1)
            prevPuzzleButton.interactable = false;
    }

    public void OpenPuzzleWindow()
    {
        PostProcessingAnimation.Instance.Blur();
        board.enabled = false;
        puzzleWindow.SetActive(true);
    }

    public void SetData(PuzzleData puzzleData)
    {
        puzzleName.text = puzzleData.PuzzleName;
        difficulty.text = puzzleData.Difficulty;
        creatorName.text = puzzleData.CreatedBy;
        date.text = puzzleData.CreatedAt;
        solvedBy.text = "Solved By: " + puzzleData.SolvedBy;

        if (PuzzleManager.Instance.AllPuzzles.Length > 1)
            nextPuzzleButton.interactable = true;
    }
}
