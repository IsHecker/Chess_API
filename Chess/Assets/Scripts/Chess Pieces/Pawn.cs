using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetLegalMoves(ChessPiece[,] board)
    {
        var oneMove = origin + Vector2Int.up * direction;

        if (!ChessBoard.CheckIfCoordsOnBoard(oneMove))
            return legalMoves;

        if (!ChessBoard.GetPieceAt(oneMove))
        {
            // One move front.
            legalMoves.Add(oneMove);
            var twoMove = oneMove + Vector2Int.up * direction;

            // Two move front.
            if (!hasMoved && !ChessBoard.GetPieceAt(twoMove))
                legalMoves.Add(twoMove);
        }

        // Capture Move
        if (origin.y != 7)
        {
            //var piece = board[origin.x + 1, origin.y + direction];
            var piece = ChessBoard.GetPieceAt(new Vector2Int(origin.x + 1, origin.y + direction));
            if (piece && piece.team != team)
                legalMoves.Add(new Vector2Int(origin.x + 1, origin.y + direction));
        }

        if (origin.x != 0)
        {
            //var piece = board[origin.x - 1, origin.y + direction];
            var piece = ChessBoard.GetPieceAt(new Vector2Int(origin.x - 1, origin.y + direction));
            if (piece && piece.team != team)
                legalMoves.Add(new Vector2Int(origin.x - 1, origin.y + direction));
        }
        return legalMoves;
    }
}
