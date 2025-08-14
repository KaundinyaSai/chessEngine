namespace ChessEngine;

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

    public static Move FindBestMove(GameState game, int depth)
    {
        Span<Move> legalMoves = stackalloc Move[256];
        int moveCount = game.AllLegalMoves(legalMoves);

        Move bestMove = default;
        int bestValue = int.MinValue;

        for (int i = 0; i < moveCount; i++)
        {
            Move move = legalMoves[i];

            game.SimplerMakeMove(move, out MoveInfo mi);
            int value = -SearchForMove(game, depth - 1, int.MinValue + 1, int.MaxValue - 1);
            game.SimplerUnmakeMove(mi);

            if (value > bestValue)
            {
                bestValue = value;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private static int SearchForMove(GameState game, int depth, int alpha, int beta)
    {
        if (depth == 0)
            return Eval.Evaluate(game);

        Span<Move> moves = stackalloc Move[256];
        int moveCount = game.AllLegalMoves(moves);

        if (moveCount == 0)
        {
            bool inCheck = game.sideToMove == PieceColor.White
                ? game.IsWhiteKingInCheck
                : game.IsBlackKingInCheck;
            return inCheck ? (-99999 + depth) : 0;
        }

        int bestValue = int.MinValue;

        for (int i = 0; i < moveCount; i++)
        {
            Move move = moves[i];
            game.SimplerMakeMove(move, out MoveInfo mi);
            int value = -SearchForMove(game, depth - 1, -beta, -alpha);
            game.SimplerUnmakeMove(mi);

            if (value > bestValue) bestValue = value;
            if (value > alpha) alpha = value;

            if (alpha >= beta)
                break;
        }

        return bestValue;
    }

}


