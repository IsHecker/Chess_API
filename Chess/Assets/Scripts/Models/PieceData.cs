using System;
using UnityEngine;

[Serializable]
public readonly struct PieceData
{
    public readonly char pieceName;
    public readonly Vector2Int position;
    public readonly ChessTeam team;

    public PieceData(char pieceName, Vector2Int position, ChessTeam team)
    {
        this.pieceName = pieceName;
        this.position = position;
        this.team = team;
    }

    public override readonly string ToString()
    {
        return $"team: {team}, piece: {pieceName}, position: {position}";
    }
}
