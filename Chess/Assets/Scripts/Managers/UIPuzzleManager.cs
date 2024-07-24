using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityHelper.Utilities;

public class UIPuzzleManager : MonoBehaviour
{
    [SerializeField] private bool isCreator = false;
    [SerializeField] private GameObject puzzleTilePrefab;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Transform resultParent;
    [SerializeField] private TMP_Text resultCount;
    [SerializeField] private TMP_InputField searchBar;

    public int PuzzlesCount { 
        get => puzzlesCount; 
        set {
            puzzlesCount = value;
            resultCount.text = $"Results({puzzlesCount})";
        } 
    }

    private int puzzlesCount = 0;

    private JSONArray solvedPuzzles;

    private void OnEnable()
    {
        StartCoroutine(AsyncOperations());
    }

    private IEnumerator AsyncOperations()
    {

        if (User.Role == "Creator")
        {
            yield return GetCreatorPuzzles();
            yield break;
        }

        yield return GetPuzzlesRequest();
    }

    private IEnumerator GetPlayerSolvedPuzzles()
    {
        yield return APIHandler.SendRequest("http://localhost:3000/api/users/getuser", "GET", result: response => solvedPuzzles = (JSONArray)response["data"]["puzzles"]);
    }


    private IEnumerator GetCreatorPuzzles()
    {
        DestroyAllPuzzleTiles();

        JSONNode response = null;

        loadingScreen.SetActive(true);

        var search = searchBar.text;
        yield return APIHandler.SendRequest($"http://localhost:3000/api/puzzles/creator", result: (res) => response = res);


        int length = response["length"];

        for (int i = 0; i < length; i++)
        {
            var puzzleData = CreatePuzzleTile();
            puzzleData.SetData(response["data"]["puzzles"][i]);
        }

        loadingScreen.SetActive(false);
    }

    private IEnumerator GetPuzzlesRequest()
    {
        DestroyAllPuzzleTiles();
        JSONNode response = null;
        loadingScreen.SetActive(true);
        var search = searchBar.text;

        yield return GetPlayerSolvedPuzzles();

        yield return APIHandler.SendRequest($"http://localhost:3000/api/puzzles/{search}?sort=-createdAt", result: (res) => response = res);

        int length = response["length"];
        PuzzlesCount = length;

        for (int i = 0; i < length; i++)
        {
            var puzzleData = CreatePuzzleTile();
            puzzleData.SetData(response["data"]["puzzles"][i]);
        }
        var allPuzzles = resultParent.GetComponentsInChildren<PuzzleTile>();

        if (!isCreator)
            MarkSolvedPuzzles(allPuzzles, solvedPuzzles);

        PuzzleManager.AllPuzzles = allPuzzles;

        loadingScreen.SetActive(false);
    }

    private void MarkSolvedPuzzles(PuzzleTile[] allPuzzles, JSONArray solvedPuzzles)
    {
        HashSet<string> solvedSet = ConvertToHashSet(solvedPuzzles);

        foreach (var puzzle in allPuzzles)
        {
            if (solvedSet.Contains(puzzle.PuzzleData.Id))
            {
                puzzle.PuzzleData.IsSolvedByPlayer = true;
                puzzle.solved.SetActive(true);
            }
        }
    }

    private HashSet<string> ConvertToHashSet(JSONArray array)
    {
        HashSet<string> result = new HashSet<string>();
        foreach (var element in array.Values)
        {
            result.Add(element);
        }
        return result;
    }

    private void DestroyAllPuzzleTiles()
    {
        resultParent.DeleteChildren();
    }

    private PuzzleTile CreatePuzzleTile()
    {
        var puzzleData = Instantiate(puzzleTilePrefab).GetComponent<PuzzleTile>();
        puzzleData.transform.SetParent(resultParent);
        puzzleData.GetComponent<RectTransform>().localScale = Vector3.one;
        puzzleData.GetComponent<RectTransform>().SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        return puzzleData;
    }

    public void GetPuzzles()
    {
        StartCoroutine(GetPuzzlesRequest());
    }

    public void Close()
    {
        gameObject.SetActive(false);

        if (!isCreator)
            FindAnyObjectByType<ChessBoard>().enabled = true;

        PostProcessingAnimation.Instance.UnBlur();
    }
}
