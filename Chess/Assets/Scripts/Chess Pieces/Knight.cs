using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    private readonly Vector2Int[] directions = {
        new(1, 2),
        new(2, 1),
        new(-1, 2),
        new(-2, 1),
        new(1, -2),
        new(2, -1),
        new(-1, -2),
        new(-2, -1)
    };

    public override List<Vector2Int> GetLegalMoves(ChessPiece[,] board)
    {
        foreach (var direction in directions)
        {
            var target = direction + origin;
            var piece = ChessBoard.GetPieceAt(target);
            if (ChessBoard.CheckIfCoordsOnBoard(target) && (!piece || piece.team != team))
                legalMoves.Add(target);

        }
        return legalMoves;
    }
}