public static class Eval
{
    public static int GetEvaluation(GameState game)
    {
        int perspective = game.sideToMove == PieceColor.White ? 1 : -1;

        int material = CountMaterial(game, PieceColor.White) - CountMaterial(game, PieceColor.Black);
        int psqt = EvalPSQTs(game.board);
        
        return perspective * (material + psqt);
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

    static  int EvalPSQTs(Board board)
    {
        int score = 0;

        score += PieceUtils.PSQTFor(board.WhitePawns, Values.PawnTable);
        score -= PieceUtils.PSQTFor(board.BlackPawns, Values.PawnTable,  mirror: true);

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


}
