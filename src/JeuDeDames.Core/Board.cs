namespace JeuDeDames.Core;

public sealed class Board
{
    public const int Size = 10;

    private readonly Piece?[,] _cells = new Piece?[Size, Size];

    public Piece? this[Square square]
    {
        get => _cells[square.Row, square.Col];
        set => _cells[square.Row, square.Col] = value;
    }

    /// <summary>Position de départ : 20 pions noirs en haut, 20 pions blancs en bas.</summary>
    public static Board Initial()
    {
        var board = new Board();
        foreach (var square in DarkSquares())
        {
            if (square.Row < 4)
                board[square] = new Piece(PieceColor.Black);
            else if (square.Row >= 6)
                board[square] = new Piece(PieceColor.White);
        }
        return board;
    }

    public static IEnumerable<Square> DarkSquares()
    {
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                if ((row + col) % 2 == 1)
                    yield return new Square(row, col);
    }

    public IEnumerable<(Square Square, Piece Piece)> PiecesOf(PieceColor color)
    {
        foreach (var square in DarkSquares())
            if (this[square] is { } piece && piece.Color == color)
                yield return (square, piece);
    }

    public int Count(PieceColor color) => PiecesOf(color).Count();

    public Board Clone()
    {
        var copy = new Board();
        Array.Copy(_cells, copy._cells, _cells.Length);
        return copy;
    }

    /// <summary>Applique un coup supposé légal et gère la promotion en dame.</summary>
    public void Apply(Move move)
    {
        var piece = this[move.From] ?? throw new InvalidOperationException($"Aucune pièce sur la case {move.From}.");

        foreach (var captured in move.Captured)
            this[captured] = null;

        this[move.From] = null;

        // Un pion n'est promu que s'il termine son coup sur la dernière rangée.
        int promotionRow = piece.Color == PieceColor.White ? 0 : Size - 1;
        if (!piece.IsKing && move.To.Row == promotionRow)
            piece = piece.Promote();

        this[move.To] = piece;
    }
}
