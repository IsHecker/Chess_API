using SimpleJSON;
using System.Collections;
using TMPro;
using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    [SerializeField] private TMP_Text puzzleName;
    [SerializeField] private TMP_Text difficulty;
    [SerializeField] private TMP_Text date;
    [SerializeField] private TMP_Text creatorName;

    [SerializeField] private bool isCreator = false;

    public GameObject solved;

    public bool IsSolvedByPlayer { get; set; }


    public PuzzleData PuzzleData { get; private set; }

    private IEnumerator DeleteRequest()
    {
        JSONObject body = new JSONObject();
        body.Add("_id", PuzzleData.Id);
        yield return APIHandler.SendRequest("http://localhost:3000/api/puzzles", "DELETE", body.ToString());
        FindAnyObjectByType<UIPuzzleManager>().PuzzlesCount--;
        Destroy(gameObject);
    }

    public void SetData(JSONNode data)
    {
        PuzzleData = new PuzzleData(data);
        puzzleName.text = PuzzleData.PuzzleName;
        difficulty.text = "Difficulty: " + PuzzleData.Difficulty;
        date.text = "Date: " + PuzzleData.CreatedAt;

        if (!isCreator)
            creatorName.text = PuzzleData.CreatedBy;
    }

    public void Solve()
    {
        FindObjectOfType<ChessBoard>().enabled = true;

        // Passes Tile date to the Puzzle setup.
        PuzzleManager.Setup(PuzzleData);

        FindAnyObjectByType<UIDetailsManager>().SetData(PuzzleData);
        FindAnyObjectByType<UIPuzzleManager>().Close();
    }

    public void Edit()
    {
        PuzzleCreator.Instance.Setup(PuzzleData);
        FindAnyObjectByType<UIPuzzleManager>().Close();
    }

    public void Delete()
    {
        StartCoroutine(DeleteRequest());
    }
}