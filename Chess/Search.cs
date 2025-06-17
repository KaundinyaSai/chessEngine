public struct PerftStats
{
    public int nodes;
    public int captures;
    public int enPassants;
    public int castles;
    public int promotions;
}

public static class Search
{
    public static void PerftDivide(GameState game, int depth)
    {
        int totalNodes = 0;
        PerftStats totalStats = new PerftStats();
        List<Move> moves = game.AllLegalMoves();

        foreach (Move move in moves)
        {
            game.board.MakeMove(move, game.enPassantSquare, out MoveInfo moveInfo, out Piece movedPiece);

            moveInfo.previousEnPassantSquare = game.enPassantSquare;
            moveInfo.previousWhiteCanShortCastle = game.whiteCanShortCastle;
            moveInfo.previousWhiteCanLongCastle = game.whiteCanLongCastle;
            moveInfo.previousBlackCanShortCastle = game.blackCanShortCastle;
            moveInfo.previousBlackCanLongCastle = game.blackCanLongCastle;

            game.plyNum++;
            game.UpdateEnPassantSquare(movedPiece, moveInfo);
            game.UpdateCastlingRights(movedPiece, moveInfo);

            PerftStats moveStats = new PerftStats();
            Perft(game, depth - 1, ref moveStats);

            Console.WriteLine($"{BoardUtils.ConvertToAlg(move)}: {moveStats.nodes}");

            // Accumulate totals
            totalNodes += moveStats.nodes;
            totalStats.nodes += moveStats.nodes;
            totalStats.captures += moveStats.captures;
            totalStats.enPassants += moveStats.enPassants;
            totalStats.castles += moveStats.castles;
            totalStats.promotions += moveStats.promotions;

            game.board.UnmakeMove(moveInfo);
            game.plyNum--;
            game.enPassantSquare = moveInfo.previousEnPassantSquare;
            game.whiteCanShortCastle = moveInfo.previousWhiteCanShortCastle;
            game.whiteCanLongCastle = moveInfo.previousWhiteCanLongCastle;
            game.blackCanShortCastle = moveInfo.previousBlackCanShortCastle;
            game.blackCanLongCastle = moveInfo.previousBlackCanLongCastle;
        }

        Console.WriteLine($"\nNodes searched: {totalStats.nodes}");
        Console.WriteLine($"Captures: {totalStats.captures}");
        Console.WriteLine($"En Passants: {totalStats.enPassants}");
        Console.WriteLine($"Castles: {totalStats.castles}");
        Console.WriteLine($"Promotions: {totalStats.promotions}");
    }




    public static void Perft(GameState game, int depth, ref PerftStats stats)
    {
        if (depth == 0)
        {
            stats.nodes++;
            return;
        }

        List<Move> moves = game.AllLegalMoves();

        foreach (Move move in moves)
        {
            game.board.MakeMove(move, game.enPassantSquare, out MoveInfo moveInfo, out Piece movedPiece);

            moveInfo.previousEnPassantSquare = game.enPassantSquare;
            moveInfo.previousWhiteCanShortCastle = game.whiteCanShortCastle;
            moveInfo.previousWhiteCanLongCastle = game.whiteCanLongCastle;
            moveInfo.previousBlackCanShortCastle = game.blackCanShortCastle;
            moveInfo.previousBlackCanLongCastle = game.blackCanLongCastle;

            game.plyNum++;
            game.UpdateEnPassantSquare(movedPiece, moveInfo);
            game.UpdateCastlingRights(movedPiece, moveInfo);
            game.SetAllAttackTables();

            // Update stats if depth == 1 (only count the first move in this path)
            if (depth == 1)
            {
                stats.nodes++;
                if (moveInfo.capturedPiece.HasValue)
                    stats.captures++;
                if (moveInfo.enPassant)
                    stats.enPassants++;
                if (moveInfo.shortCastle || moveInfo.longCastle)
                    stats.castles++;
                if (moveInfo.promotionType.HasValue)
                    stats.promotions++;
            }
            else
            {
                Perft(game, depth - 1, ref stats);
            }

            game.board.UnmakeMove(moveInfo);
            game.plyNum--;
            game.enPassantSquare = moveInfo.previousEnPassantSquare;
            game.whiteCanShortCastle = moveInfo.previousWhiteCanShortCastle;
            game.whiteCanLongCastle = moveInfo.previousWhiteCanLongCastle;
            game.blackCanShortCastle = moveInfo.previousBlackCanShortCastle;
            game.blackCanLongCastle = moveInfo.previousBlackCanLongCastle;
            game.SetAllAttackTables();
        }
    }


}