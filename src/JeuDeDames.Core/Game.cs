namespace JeuDeDames.Core;

public enum GameStatus
{
    InProgress,
    WhiteWins,
    BlackWins,
}

/// <summary>Une partie : le plateau, le joueur au trait et l'historique des coups. Les blancs commencent.</summary>
public sealed class Game
{
    private readonly List<Move> _history = [];

    public Game() : this(Board.Initial(), PieceColor.White) { }

    public Game(Board board, PieceColor currentPlayer)
    {
        Board = board;
        CurrentPlayer = currentPlayer;
        LegalMoves = Rules.LegalMoves(Board, CurrentPlayer);
    }

    public Board Board { get; }

    public PieceColor CurrentPlayer { get; private set; }

    public IReadOnlyList<Move> LegalMoves { get; private set; }

    public IReadOnlyList<Move> History => _history;

    /// <summary>Un joueur qui ne peut plus jouer (plus de pièces ou pièces bloquées) a perdu.</summary>
    public GameStatus Status => LegalMoves.Count > 0
        ? GameStatus.InProgress
        : CurrentPlayer == PieceColor.White ? GameStatus.BlackWins : GameStatus.WhiteWins;

    public void Play(Move move)
    {
        if (Status != GameStatus.InProgress)
            throw new InvalidOperationException("La partie est terminée.");
        if (!LegalMoves.Contains(move))
            throw new InvalidOperationException($"Coup illégal : {move}.");

        Board.Apply(move);
        _history.Add(move);
        CurrentPlayer = CurrentPlayer.Opponent();
        LegalMoves = Rules.LegalMoves(Board, CurrentPlayer);
    }
}
