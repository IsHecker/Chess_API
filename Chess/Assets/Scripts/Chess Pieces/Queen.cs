using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    private readonly Vector2Int[] directions = {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
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
