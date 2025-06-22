
using System.Numerics;

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

    public bool IsWhiteKingInCheck => BoardUtils.IsSquareAttacked(board, BitOperations.TrailingZeroCount(board.WhiteKing), PieceColor.Black);
    public bool IsBlackKingInCheck =>   BoardUtils.IsSquareAttacked(board, BitOperations.TrailingZeroCount(board.BlackKing), PieceColor.White);

    public GameState(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        board = new Board(fen);
        board.PopulateFromFen(fen);

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
                int sq = BitBoardUtils.PopLS1B(ref pawns);
                bb |= pawnAttackTable[sq];
            }
        }
        else
        {
            Span<Move> buffer = stackalloc Move[256];
            int count = 0;

            MoveGen.MovesForPiece(this, piece, true, buffer, ref count);
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

        Span<Move> buffer = stackalloc Move[64]; // 64 is more than enough for a single piece
        int count = 0;
        MoveGen.MovesForPiece(this, piece, false, buffer, ref count);

        // Check if the move is in the generated span
        bool found = false;
        for (int i = 0; i < count; i++)
        {
            if (buffer[i].Equals(move))
            {
                found = true;
                break;
            }
        }

        if (!found)
            return false;

        // Check if it leaves the king in check
        board.MakeMove(move, enPassantSquare, out MoveInfo moveInfo, out piece);
        SetAllAttackTables();

        bool kingInCheck = piece.color == PieceColor.White ? IsWhiteKingInCheck : IsBlackKingInCheck;
        board.UnmakeMove(moveInfo);
        SetAllAttackTables();

        return !kingInCheck;
    }


    public int AllLegalMoves(Span<Move> legalMoves)
    {
        Span<Move> pseudoMoves = stackalloc Move[256];
        int pseudoCount = 0;
        MoveGen.AllPseudoLegalMoves(this, pseudoMoves, ref pseudoCount);

        int legalCount = 0;
        int whiteKingSquare = BitBoardUtils.GetKingSquare(board, PieceColor.White);
        int blackKingSquare = BitBoardUtils.GetKingSquare(board, PieceColor.Black);

        for (int i = 0; i < pseudoCount; i++)
        {
            Move move = pseudoMoves[i];
            board.MakeMove(move, enPassantSquare, out var moveInfo, out var piece);
            plyNum++;

            int kingSquare = piece.type == PieceType.King
                ? move.toIndex
                : (piece.color == PieceColor.White ? whiteKingSquare : blackKingSquare);

            bool inCheck = BoardUtils.IsSquareAttacked(
                board,
                kingSquare,
                piece.color == PieceColor.White ? PieceColor.Black : PieceColor.White
            );

            board.UnmakeMove(moveInfo);
            plyNum--;

            if (!inCheck)
                legalMoves[legalCount++] = move;
        }

        return legalCount;
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