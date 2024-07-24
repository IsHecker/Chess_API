using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessNotation : MonoBehaviour
{
    public static List<PieceData> ParseToPiecePosition(JSONArray positions)
    {
        List<PieceData> result = new List<PieceData>();
        foreach (string notation in positions.Values)
        {
            var team = GetPlayerTeam(notation[0]);
            var piece = notation[1];
            var position = FileToPosition(notation.AsSpan()[1..]);
            result.Add(new PieceData(piece, position, team));
        }
        return result;
    }

    public static List<(Vector2Int, Vector2Int)> ParseSolution(JSONArray solutions)
    {
        List<(Vector2Int, Vector2Int)> result = new List<(Vector2Int, Vector2Int)>();
        foreach (string solution in solutions.Values)
        {
            var origin = FileToPosition(solution.AsSpan());
            var destination = FileToPosition(solution.AsSpan()[2..]);
            result.Add((origin, destination));
        }
        return result;
    }

    public static JSONArray ConvertToNotation(List<(Vector2Int, Vector2Int)> solution)
    {
        JSONArray result = new JSONArray();
        for (int i = 0; i < solution.Count; i++)
        {
            var (origin, destination) = solution[i];
            result.Add($"{PositionToFileName(origin)}{PositionToFileName(destination)}");
        }
        return result;
    }


    public static string ConvertToNotation(Vector2Int origin, Vector2Int destination)
    {
        return PositionToFileName(origin) + PositionToFileName(destination);
    }

    public static string ConvertToNotation(PieceData pieceData)
    {
        return $"{pieceData.team.ToString()[0]}{pieceData.pieceName}{PositionToFileName(pieceData.position)}";
    }

    public static ChessTeam GetPlayerTeam(char color)
    {
        return color == 'W' ? ChessTeam.White : ChessTeam.Black;
    }

    private static Vector2Int FileToPosition(ReadOnlySpan<char> file)
    {
        byte startIndex = 0;
        while (file[startIndex] - 'a' < 0 || file[startIndex] - 'h' > 7)
            startIndex++;

        int fileName = file[startIndex] - 'a';
        int rank = file[startIndex + 1] - '1';

        //Debug.Log($"[{startIndex}]({file}, {fileName}, {rank})");
        return new Vector2Int(fileName, rank);
    }

    private static string PositionToFileName(Vector2Int position)
    {
        return $"{(char)(position.x + 'a')}{(char)(position.y + '1')}";
    }

    /*
     Pieces:
        K = King
        Q = Queen
        R = Rook
        B = Bishop
        N = Knight
        No letter for pawns

    Squares:
        Files: a, b, c, d, e, f, g, h
        Ranks: 1 to 8

    Moves:
        Piece + Destination square (e.g., Nf3, e4)
        Capture: "x" (e.g., Bxe5, exd5)
        Check: "+"
        Checkmate: "#"
        Castling: Kingside (O-O), Queenside (O-O-O)
        Promotion: "=" (e.g., e8=Q)
        En passant: "e.p." (e.g., exd6 e.p.)

    Special annotations:

        Good move: "!"
        Excellent move: "!!"
        Mistake: "?"
        Blunder: "??"
        Interesting move: "!?"
        Dubious move: "?!"
        White to move: "1."
        Black to move: "1...
     
     */
}
