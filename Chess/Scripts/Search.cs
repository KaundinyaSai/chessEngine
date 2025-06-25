
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