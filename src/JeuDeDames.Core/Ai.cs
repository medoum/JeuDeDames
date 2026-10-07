using System.Diagnostics;

namespace JeuDeDames.Core;

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
}

/// <summary>
/// Joueur ordinateur : recherche negamax avec élagage alpha-bêta et approfondissement itératif.
/// La recherche s'arrête à la profondeur maximale ou quand le temps imparti est écoulé ;
/// on garde alors le meilleur coup de la dernière profondeur terminée.
/// </summary>
public sealed class Ai
{
    private const int ManValue = 100;
    private const int KingValue = 300;
    private const int WinScore = 1_000_000;

    private readonly int _maxDepth;
    private readonly TimeSpan _timeLimit;
    private readonly Random _random;
    private Stopwatch _clock = new();

    public Ai(int maxDepth, TimeSpan timeLimit, Random? random = null)
    {
        _maxDepth = maxDepth;
        _timeLimit = timeLimit;
        _random = random ?? Random.Shared;
    }

    public static Ai For(Difficulty difficulty, Random? random = null) => difficulty switch
    {
        Difficulty.Easy => new Ai(2, TimeSpan.FromMilliseconds(300), random),
        Difficulty.Medium => new Ai(4, TimeSpan.FromMilliseconds(800), random),
        _ => new Ai(10, TimeSpan.FromMilliseconds(1500), random),
    };

    public Move ChooseMove(Board board, PieceColor player)
    {
        var moves = Rules.LegalMoves(board, player).ToList();
        if (moves.Count == 0)
            throw new InvalidOperationException("Aucun coup possible.");
        if (moves.Count == 1)
            return moves[0];

        // Mélange initial pour varier les parties entre coups de même valeur.
        _random.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(moves));
        _clock = Stopwatch.StartNew();

        var best = moves[0];
        for (int depth = 1; depth <= _maxDepth; depth++)
        {
            var result = SearchRoot(board, player, moves, depth);
            if (result is null)
                break; // Temps écoulé : profondeur incomplète, on l'ignore.

            best = result;
            // Le meilleur coup est exploré en premier à la profondeur suivante (meilleur élagage).
            moves.Remove(best);
            moves.Insert(0, best);
        }
        return best;
    }

    private Move? SearchRoot(Board board, PieceColor player, List<Move> moves, int depth)
    {
        Move? best = null;
        int alpha = -WinScore - 1;
        const int beta = WinScore + 1;

        foreach (var move in moves)
        {
            var next = board.Clone();
            next.Apply(move);
            int? score = -Negamax(next, player.Opponent(), depth - 1, -beta, -alpha, ply: 1);
            if (score is null)
                return null;

            if (score > alpha)
            {
                alpha = score.Value;
                best = move;
            }
        }
        return best;
    }

    /// <summary>Renvoie le score du point de vue de <paramref name="player"/>, ou null si le temps est écoulé.</summary>
    private int? Negamax(Board board, PieceColor player, int depth, int alpha, int beta, int ply)
    {
        if (_clock.Elapsed > _timeLimit)
            return null;

        var moves = Rules.LegalMoves(board, player);
        if (moves.Count == 0)
            return -WinScore + ply; // Défaite : plus elle est lointaine, mieux c'est.

        // On ne s'arrête pas au milieu d'une série de prises, pour éviter d'évaluer une position instable.
        if (depth <= 0 && !moves[0].IsCapture)
            return Evaluate(board, player);

        foreach (var move in moves)
        {
            var next = board.Clone();
            next.Apply(move);
            int? score = -Negamax(next, player.Opponent(), depth - 1, -beta, -alpha, ply + 1);
            if (score is null)
                return null;

            if (score >= beta)
                return beta;
            if (score > alpha)
                alpha = score.Value;
        }
        return alpha;
    }

    /// <summary>Matériel, plus un petit bonus pour les pions avancés et centrés.</summary>
    public static int Evaluate(Board board, PieceColor player)
    {
        int score = 0;
        foreach (var square in Board.DarkSquares())
        {
            if (board[square] is not { } piece)
                continue;

            int value;
            if (piece.IsKing)
            {
                value = KingValue;
            }
            else
            {
                int advancement = piece.Color == PieceColor.White ? Board.Size - 1 - square.Row : square.Row;
                int centrality = square.Col is >= 2 and <= 7 ? 3 : 0;
                value = ManValue + advancement * 2 + centrality;
            }

            score += piece.Color == player ? value : -value;
        }
        return score;
    }
}
