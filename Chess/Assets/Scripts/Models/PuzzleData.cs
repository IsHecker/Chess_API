using SimpleJSON;

public class PuzzleData
{
    public string Id { get; private set; }
    public string PuzzleName { get; set; }
    public string Difficulty { get; set; }
    public ChessTeam PlayerTeam { get; set; }
    public string CreatedBy { get; private set; }
    public string CreatedAt { get; private set; }
    public int SolvedBy { get; set; }
    public JSONArray PiecesPosition { get; set; }
    public JSONArray Solution { get; set; }
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
        PiecesPosition = (JSONArray)data["piecesPosition"];
        Solution = (JSONArray)data["solution"];
    }
}