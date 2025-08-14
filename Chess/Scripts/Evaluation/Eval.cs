namespace ChessEngine;

using System.Numerics;

public static class Eval
{
    public static int Evaluate(GameState game)
    {
        int score = 0;

        score += EvaluateMaterial(game);
        score += EvaluatePSQTs(game);

        return game.sideToMove == PieceColor.White ? score : -score;
    }

    static int EvaluateMaterial(GameState game)
    {
        int score = 0;

        // Add material values for each piece type
        score += Values.PAWN_VALUE * BitOperations.PopCount(game.board.WhitePawns);
        score += Values.KNIGHT_VALUE * BitOperations.PopCount(game.board.WhiteKnights);
        score += Values.BISHOP_VALUE * BitOperations.PopCount(game.board.WhiteBishops);
        score += Values.ROOK_VALUE * BitOperations.PopCount(game.board.WhiteRooks);
        score += Values.QUEEN_VALUE * BitOperations.PopCount(game.board.WhiteQueens);

        score -= Values.PAWN_VALUE * BitOperations.PopCount(game.board.BlackPawns);
        score -= Values.KNIGHT_VALUE * BitOperations.PopCount(game.board.BlackKnights);
        score -= Values.BISHOP_VALUE * BitOperations.PopCount(game.board.BlackBishops);
        score -= Values.ROOK_VALUE * BitOperations.PopCount(game.board.BlackRooks);
        score -= Values.QUEEN_VALUE * BitOperations.PopCount(game.board.BlackQueens);

        return score;
    }

    static int EvaluatePSQTs(GameState game)
    {
        int score = 0;

        for (int i = 0; i < 64; i++)
        {
            Piece? piece = game.board.Pieces[i];
            if (piece == null) continue;

            bool isWhite = piece.Value.color == PieceColor.White;

            // Flip vertically for black’s perspective
            int squareIndex = isWhite ? i : i ^ 56; // neat trick I found online. i ^ 56 flips the square for black.

            int value = piece.Value.type switch
            {
                PieceType.Pawn   => Values.PawnPSQT[squareIndex],
                PieceType.Knight => Values.KnightPSQT[squareIndex],
                PieceType.Bishop => Values.BishopPSQT[squareIndex],
                PieceType.Rook   => Values.RookPSQT[squareIndex],
                PieceType.Queen  => Values.QueenPSQT[squareIndex],
                PieceType.King   => Values.KingOpeningPSQT[squareIndex],
                _ => 0
            };

            score += isWhite ? value : -value;
        }
        return score;
    }


}