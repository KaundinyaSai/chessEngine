namespace Chess;

public class GameState
{
    public Board board;
    public Color currentTurn;
    public int halfMoveClock; // Half-move clock for the fifty-move rule
    public int fullMoveNumber; // Full move number in the game
    public List<Move> moveHistory; // History of moves made in the game

    public bool canWhiteShortCastle;
    public bool canWhiteLongCastle;
    public bool canBlackShortCastle;
    public bool canBlackLongCastle;

    // The bitwise AND (&) operator returns 1 if both bits are 1, otherwise it returns 0.
    // So we use get the bitboard of the king's index (with only one bit set to one, which is the king's index)
    // and when we & that with the attack map of the opposite color, it will return 0 if and only if the bit at the 
    // king's index is set to zero in the attack map, meaning that square isnt attacked by the opposite color.
    // if it isnt 0, then that bit is set to 1, meaning that square is attacked by the opposite color.
    public bool whiteKingInCheck => (MoveGen.blackAttackMap & Utils.GetBitboardFromIndex(board.whiteKingIndex)) != 0;
    public bool blackKingInCheck => (MoveGen.whiteAttackMap & Utils.GetBitboardFromIndex(board.blackKingIndex)) != 0;

    public GameState(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        board = new Board(fen);

        Utils.GetCastlingRightsFromFEN(fen, out canWhiteShortCastle, out canWhiteLongCastle, out canBlackShortCastle
        , out canBlackLongCastle);

        currentTurn = Color.White;
        halfMoveClock = 0;
        fullMoveNumber = 1;
        moveHistory = new List<Move>();

        board.LoadFromFEN(fen);
    }

    public void LoadBoard()
    {
        board.LoadFromFEN(board.fen);
        MoveGen.GenerateMovesForAllPieces(board.pieces_on_board, null, this);
    }

    public void MakeMove(Move move)
    {
        if (!IsStrictlyLegalMove(move))
        {
            throw new InvalidOperationException("Illegal move attempted. Leaves king in check.");
        }

        board.MakeMove(move);

        moveHistory.Add(move);
        currentTurn = currentTurn == Color.White ? Color.Black : Color.White;

        MoveGen.GenerateMovesForAllPieces(board.pieces_on_board, moveHistory.LastOrDefault(), this);

        if (currentTurn == Color.White)
        {
            fullMoveNumber++;
        }
    }

    public void UnmakeMove(Move move)
    {
        board.UnmakeMove(move);

        if (moveHistory.Count > 0)
        {
            moveHistory.RemoveAt(moveHistory.Count - 1);
        }

        currentTurn = currentTurn == Color.White ? Color.Black : Color.White;

        MoveGen.GenerateMovesForAllPieces(board.pieces_on_board, moveHistory.LastOrDefault(), this);

        if (currentTurn == Color.White)
        {
            fullMoveNumber--;
        }
    }

    public bool IsStrictlyLegalMove(Move move)
    {
        board.MakeMove(move);
        Color pieceColor = move.movedPiece?.pieceColor ?? Color.None;

        MoveGen.GenerateMovesForAllPieces(board.pieces_on_board, move, this);

        bool isInCheck = pieceColor == Color.White ? whiteKingInCheck : blackKingInCheck;
        board.UnmakeMove(move);

        MoveGen.GenerateMovesForAllPieces(board.pieces_on_board, moveHistory.LastOrDefault(), this);

        return !isInCheck;
    }

    public List<Move> GetAllMoves()
    {
        List<Move> allMoves = new List<Move>();
        foreach (Piece piece in board.pieces_on_board)
        {
            if (piece.pieceColor != currentTurn) continue;

            allMoves.AddRange(piece.legalMoves);
        }
        return allMoves;
    
    }
}