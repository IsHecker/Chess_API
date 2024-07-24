using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    private readonly Vector2Int[] directions = {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public override List<Vector2Int> GetLegalMoves(ChessPiece[,] board)
    {



        // Up
        //for (int y = origin.y + 1; y < 8; y++)
        //{
        //    if (board[origin.x, y] != null)
        //    {
        //        if(board[origin.x, y].team!=team)
        //            legalMoves.Add(new Vector2Int(origin.x, y));
        //        break;
        //    }
        //    legalMoves.Add(new Vector2Int(origin.x, y));
        //}

        //// Down
        //for (int y = origin.y - 1; y >= 0; y--)
        //{
        //    if (board[origin.x, y] != null)
        //    {
        //        if (board[origin.x, y].team != team)
        //            legalMoves.Add(new Vector2Int(origin.x, y));
        //        break;
        //    }
        //    legalMoves.Add(new Vector2Int(origin.x, y));
        //}

        //// Right
        //for (int x = origin.x + 1; x < 8; x++)
        //{
        //    if (board[x, origin.y] != null)
        //    {
        //        if (board[x, origin.y].team != team)
        //            legalMoves.Add(new Vector2Int(x, origin.y));
        //        break;
        //    }
        //    legalMoves.Add(new Vector2Int(x, origin.y));
        //}

        //// Left
        //for (int x = origin.x - 1; x >= 0; x--)
        //{
        //    if (board[x, origin.y] != null)
        //    {
        //        if (board[x, origin.y].team != team)
        //            legalMoves.Add(new Vector2Int(x, origin.y));
        //        break;
        //    }
        //    legalMoves.Add(new Vector2Int(x, origin.y));
        //}
        return GenerateLegalMoves(directions, legalMoves);
    }
}
