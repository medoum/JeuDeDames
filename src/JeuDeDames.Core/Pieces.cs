namespace JeuDeDames.Core;

public enum PieceColor
{
    White,
    Black,
}

public static class PieceColorExtensions
{
    public static PieceColor Opponent(this PieceColor color) =>
        color == PieceColor.White ? PieceColor.Black : PieceColor.White;
}

/// <summary>Un pion ou une dame.</summary>
public readonly record struct Piece(PieceColor Color, bool IsKing = false)
{
    public Piece Promote() => this with { IsKing = true };
}

/// <summary>
/// Case du plateau. La ligne 0 est en haut (côté noir), la ligne 9 en bas (côté blanc).
/// Seules les cases sombres, où (Row + Col) est impair, sont jouables.
/// </summary>
public readonly record struct Square(int Row, int Col)
{
    public bool IsOnBoard => Row is >= 0 and < Board.Size && Col is >= 0 and < Board.Size;

    public bool IsDark => (Row + Col) % 2 == 1;

    /// <summary>Numéro officiel de la case (1 à 50), compté depuis le coin haut gauche.</summary>
    public int Number => Row * (Board.Size / 2) + Col / 2 + 1;

    public static Square FromNumber(int number)
    {
        if (number is < 1 or > 50)
            throw new ArgumentOutOfRangeException(nameof(number));

        int index = number - 1;
        int row = index / 5;
        int col = (index % 5) * 2 + (row % 2 == 0 ? 1 : 0);
        return new Square(row, col);
    }

    public Square Offset(int dRow, int dCol) => new(Row + dRow, Col + dCol);

    public override string ToString() => Number.ToString();
}
