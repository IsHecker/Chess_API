using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityHelper.Utilities;

public class PuzzleTile : MonoBehaviour
{
    [SerializeField] private TMP_Text puzzleName;
    [SerializeField] private TMP_Text difficulty;
    [SerializeField] private TMP_Text date;
    [SerializeField] private TMP_Text solvedBy;
    [SerializeField] private TMP_Text creatorName;

    [SerializeField] private Transform piecesParent;
    [SerializeField] private Image imagePrefab;

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

    private Vector2 ConvertToBlackPosition(Vector2 position)
    {
        return new Vector2(Mathf.Abs(position.x - 7), Mathf.Abs(position.y - 7));
    }

    public void SpawnPiecesInPosition(List<PieceData> piecesPositions, Sprite[] whitePieces, Sprite[] blackPieces)
    {
        if (piecesPositions == null)
            return;

        foreach (var piece in piecesPositions)
        {
            var sprite = piece.team == ChessTeam.White ?
                whitePieces.Find(obj => obj.name[1] == char.ToLower(piece.pieceName)) :
                blackPieces.Find(obj => obj.name[1] == char.ToLower(piece.pieceName));

            var pieceInstance = Instantiate(imagePrefab, piecesParent);
            pieceInstance.sprite = sprite;

            Vector2 position = new Vector2(piece.position.x, piece.position.y);
            if (PuzzleData.PlayerTeam == ChessTeam.Black)
                position = ConvertToBlackPosition(position);

            pieceInstance.transform.localPosition = position;
        }
    }


    public void SetData(JSONNode data, Sprite[] whitePieces, Sprite[] blackPieces)
    {
        PuzzleData = new PuzzleData(data);
        puzzleName.text = PuzzleData.PuzzleName;
        difficulty.text = "Difficulty: " + PuzzleData.Difficulty;
        date.text = "Date: " + PuzzleData.CreatedAt;

        if (!isCreator)
        {
            creatorName.text = PuzzleData.CreatedBy;
        }
        else
        {
            solvedBy.text = PuzzleData.SolvedBy.ToString();
        }

        SpawnPiecesInPosition(PuzzleData.PiecesPosition, whitePieces, blackPieces);
    }

    public void Solve()
    {
        FindObjectOfType<ChessBoard>().enabled = true;

        // Passes Tile date to the Puzzle setup.
        PuzzleManager.Instance.Setup(PuzzleData);

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