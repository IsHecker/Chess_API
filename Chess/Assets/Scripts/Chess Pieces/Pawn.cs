using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    private int StartYPosition => (7 + direction) % 7;

    public override List<Vector2Int> GetLegalMoves(ChessPiece[,] board)
    {
        hasMoved = origin.y != StartYPosition;

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

        // right side Capture Move
        Vector2Int movePosition = new Vector2Int(origin.x + 1, origin.y + direction);
        ChessPiece piece = ChessBoard.GetPieceAt(movePosition);
        if (piece && piece.team != team)
            legalMoves.Add(movePosition);

        // left side Capture Move
        movePosition = new Vector2Int(origin.x - 1, origin.y + direction);
        piece = ChessBoard.GetPieceAt(movePosition);
        if (piece && piece.team != team)
            legalMoves.Add(movePosition);

        return legalMoves;
    }
}
