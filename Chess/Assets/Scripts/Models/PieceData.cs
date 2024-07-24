using System;
using UnityEngine;

[Serializable]
public struct PieceData
{
    public char pieceName;
    public Vector2Int position;
    public ChessTeam team;

    public PieceData(char piece, Vector2Int position, ChessTeam team)
    {
        this.pieceName = piece;
        this.position = position;
        this.team = team;
    }

    public override readonly string ToString()
    {
        return $"team: {team}, piece: {pieceName}, position: {position}";
    }
}
