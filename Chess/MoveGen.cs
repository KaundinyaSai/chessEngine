
using System.Numerics;
public static class MoveGen
{
    static readonly ulong[] KnightLookUpTable = BoardUtils.KnightLookUpInit();
    static readonly ulong[] KingLookUpTable = BoardUtils.KingLookUpInit();
    public static readonly ulong[] WhitePawnAttackTable = BoardUtils.PawnAttacksInit(PieceColor.White);
    public static readonly ulong[] BlackPawnAttackTable = BoardUtils.PawnAttacksInit(PieceColor.Black);

    public static List<Move> MovesForPiece(GameState game, Piece piece, bool forAttackMap)
    {
        switch (piece.type)
        {
            case PieceType.Pawn:
                return PawnMoves(game, piece.color, forAttackMap);
            case PieceType.Knight:
                return KnightMoves(game.board, piece.color, forAttackMap);
            case PieceType.Bishop:
                return SlidingMoves(game.board, piece.color, PieceType.Bishop, forAttackMap);
            case PieceType.Rook:
                return SlidingMoves(game.board, piece.color, PieceType.Rook, forAttackMap);
            case PieceType.Queen:
                return SlidingMoves(game.board, piece.color, PieceType.Queen, forAttackMap);
            case PieceType.King:
                return KingMoves(game, piece.color, forAttackMap);
            default:
                return new List<Move>();
        }
    }

    public static ulong GetAttackBitboardFromListMove(GameState game, List<Move> moves)
    {
        ulong bb = 0;

        foreach (Move move in moves)
        {
            if (BoardUtils.GetPieceAt(game.board, move.fromIndex).type == PieceType.Pawn)
            {
                if (BoardUtils.GetPieceAt(game.board, move.fromIndex).color == PieceColor.White)
                    bb |= WhitePawnAttackTable[move.fromIndex] & ~game.board.WhitePieces;
                else
                    bb |= BlackPawnAttackTable[move.fromIndex] & ~game.board.BlackPieces;
            }
            else
            {
                bb |= 1UL << move.toIndex;
            }
        }

        return bb;
    }

    public static List<Move> PawnMoves(GameState game, PieceColor color, bool forAttackMap)
    {
        Board board = game.board;
        var moves = new List<Move>();
        ulong pawns = color == PieceColor.White ? board.WhitePawns : board.BlackPawns;
        ulong empty = board.EmptySquares;
        ulong opp = color == PieceColor.White ? board.BlackPieces : board.WhitePieces;
        int forward = color == PieceColor.White ? 8 : -8;
        int startRank = color == PieceColor.White ? 1 : 6;
        int promotionRank = color == PieceColor.White ? 7 : 0;
        int enPassantRank = color == PieceColor.White ? 4 : 3;

        while (pawns != 0)
        {
            int square = BitBoardUtils.PopMS1B(ref pawns);
            int rank = square / 8;
            int file = square % 8;

            // Forward move
            int to = square + forward;
            if (to >= 0 && to < 64 && ((empty & (1UL << to)) != 0))
            {
                if (rank == promotionRank)
                {
                    // Promotions
                    moves.Add(new Move(square, to, PieceType.Queen));
                    moves.Add(new Move(square, to, PieceType.Rook));
                    moves.Add(new Move(square, to, PieceType.Bishop));
                    moves.Add(new Move(square, to, PieceType.Knight));
                }
                else
                {
                    moves.Add(new Move(square, to));
                    // Double push
                    if (rank == startRank)
                    {
                        int to2 = square + 2 * forward;
                        int mid = square + forward;
                        if ((empty & (1UL << to2)) != 0 && (empty & (1UL << mid)) != 0)
                            moves.Add(new Move(square, to2));
                    }
                }
            }

            // Captures
            foreach (int df in new int[] { -1, 1 })
            {
                int captureFile = file + df;
                if (captureFile < 0 || captureFile > 7) continue;
                int captureTo = square + forward + df;
                if (captureTo < 0 || captureTo >= 64) continue;
                if ((opp & (1UL << captureTo)) != 0)
                {
                    if (rank == promotionRank)
                    {
                        moves.Add(new Move(square, captureTo, PieceType.Queen));
                        moves.Add(new Move(square, captureTo, PieceType.Rook));
                        moves.Add(new Move(square, captureTo, PieceType.Bishop));
                        moves.Add(new Move(square, captureTo, PieceType.Knight));
                    }
                    else
                    {
                        moves.Add(new Move(square, captureTo));
                    }
                }
            }

            // En Passant
            if (game.enPassantSquare != -1 && rank == enPassantRank)
            {
                foreach (int df in new int[] { -1, 1 })
                {
                    int epFile = file + df;
                    if (epFile < 0 || epFile > 7) continue;
                    int epTo = square + forward + df;
                    if (epTo == game.enPassantSquare)
                    {
                        int epPawnSquare = square + df;
                        ulong epPawnMask = 1UL << epPawnSquare;
                        if ((opp & epPawnMask) != 0) // Make sure there's an opposing pawn to capture
                        {
                            moves.Add(new Move(square, epTo));
                        }
                    }
                }
            }

        }
        return moves;
    }

