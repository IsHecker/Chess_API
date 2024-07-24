using UnityEngine;

public static class PieceDragDrop
{
    public static ChessPiece DraggedPiece { get; set; } = null;

    public static bool DragPiece(RaycastHit hitInfo)
    {
        if (DraggedPiece == null && Input.GetMouseButtonDown(0))
        {
            var tilePosition = ChessBoard.LookUpTilePosition(hitInfo.transform.gameObject.name);
            DraggedPiece = ChessBoard.GetPieceAt(tilePosition);

            if (DraggedPiece == null)
                return false;

            DraggedPiece.SpriteRenderer.sortingOrder = 1;
        }

        return DraggedPiece;
    }

    public static bool DropPiece(RaycastHit hitInfo)
    {
        if (!Input.GetMouseButtonUp(0))
            return false;

        Vector2Int tilePosition = ChessBoard.LookUpTilePosition(hitInfo.transform.gameObject.name);
        DraggedPiece.SpriteRenderer.sortingOrder = 0;


        if (DraggedPiece.origin == tilePosition || !ChessBoard.IsTileFree(null, tilePosition, false))
        {
            DraggedPiece.SetPosition(DraggedPiece.origin);
            DraggedPiece = null;
            return true;
        }

        ChessBoard.UpdatePiecePosition(tilePosition, DraggedPiece, true);
        DraggedPiece = null;
        return true;
    }
}
