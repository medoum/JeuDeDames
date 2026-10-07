using System.Diagnostics;
using JeuDeDames.Core;

namespace JeuDeDames.Core.Tests;

public class AiTests
{
    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Medium)]
    [InlineData(Difficulty.Hard)]
    public void Plays_a_legal_move_from_the_initial_position_within_time(Difficulty difficulty)
    {
        var game = new Game();
        var clock = Stopwatch.StartNew();

        var move = Ai.For(difficulty, new Random(42)).ChooseMove(game.Board, game.CurrentPlayer);

        Assert.Contains(move, game.LegalMoves);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Finds_the_winning_move()
    {
        // Le pion noir en 5 ne peut aller qu'en 10 (occupé) ; 19-14 l'empêche aussi de prendre : il est bloqué.
        var board = new Board
        {
            [Square.FromNumber(10)] = new Piece(PieceColor.White),
            [Square.FromNumber(19)] = new Piece(PieceColor.White),
            [Square.FromNumber(5)] = new Piece(PieceColor.Black),
        };

        var move = Ai.For(Difficulty.Easy, new Random(1)).ChooseMove(board, PieceColor.White);

        Assert.Equal("19-14", move.ToString());
    }

    [Fact]
    public void Evaluation_favours_the_side_with_more_material()
    {
        var game = new Game();
        game.Board[Square.FromNumber(1)] = null;

        Assert.True(Ai.Evaluate(game.Board, PieceColor.White) > 0);
        Assert.True(Ai.Evaluate(game.Board, PieceColor.Black) < 0);
    }
}
