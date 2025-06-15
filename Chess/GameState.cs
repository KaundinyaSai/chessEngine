
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

    public bool isWhiteTurn => plyNum % 2 == 0;

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
        moves.Push(moveInfo);

        // Check if it was a double pawn push and update the en passant square accorginly
        if (pieceToMove.type == PieceType.Pawn && MathF.Abs(move.toIndex - move.fromIndex) == 16)
        {
            int enPassantOffset = pieceToMove.color == PieceColor.White ? -16 : 16;
            enPassantSquare = move.toIndex + enPassantOffset;
        }
        else
        {
            enPassantSquare = -1;
        }

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
        board.UnmakeMove(moveToUnmake);
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

}