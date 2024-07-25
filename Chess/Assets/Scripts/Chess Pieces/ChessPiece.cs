using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum ChessPieceType
{
    None,
    Pawn,
    Bishop,
    Knight,
    Rook,
    Queen,
    King
}

public class ChessPiece : MonoBehaviour
{
    public ChessTeam team;
    public Vector2Int origin;
    public ChessPieceType type;

    public Vector2 desiredPosition;
    protected int direction;
    protected List<Vector2Int> legalMoves = new();

    public bool hasMoved = false;

    public SpriteRenderer SpriteRenderer { get; private set; }
    public bool IsCaputred { get; private set; } = false;

    private float alpha = 1f;

    private bool smooth = false;

    private void Awake()
    {
        origin = -Vector2Int.one;

        SpriteRenderer = GetComponent<SpriteRenderer>();
        direction = (int)team;
    }

    private void Update()
    {
        if (smooth)
            transform.localPosition = Vector2.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * 10f);

        SpriteRenderer.color = Color.Lerp(SpriteRenderer.color, new Color(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b, alpha), Time.deltaTime * 15f);
    }

    public void SetPosition(Vector2 target, bool smooth = false)
    {
        this.smooth = smooth;
        desiredPosition = new Vector2(Mathf.Clamp(target.x, -0.5f, 7.5f), Mathf.Clamp(target.y, -0.5f, 7.5f));
        if (!smooth)
        {
            transform.position = desiredPosition;
        }
    }

    public void SetOrigin(Vector2Int target, bool smooth = false)
    {
        origin = target;
        SetPosition(target, smooth);
    }
    public void Capture(ChessPiece pieceToCapture)
    {
        if (pieceToCapture.team == team)
            return;

        pieceToCapture.FadeOut(); // when it's opponent team.
    }

    public void UnCapture(ChessPiece pieceToUnCapture)
    {
        pieceToUnCapture.FadeIn();
    }

    public List<Vector2Int> GenerateLegalMoves(Vector2Int[] directions, List<Vector2Int> legalMoves, int distance = 8)
    {
        foreach (var direction in directions)
        {
            int x = origin.x + direction.x, y = origin.y + direction.y;
            int currentDistance = distance;
            for (; currentDistance-- > 0 && ChessBoard.CheckIfCoordsOnBoard(new Vector2Int(x, y)); x += direction.x, y += direction.y)
            {
                var anotherPiece = ChessBoard.GetPieceAt(new Vector2Int(x, y));
                if (anotherPiece)
                {
                    if (anotherPiece.team != team)
                        legalMoves.Add(new Vector2Int(x, y));
                    break;
                }
                legalMoves.Add(new Vector2Int(x, y));
            }
        }
        return legalMoves;
    }
    public virtual List<Vector2Int> GetLegalMoves(ChessPiece[,] board)
    {
        return new List<Vector2Int>
        {
            new (3,3),
            new (3,4),
            new (4,3),
            new (4,4)
        };
    }

    public void FadeOut()
    {
        alpha = 0f;
        IsCaputred = true;
        SpriteRenderer.sortingOrder = -1;
    }

    public void FadeIn()
    {
        alpha = 1f;
        IsCaputred = false;
        SpriteRenderer.sortingOrder = -1;
    }
}
