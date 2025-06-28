public static class Eval
{
    public static int GetEvaluation(GameState game)
    {
        int eval = 0;

        eval += CountMaterial(game, PieceColor.White) - CountMaterial(game, PieceColor.Black);
        eval += EvalPSQTs(game.board);

        // Flip based on player to move:
        return game.sideToMove == PieceColor.White ? eval : -eval;
    }



    public static int CountMaterial(GameState game, PieceColor color)
    {
        int total = 0;

        foreach (Piece? piece in game.board.Pieces)
        {
            if (piece is { } p && p.color == color)
            {
                total += PieceUtils.GetValue(p.type);
            }
        }

        return total;
    }

    static int EvalPSQTs(Board board)
    {
        int score = 0;

        score += PieceUtils.PSQTFor(board.WhitePawns, Values.PawnTable);
        score -= PieceUtils.PSQTFor(board.BlackPawns, Values.PawnTable, mirror: true);

        score += PieceUtils.PSQTFor(board.WhiteKnights, Values.KnightTable);
        score -= PieceUtils.PSQTFor(board.BlackKnights, Values.KnightTable, mirror: true);

        score += PieceUtils.PSQTFor(board.WhiteBishops, Values.BishopTable);
        score -= PieceUtils.PSQTFor(board.BlackBishops, Values.BishopTable, mirror: true);

        score += PieceUtils.PSQTFor(board.WhiteRooks, Values.RookTable);
        score -= PieceUtils.PSQTFor(board.BlackRooks, Values.RookTable, mirror: true);

        score += PieceUtils.PSQTFor(board.WhiteQueens, Values.QueenTable);
        score -= PieceUtils.PSQTFor(board.BlackQueens, Values.QueenTable, mirror: true);


        var kingSqWhite = BoardUtils.GetKingSquare(board, PieceColor.White);
        var kingSqBlack = BoardUtils.GetKingSquare(board, PieceColor.Black);

        var kingTable = Values.KingTableOpening;

        score += kingTable[kingSqWhite];
        score -= kingTable[Values.Mirror(kingSqBlack)];

        return score;
    }

    static int ApplyHeuristics(Board board, PieceColor sideToMove)
    {
        // Basically, common things that make a position "good" or "bad". 
        // Such as center control, castling, etc....

        int score = 0;

        // 1. Center Control (For opening, so only gives slight bonuses to pawns, knights and bishops).
        const ulong CenterMask = (1UL << (int)Squares.d4) |
                             (1UL << (int)Squares.e4) |
                             (1UL << (int)Squares.d5) |
                             (1UL << (int)Squares.e5);

        const int CenterBonusPawn = 5;
        const int CenterBonusKnight = 3;
        const int CenterBonusBishop = 2;

        ulong allyPieces = sideToMove == PieceColor.White ? board.WhitePieces : board.BlackPieces;
        ulong centerPieces = allyPieces & CenterMask;

        while (centerPieces != 0)
        {
            int square = BitBoardUtils.PopLS1B(ref centerPieces);
            Piece piece = BoardUtils.GetPieceAt(board, square);

            switch (piece.type)
            {
                case PieceType.Pawn:
                    score += CenterBonusPawn;
                    break;
                case PieceType.Knight:
                    score += CenterBonusKnight;
                    break;
                case PieceType.Bishop:
                    score += CenterBonusBishop;
                    break;
            }
        }


        return score;
    }


}
