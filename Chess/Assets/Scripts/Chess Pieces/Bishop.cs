using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    private readonly Vector2Int[] directions = {
        new(1, 1),
        new(-1, 1),
        new(1, -1),
        new(-1, -1)
    };

    public override List<Vector2Int> GetLegalMoves(ChessPiece[,] board)
    {
        return GenerateLegalMoves(directions, legalMoves);
    }
}