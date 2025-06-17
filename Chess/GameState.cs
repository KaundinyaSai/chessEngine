
public class GameState
{
    public Board board;

    public Stack<MoveInfo> moves;
    public int halfMoveClock; // for 50 move rule

    public bool whiteCanShortCastle;
    public bool whiteCanLongCastle;
    public bool blackCanShortCastle;
    public bool blackCanLongCastle;

    public int enPassantSquare; // null if none, 0-63 is not none.

    public int plyNum; // a ply is half of a move

    public PieceColor sideToMove => plyNum % 2 == 0 ? PieceColor.White : PieceColor.Black;

    public bool IsWhiteKingInCheck => (board.WhiteKing & board.BlackAttacks) != 0;
    public bool IsBlackKingInCheck => (board.BlackKing & board.WhiteAttacks) != 0;

    public GameState(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        board = new Board(fen);
        board.PopulateFromFen(fen);
        SetAllAttackTables();

        moves = new Stack<MoveInfo>();

        BoardUtils.ParseFen(this, fen);
    }

    public void MakeMove(Move move)
    {
        if (!IsMoveLegal(move))
        {
            throw new ArgumentException("Not legal move");
        }
        board.MakeMove(move, enPassantSquare, out MoveInfo moveInfo, out Piece pieceToMove);
        moveInfo.previousEnPassantSquare = enPassantSquare;
        moveInfo.previousWhiteCanShortCastle = whiteCanShortCastle;
        moveInfo.previousWhiteCanLongCastle = whiteCanLongCastle;
        moveInfo.previousBlackCanShortCastle = blackCanShortCastle;
        moveInfo.previousBlackCanLongCastle = blackCanLongCastle;

        moves.Push(moveInfo);

        UpdateEnPassantSquare(pieceToMove, moveInfo);
        UpdateCastlingRights(pieceToMove, moveInfo);

        plyNum++;
        SetAllAttackTables();
    }

    public void UnmakeLastMove()
    {
        if (moves.Count <= 0)
        {
            throw new Exception("No moves to unmake");
        }

        MoveInfo moveToUnmake = moves.Pop();
        Piece pieceToMove = BoardUtils.GetPieceAt(board, moveToUnmake.move.toIndex);
        board.UnmakeMove(moveToUnmake);

        plyNum--;
        
        enPassantSquare = moveToUnmake.previousEnPassantSquare;
        whiteCanShortCastle = moveToUnmake.previousWhiteCanShortCastle;
        whiteCanLongCastle = moveToUnmake.previousWhiteCanLongCastle;
        blackCanShortCastle = moveToUnmake.previousBlackCanShortCastle;
        blackCanLongCastle = moveToUnmake.previousBlackCanLongCastle;

        SetAllAttackTables();
    }

    public void SetAttackTables(Piece piece)
    {
        ref ulong bb = ref BoardUtils.GetAttackBitboardFromPiece(board, piece);
        bb = 0;

        if (piece.type == PieceType.Pawn)
        {
            ulong pawns = piece.color == PieceColor.White ? board.WhitePawns : board.BlackPawns;
            var pawnAttackTable = piece.color == PieceColor.White
                ? MoveGen.WhitePawnAttackTable
                : MoveGen.BlackPawnAttackTable;

            while (pawns != 0)
            {
                int sq = BitBoardUtils.PopMS1B(ref pawns);
                bb |= pawnAttackTable[sq];
            }
        }
        else
        {
            List<Move> moves = MoveGen.MovesForPiece(this, piece, true);
            bb |= MoveGen.GetAttackBitboardFromListMove(this, moves);
        }
    }

    public void SetAllAttackTables()
    {
        foreach (PieceColor color in Enum.GetValues(typeof(PieceColor)))
        {
            foreach (PieceType type in Enum.GetValues(typeof(PieceType)))
            {
                SetAttackTables(new Piece(type, color));
            }
        }
    }

    public bool IsMoveLegal(Move move)
    {
        Piece piece = BoardUtils.GetPieceAt(board, move.fromIndex);
        List<Move> moves = BoardUtils.returnMovesWithFromIndex(MoveGen.MovesForPiece(this, piece, false), move.fromIndex);

        if (!moves.Contains(move))
            return false;

        // Check if it leaves the king in check
        board.MakeMove(move, enPassantSquare, out MoveInfo moveInfo, out piece);
        SetAllAttackTables();

        bool kingInCheck = piece.color == PieceColor.White ? IsWhiteKingInCheck : IsBlackKingInCheck;
        board.UnmakeMove(moveInfo);
        SetAllAttackTables();

        return !kingInCheck;
    }

    public List<Move> AllLegalMoves()
    {
        List<Move> legalMoves = new();
        var pseudoMoves = MoveGen.AllPseudoLegalMoves(this);
        
        int whiteKingSquare = BitBoardUtils.GetKingSquare(board, PieceColor.White);
        int blackKingSquare = BitBoardUtils.GetKingSquare(board, PieceColor.Black);

        foreach (var move in pseudoMoves)
        {
            board.MakeMove(move, enPassantSquare, out var moveInfo, out var piece);
            plyNum++;
            int kingSquare = piece.type == PieceType.King ? move.toIndex :
                            piece.color == PieceColor.White ? whiteKingSquare : blackKingSquare;

            bool inCheck = BoardUtils.IsSquareAttacked(
                board, kingSquare,
                piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White
            );

            board.UnmakeMove(moveInfo);
            plyNum--;

            if (!inCheck)
                legalMoves.Add(move);
        }

        return legalMoves;
    }


    public void UpdateEnPassantSquare(Piece pieceToMove, MoveInfo moveInfo)
    {
        Move move = moveInfo.move;
        // Check if it was a double pawn push and update the en passant square accorginly
        if (pieceToMove.type == PieceType.Pawn && MathF.Abs(move.toIndex - move.fromIndex) == 16)
        {
            int enPassantOffset = pieceToMove.color == PieceColor.White ? -8 : 8;
            enPassantSquare = move.toIndex + enPassantOffset;
        }
        else
        {
            enPassantSquare = -1;
        }
    }

    public void UpdateCastlingRights(Piece pieceToMove, MoveInfo moveInfo)
    {
        Move move = moveInfo.move;
        switch (pieceToMove.type)
        {
            case PieceType.King:
                if (pieceToMove.color == PieceColor.White)
                {
                    whiteCanShortCastle = false;
                    whiteCanLongCastle = false;
                }
                else
                {
                    blackCanShortCastle = false;
                    blackCanLongCastle = false;
                }
                break;
            case PieceType.Rook:
                if (pieceToMove.color == PieceColor.White)
                {
                    if (move.fromIndex == 0) whiteCanLongCastle = false;
                    if (move.fromIndex == 7) whiteCanShortCastle = false;
                }
                else
                {
                    if (move.fromIndex == 56) blackCanLongCastle = false;
                    if (move.fromIndex == 63) blackCanShortCastle = false;
                }
                break;
        }

        // If a rook is captured, update castling rights
        if (moveInfo.capturedPiece.HasValue && moveInfo.capturedPiece.Value.type == PieceType.Rook)
        {
            if (moveInfo.capturedPiece.Value.color == PieceColor.White)
            {
                if (move.toIndex == 0) whiteCanLongCastle = false;
                if (move.toIndex == 7) whiteCanShortCastle = false;
            }
            else
            {
                if (move.toIndex == 56) blackCanLongCastle = false;
                if (move.toIndex == 63) blackCanShortCastle = false;
            }
        }
    }


}