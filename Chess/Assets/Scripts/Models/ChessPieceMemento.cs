using System;
using UnityEngine;

/// <summary>
/// Stores Data about History Move.
/// </summary>
[Serializable]
public class ChessPieceMemento
{
    public ChessPiece Piece { get; private set; }
    public Vector2Int Position { get; set; }
    public ChessPiece CapturedPiece { get; set; }

    public ChessPieceMemento(ChessPiece piece, Vector2Int position, ChessPiece capturedPiece)
    {
        Piece = piece;
        Position = position;
        CapturedPiece = capturedPiece;
    }
}
