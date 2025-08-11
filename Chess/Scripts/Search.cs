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

}