    public static List<Move> KnightMoves(Board board, PieceColor color, bool forAttackMap)
    {
        List<Move> moves = new List<Move>();
        ulong knights = color == PieceColor.White ? board.WhiteKnights : board.BlackKnights;
        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (knights != 0)
        {
            int square = BitBoardUtils.PopMS1B(ref knights);
            ulong attacks = KnightLookUpTable[square]; 
            while (attacks != 0)
            {
                int squareToAdd = BitBoardUtils.PopMS1B(ref attacks);
                bool shouldAdd = forAttackMap ? true : ((1UL << squareToAdd) & ownPieces) == 0;
                if (shouldAdd)
                {
                    moves.Add(new Move(square, squareToAdd));
                }
            }
        }

        return moves;
    }

    public static List<Move> KingMoves(GameState game, PieceColor color, bool forAttackMap)
    {
        Board board = game.board;

        List<Move> moves = new List<Move>();

        ulong bitboard = color == PieceColor.White ? board.WhiteKing : board.BlackKing;
        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (bitboard != 0)
        {
            int square = BitBoardUtils.PopMS1B(ref bitboard);
            ulong attacks = KingLookUpTable[square];
            while (attacks != 0)
            {
                int squareToAdd = BitBoardUtils.PopMS1B(ref attacks);
                bool shouldAdd = forAttackMap ? true : ((1UL << square) & ownPieces) == 0;
                if (shouldAdd)
                {
                    moves.Add(new Move(square, squareToAdd));
                }
            }
        }
        
        if (color == PieceColor.White)
        {
            // KingSide
            if (game.whiteCanShortCastle &&
                (board.WhiteKing & (1UL << 4)) != 0 && // King on e1
                (board.WhiteRooks & (1UL << 7)) != 0 && // Rook on h1
                (board.AllPieces & ((1UL << 5) | (1UL << 6))) == 0) // Squares f1, g1 empty
            {
                moves.Add(new Move(4, 6)); // King moves to g1
            }

            // Queen side
            if (game.whiteCanLongCastle &&
                (board.WhiteKing & (1UL << 4)) != 0 && // King on e1
                (board.WhiteRooks & (1UL << 0)) != 0 && // Rook on a1
                (board.AllPieces & ((1UL << 1) | (1UL << 2) | (1UL << 3))) == 0) // Squares b1, c1, d1 empty
            {
                moves.Add(new Move(4, 2)); // King moves to c1
            }
        }
        else
        {
            // KingSide
            if (game.blackCanShortCastle &&
                (board.BlackKing & (1UL << 60)) != 0 && // King on e8
                (board.BlackRooks & (1UL << 63)) != 0 && // Rook on h8
                (board.AllPieces & ((1UL << 61) | (1UL << 62))) == 0)  // Squares f8, g8 empty
            {
                moves.Add(new Move(60, 62)); // King moves to g8
            }

            // QueenSide
            if (game.blackCanLongCastle &&
                (board.BlackKing & (1UL << 60)) != 0 && // King on e8
                (board.BlackRooks & (1UL << 56)) != 0 && // Rook on a8
                (board.AllPieces & ((1UL << 57) | (1UL << 58) | (1UL << 59))) == 0) // Squares b8, c8, d8 empty
            {
                moves.Add(new Move(60, 58)); // King moves to c8
            }
        }


        return moves;
    }


    public static List<Move> SlidingMoves(Board board, PieceColor color, PieceType type, bool forAttackMap)
    {
        List<Move> moves = new List<Move>();

        ulong bitboard;

        switch (type)
        {
            case PieceType.Bishop:
                bitboard = color == PieceColor.White ? board.WhiteBishops : board.BlackBishops;
                break;
            case PieceType.Rook:
                bitboard = color == PieceColor.White ? board.WhiteRooks : board.BlackRooks;
                break;
            case PieceType.Queen:
                bitboard = color == PieceColor.White ? board.WhiteQueens : board.BlackQueens;
                break;
            default: throw new ArgumentException("Not a sliding piece");
        }

        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;
        ulong opponentPieces = color == PieceColor.White ? board.BlackPieces : board.WhitePieces;

        int[] directions;

        switch (type)
        {
            case PieceType.Bishop:
                directions = [9, 7, -9, -7];
                break;
            case PieceType.Rook:
                directions = [8, 1, -8, -1];
                break;
            case PieceType.Queen:
                directions = [8, 1, -8, -1, 9, 7, -9, -7];
                break;
            default: throw new ArgumentException("Not a sliding piece");
        }

        while (bitboard != 0)
        {
            int square = BitBoardUtils.PopMS1B(ref bitboard);

            foreach (int dir in directions)
            {
                int next = square;
                while (true)
                {
                    if (IsWrapAround(dir, next))
                        break;

                    next += dir;
                    if (next < 0 || next >= 64) break;

                    ulong bit = 1UL << next;

                    if (forAttackMap)
                    {
                        moves.Add(new Move(square, next)); // Always add
                        if ((bit & board.AllPieces) != 0) break; // Stop at any piece
                    }
                    else
                    {
                        if ((bit & ownPieces) != 0) break; // Stop at friendly
                        moves.Add(new Move(square, next));
                        if ((bit & opponentPieces) != 0) break; // Stop after capture
                    }
                }
            }
        }

        return moves;
    }

    private static bool IsWrapAround(int dir, int fromSquare)
    {
        ulong fromBB = 1UL << fromSquare;
        switch (dir)
        {
            case 1:  return (fromBB & Board.FileH) != 0;
            case -1: return (fromBB & Board.FileA) != 0;
            case 9:
            case -7: return (fromBB & Board.FileH) != 0;
            case 7:
            case -9: return (fromBB & Board.FileA) != 0;
            default: return false;
        }
    }

}