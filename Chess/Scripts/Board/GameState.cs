
using System.Numerics;

namespace ChessEngine;

public class GameState
{
    public Board board;

    public Stack<MoveInfo> moves;
    public int halfMoveClock; // for 50 move rule

    public CastlingRights castlingRights;

    public int enPassantSquare; // null if none, 0-63 is not none.

    public int plyNum; // a ply is half of a move

    public PieceColor sideToMove => plyNum % 2 == 0 ? PieceColor.White : PieceColor.Black;

    public bool IsWhiteKingInCheck => BoardUtils.IsSquareAttacked(board, BitOperations.TrailingZeroCount(board.WhiteKing), PieceColor.Black);
    public bool IsBlackKingInCheck => BoardUtils.IsSquareAttacked(board, BitOperations.TrailingZeroCount(board.BlackKing), PieceColor.White);

    const int ENDGAME_MATERIAL_THRESHOLD = 1300; // Estimation of material for endgame, about 13 pawns worth of material
    public GameState(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") // Starting fen
    {
        Magic.AttackTablesInit();
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
        moveInfo.previousCastlingRights = castlingRights;

        moves.Push(moveInfo);

        UpdateEnPassantSquare(pieceToMove, moveInfo);
        SetCastlingRights(pieceToMove, moveInfo);

        plyNum++;
    }

    public void UnmakeLastMove()
    {
        if (moves.Count <= 0)
        {
            throw new Exception("No moves to unmake");
        }

        MoveInfo moveToUnmake = moves.Pop();
        board.UnmakeMove(moveToUnmake);

        plyNum--;

        enPassantSquare = moveToUnmake.previousEnPassantSquare;
        castlingRights = moveToUnmake.previousCastlingRights;
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

        bool kingInCheck = piece.color == PieceColor.White ? IsWhiteKingInCheck : IsBlackKingInCheck;
        board.UnmakeMove(moveInfo);

        return !kingInCheck;
    }


    public int AllLegalMoves(Span<Move> legalMoves, bool onlyCaptures = false, bool onlyChecks = false)
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

            if (onlyCaptures && onlyChecks && (!BoardUtils.IsMoveCapture(move, board) || !BoardUtils.IsMoveCheck(move, this, sideToMove)))
                continue;

            if ((onlyChecks && !onlyCaptures) && !BoardUtils.IsMoveCheck(move, this, sideToMove))
                continue;

            if ((onlyCaptures && !onlyChecks) && !BoardUtils.IsMoveCapture(move, board))
                continue;

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


    public void SetCastlingRights(Piece pieceToMove, MoveInfo moveInfo)
    {
        int from = moveInfo.move.fromIndex;
        int to = moveInfo.move.toIndex;

        if (pieceToMove.type == PieceType.King)
        {
            if (pieceToMove.color == PieceColor.White)
                castlingRights &= ~(CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside);
            else
                castlingRights &= ~(CastlingRights.BlackKingside | CastlingRights.BlackQueenside);
        }
        else if (pieceToMove.type == PieceType.Rook)
        {
            if (pieceToMove.color == PieceColor.White)
            {
                if (from == 0) castlingRights &= ~CastlingRights.WhiteQueenside;
                else if (from == 7) castlingRights &= ~CastlingRights.WhiteKingside;
            }
            else
            {
                if (from == 56) castlingRights &= ~CastlingRights.BlackQueenside;
                else if (from == 63) castlingRights &= ~CastlingRights.BlackKingside;
            }
        }

        // Handle captured rook
        if (moveInfo.capturedPiece is { } captured && captured.type == PieceType.Rook)
        {
            if (captured.color == PieceColor.White)
            {
                if (to == 0) castlingRights &= ~CastlingRights.WhiteQueenside;
                else if (to == 7) castlingRights &= ~CastlingRights.WhiteKingside;
            }
            else
            {
                if (to == 56) castlingRights &= ~CastlingRights.BlackQueenside;
                else if (to == 63) castlingRights &= ~CastlingRights.BlackKingside;
            }
        }
    }

    public void SimplerMakeMove(Move move, out MoveInfo moveInfo)
    {
        board.MakeMove(move, enPassantSquare, out moveInfo, out Piece movedPiece);

        moveInfo.previousEnPassantSquare = enPassantSquare;
        moveInfo.previousCastlingRights = castlingRights;
        plyNum++;
        UpdateEnPassantSquare(movedPiece, moveInfo);
        SetCastlingRights(movedPiece, moveInfo);
    }

    public void SimplerUnmakeMove(MoveInfo moveInfo)
    {
        board.UnmakeMove(moveInfo);
        plyNum--;
        enPassantSquare = moveInfo.previousEnPassantSquare;
        castlingRights = moveInfo.previousCastlingRights;
    }
}


