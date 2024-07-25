using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleData
{
    public string Id { get; private set; }
    public string PuzzleName { get; set; }
    public string Difficulty { get; set; }
    public ChessTeam PlayerTeam { get; set; }
    public string CreatedBy { get; private set; }
    public string CreatedAt { get; private set; }
    public int SolvedBy { get; set; }
    public List<PieceData> PiecesPosition { get; set; }
    public List<(Vector2Int, Vector2Int)> Solution { get; set; }
    public bool IsSolvedByPlayer { get; set; }

    public PuzzleData()
    {
        Difficulty = "Beginner";
        PlayerTeam = ChessTeam.White;
    }

    public PuzzleData(JSONNode data)
    {
        Id = data["_id"];
        PuzzleName = data["name"];
        Difficulty = data["difficulty"];
        PlayerTeam = data["color"] == "White" ? ChessTeam.White : ChessTeam.Black;
        CreatedBy = data["createdBy"];
        CreatedAt = data["createdAt"];
        SolvedBy = data["solvedBy"];
        PiecesPosition = ChessNotation.ParseToPiecePosition((JSONArray)data["piecesPosition"]);
        Solution = ChessNotation.ParseSolution((JSONArray)data["solution"]);
    }
}