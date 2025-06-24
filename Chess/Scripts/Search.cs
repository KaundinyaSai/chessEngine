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

        Span<Move> moves = stackalloc Move[256];
        int moveCount = 0;
        MoveGen.AllPseudoLegalMoves(game, moves, ref moveCount);

        for (int i = 0; i < moveCount; i++)
        {
            Move move = moves[i];
            game.SimplerMakeMove(move, out MoveInfo moveInfo);

            // Check king safety after move
            int kingSquare = BitBoardUtils.GetKingSquare(
                game.board,
                game.sideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White
            );
            bool inCheck = BoardUtils.IsSquareAttacked(game.board, kingSquare, game.sideToMove);

            if (!inCheck)
            {
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
            }

            // Always unmake the move!
            game.SimplerUnmakeMove(moveInfo);
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

        Span<Move> moves = stackalloc Move[256];
        int moveCount = 0;
        MoveGen.AllPseudoLegalMoves(game, moves, ref moveCount);

        for (int i = 0; i < moveCount; i++)
        {
            Move move = moves[i];
            game.SimplerMakeMove(move, out MoveInfo moveInfo);

            // Check king safety after move
            int kingSquare = BitBoardUtils.GetKingSquare(
                game.board,
                game.sideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White
            );
            bool inCheck = BoardUtils.IsSquareAttacked(game.board, kingSquare, game.sideToMove);

            if (!inCheck)
            {
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
            }

            // Always unmake the move!
            game.SimplerUnmakeMove(moveInfo);
        }
    }

   public static int Negamax(GameState game, int depth)
    {
        if (depth == 0)
            return Eval.GetEvaluation(game); // No perspective flip inside

        Span<Move> moves = stackalloc Move[256];
        int count = game.AllLegalMoves(moves);

        int bestScore = int.MinValue;

        for (int i = 0; i < count; i++)
        {
            Move move = moves[i];

            game.SimplerMakeMove(move, out MoveInfo moveInfo);

            int score = -Negamax(game, depth - 1);

            game.SimplerUnmakeMove(moveInfo);

            bestScore = Math.Max(bestScore, score);
        }

        return bestScore;
    }


    public static Move FindBestMove(GameState game, int depth)
    {
        Span<Move> moves = stackalloc Move[256];
        int count = game.AllLegalMoves(moves);

        int bestEval = int.MinValue;
        Move bestMove = default;

        for (int i = 0; i < count; i++)
        {
            Move move = moves[i];

            game.SimplerMakeMove(move, out MoveInfo moveInfo);

            int eval = -Negamax(game, depth - 1);

            game.SimplerUnmakeMove(moveInfo);

            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = move;
            }
        }

        return bestMove;
    }


    public static PieceColor FlipColor(PieceColor color) =>
        color == PieceColor.White ? PieceColor.Black : PieceColor.White;

}