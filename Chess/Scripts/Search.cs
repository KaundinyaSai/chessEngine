public static class Search
{
    public static int Perft(GameState game, int depth)
    {
        if (depth == 0)
            return 1;

        int nodes = 0;
        Span<Move> moves = stackalloc Move[256];
        int moveCount = 0;
        MoveGen.AllPseudoLegalMoves(game, moves, ref moveCount);

        for (int i = 0; i < moveCount; i++)
        {
            Move move = moves[i];
            game.SimplerMakeMove(move, out MoveInfo moveInfo);

            int kingSquare = BitBoardUtils.GetKingSquare(
                game.board,
                game.sideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White
            );

            bool inCheck = BoardUtils.IsSquareAttacked(game.board, kingSquare, game.sideToMove);

            if (!inCheck)
            {
                nodes += Perft(game, depth - 1);
            }

            game.SimplerUnmakeMove(moveInfo);
        }

        return nodes;
    }

    public static int Negamax(GameState game, int depth, int alpha, int beta)
    {
        Span<Move> moves = stackalloc Move[256];
        int count = game.AllLegalMoves(moves);

        if (depth == 0)
        {
            return Eval.GetEvaluation(game);
        }

        OrderMoves(game, moves, count);

        int bestScore = int.MinValue;

        bool isInCheckNow = game.sideToMove == PieceColor.White
            ? game.IsWhiteKingInCheck
            : game.IsBlackKingInCheck;

        if (count == 0)
        {
            return isInCheckNow ? -(Values.MATE_VALUE - depth) : 0; 
            // Negitive sign is VERY important here as negamax flips the
            // perspective of the evaluation instead of returning positive for white and negative for black as minimax does.
            // subtracting depth from mate value is to ensure that mate in fewer moves is better  
                                                                       
        }

        for (int i = 0; i < count; i++)
        {
            Move move = moves[i];

            game.SimplerMakeMove(move, out MoveInfo moveInfo);

            int score = -Negamax(game, depth - 1, -beta, -alpha);

            game.SimplerUnmakeMove(moveInfo);

            bestScore = Math.Max(bestScore, score);
            alpha = Math.Max(alpha, score);

            if (beta <= alpha)
            {
                break; // Prune
            }
        }

        return bestScore;
    }

    public static Move FindBestMove(GameState game, int depth)
    {
        Span<Move> moves = stackalloc Move[256];
        int count = game.AllLegalMoves(moves);

        int bestEval = int.MinValue;
        Move bestMove = default;

        int alpha = int.MinValue + 1; // to avoid overflow when negating
        int beta = int.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Move move = moves[i];

            game.SimplerMakeMove(move, out MoveInfo moveInfo);

            int eval = -Negamax(game, depth - 1, -beta, -alpha);

            game.SimplerUnmakeMove(moveInfo);

            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = move;
            }

            alpha = Math.Max(alpha, eval);
        }

        return bestMove;
    }

    static void OrderMoves(GameState game, Span<Move> moves, int count)
    {
        // Orders moves from "best" to "worst", which makes alpha-beta pruning faster.

        Span<Move> buffer = stackalloc Move[256];
        int insert = 0;

        for (int i = 0; i < count; i++)
            if (BoardUtils.IsMovePromotion(moves[i], game.board))
                buffer[insert++] = moves[i];

        for (int i = 0; i < count; i++)
            if (BoardUtils.IsMoveCastle(moves[i], game.board))
                buffer[insert++] = moves[i];

        for (int i = 0; i < count; i++)
            if (BoardUtils.IsMoveCapture(moves[i], game.board))
                buffer[insert++] = moves[i];

        for (int i = 0; i < count; i++)
            if (!BoardUtils.IsMovePromotion(moves[i], game.board) &&
                !BoardUtils.IsMoveCastle(moves[i], game.board) &&
                !BoardUtils.IsMoveCapture(moves[i], game.board))
                buffer[insert++] = moves[i];

        for (int i = 0; i < count; i++)
            moves[i] = buffer[i];
    }

}