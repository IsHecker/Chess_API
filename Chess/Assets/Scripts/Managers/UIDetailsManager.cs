using UnityEngine;
using TMPro;
using SimpleJSON;
using UnityEngine.UI;
public class UIDetailsManager : MonoBehaviour
{
    [SerializeField] private Button nextPuzzleButton;
    [SerializeField] private Button prevPuzzleButton;

    [SerializeField] private TMP_Text puzzleName;
    [SerializeField] private TMP_Text creatorName;
    [SerializeField] private TMP_Text difficulty;
    [SerializeField] private TMP_Text date;

    [SerializeField] private GameObject puzzleWindow;

    private ChessBoard board;
    private int currentPuzzleIndex = 0;

    private void Start()
    {
        board = FindObjectOfType<ChessBoard>();
    }

    public void UndoMove()
    {
        board.UndoMove();
    }

    public void RedoMove()
    {
        board.RedoMove();   
    }

    public void Restart()
    {
        board.RestartPuzzle();
    }

    public void NextPuzzle()
    {
        if (currentPuzzleIndex >= PuzzleManager.AllPuzzles.Length - 1)
        {
            nextPuzzleButton.interactable = false;
            return;
        }

        prevPuzzleButton.interactable = true;

        SetData(PuzzleManager.GetPuzzle(++currentPuzzleIndex));

        if (currentPuzzleIndex >= PuzzleManager.AllPuzzles.Length - 1)
            nextPuzzleButton.interactable = false;
    }

    public void PrevPuzzle()
    {
        if (currentPuzzleIndex < 1)
        {
            prevPuzzleButton.interactable = false;
            return;
        }

        nextPuzzleButton.interactable = true;

        SetData(PuzzleManager.GetPuzzle(--currentPuzzleIndex));

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

        if (PuzzleManager.AllPuzzles.Length > 0)
            nextPuzzleButton.interactable = true;
    }
}
