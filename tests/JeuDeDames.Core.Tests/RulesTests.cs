using JeuDeDames.Core;

namespace JeuDeDames.Core.Tests;

public class RulesTests
{
    private static Board BoardWith(params (int Number, Piece Piece)[] pieces)
    {
        var board = new Board();
        foreach (var (number, piece) in pieces)
            board[Square.FromNumber(number)] = piece;
        return board;
    }

    private static readonly Piece WhiteMan = new(PieceColor.White);
    private static readonly Piece BlackMan = new(PieceColor.Black);
    private static readonly Piece WhiteKing = new(PieceColor.White, IsKing: true);

    [Fact]
    public void Square_numbers_round_trip()
    {
        for (int n = 1; n <= 50; n++)
        {
            var square = Square.FromNumber(n);
            Assert.True(square.IsDark);
            Assert.Equal(n, square.Number);
        }
    }

    [Fact]
    public void Initial_position_has_20_pieces_each_and_9_opening_moves()
    {
        var game = new Game();

        Assert.Equal(20, game.Board.Count(PieceColor.White));
        Assert.Equal(20, game.Board.Count(PieceColor.Black));
        Assert.Equal(PieceColor.White, game.CurrentPlayer);
        Assert.Equal(9, game.LegalMoves.Count);
    }

    [Fact]
    public void Capture_is_mandatory()
    {
        // Pion blanc en 32, pion noir en 27 : la seule option est 32x21.
        var board = BoardWith((32, WhiteMan), (27, BlackMan), (45, WhiteMan));

        var moves = Rules.LegalMoves(board, PieceColor.White);

        var move = Assert.Single(moves);
        Assert.Equal("32x21", move.ToString());
    }

    [Fact]
    public void Man_can_capture_backwards()
    {
        var board = BoardWith((23, WhiteMan), (28, BlackMan));

        var move = Assert.Single(Rules.LegalMoves(board, PieceColor.White));

        Assert.Equal("23x32", move.ToString());
    }

    [Fact]
    public void Majority_capture_is_mandatory()
    {
        // 32 peut prendre 27 puis 17 (deux pièces) ; 45 ne peut en prendre qu'une.
        var board = BoardWith((32, WhiteMan), (27, BlackMan), (17, BlackMan), (45, WhiteMan), (40, BlackMan));

        var move = Assert.Single(Rules.LegalMoves(board, PieceColor.White));

        Assert.Equal("32x21x12", move.ToString());
        Assert.Equal(2, move.Captured.Count);
    }

    [Fact]
    public void King_flies_and_captures_at_distance()
    {
        // Dame en 46, pion noir en 28 sur la grande diagonale : atterrissage possible en 23, 19, 14, 10 ou 5.
        var board = BoardWith((46, WhiteKing), (28, BlackMan));

        var moves = Rules.LegalMoves(board, PieceColor.White);

        Assert.Equal(["46x23", "46x19", "46x14", "46x10", "46x5"], moves.Select(m => m.ToString()));
    }

    [Fact]
    public void Man_reaching_last_row_is_promoted()
    {
        var game = new Game(BoardWith((6, WhiteMan), (50, BlackMan)), PieceColor.White);

        game.Play(game.LegalMoves.First(m => m.To.Number == 1));

        Assert.Equal(WhiteKing, game.Board[Square.FromNumber(1)]);
    }

    [Fact]
    public void Player_without_moves_loses()
    {
        var game = new Game(BoardWith((32, WhiteMan), (27, BlackMan)), PieceColor.White);

        game.Play(Assert.Single(game.LegalMoves));

        Assert.Equal(GameStatus.WhiteWins, game.Status);
    }
}
