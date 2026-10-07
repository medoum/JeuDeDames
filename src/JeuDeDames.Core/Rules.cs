namespace JeuDeDames.Core;

/// <summary>
/// Règles internationales (10×10) :
/// - la prise est obligatoire, et il faut choisir la rafle qui prend le plus de pièces ;
/// - les pions avancent en diagonale mais peuvent prendre vers l'avant comme vers l'arrière ;
/// - les dames sont « volantes » : elles se déplacent et prennent à distance ;
/// - les pièces prises ne sont retirées qu'à la fin de la rafle et ne peuvent pas être sautées deux fois.
/// </summary>
public static class Rules
{
    private static readonly (int DRow, int DCol)[] Directions = [(-1, -1), (-1, 1), (1, -1), (1, 1)];

    public static IReadOnlyList<Move> LegalMoves(Board board, PieceColor player)
    {
        var captures = new List<Move>();
        foreach (var (square, piece) in board.PiecesOf(player))
            captures.AddRange(CapturesFrom(board, square, piece));

        if (captures.Count > 0)
        {
            int max = captures.Max(m => m.Captured.Count);
            return captures.Where(m => m.Captured.Count == max).ToList();
        }

        var moves = new List<Move>();
        foreach (var (square, piece) in board.PiecesOf(player))
            moves.AddRange(SimpleMovesFrom(board, square, piece));
        return moves;
    }

    private static IEnumerable<Move> SimpleMovesFrom(Board board, Square from, Piece piece)
    {
        int forward = piece.Color == PieceColor.White ? -1 : 1;

        foreach (var (dRow, dCol) in Directions)
        {
            if (!piece.IsKing && dRow != forward)
                continue;

            var target = from.Offset(dRow, dCol);
            while (target.IsOnBoard && board[target] is null)
            {
                yield return new Move(from, [target], []);
                if (!piece.IsKing)
                    break;
                target = target.Offset(dRow, dCol);
            }
        }
    }

    private static List<Move> CapturesFrom(Board board, Square from, Piece piece)
    {
        var results = new List<Move>();
        ExploreCaptures(board, from, piece, from, [], [], results);
        return results;
    }

    /// <summary>Parcours en profondeur de toutes les rafles possibles depuis <paramref name="current"/>.</summary>
    private static void ExploreCaptures(
        Board board, Square origin, Piece piece, Square current,
        List<Square> path, List<Square> captured, List<Move> results)
    {
        bool extended = false;

        foreach (var (dRow, dCol) in Directions)
        {
            // Cherche la pièce adverse à sauter dans cette direction.
            var victim = current.Offset(dRow, dCol);
            if (piece.IsKing)
                while (victim.IsOnBoard && IsEmpty(board, victim, origin))
                    victim = victim.Offset(dRow, dCol);

            if (!victim.IsOnBoard
                || board[victim] is not { } target
                || target.Color == piece.Color
                || captured.Contains(victim))
                continue;

            // Cases d'atterrissage : juste derrière pour un pion, n'importe quelle case libre derrière pour une dame.
            var landing = victim.Offset(dRow, dCol);
            while (landing.IsOnBoard && IsEmpty(board, landing, origin))
            {
                extended = true;
                path.Add(landing);
                captured.Add(victim);
                ExploreCaptures(board, origin, piece, landing, path, captured, results);
                path.RemoveAt(path.Count - 1);
                captured.RemoveAt(captured.Count - 1);

                if (!piece.IsKing)
                    break;
                landing = landing.Offset(dRow, dCol);
            }
        }

        if (!extended && captured.Count > 0)
            results.Add(new Move(origin, path.ToList(), captured.ToList()));
    }

    // La case de départ est considérée vide : la pièce l'a quittée pendant la rafle.
    private static bool IsEmpty(Board board, Square square, Square origin) =>
        board[square] is null || square == origin;
}
