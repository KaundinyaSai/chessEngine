
using System.Numerics;
public static class MoveGen
{
    static ulong[] KnightLookUpTable = BoardUtils.KnightLookUpInit();
    static ulong[] KingLookUpTable = BoardUtils.KingLookUpInit();

    public static Dictionary<int, ulong> PawnMoves(Board board, PieceColor color)
    {
        Dictionary<int, ulong> result = new Dictionary<int, ulong>();
        ulong pawns = color == PieceColor.White ? board.WhitePawns : board.BlackPawns;
        ulong empty = board.EmptySquares;
        ulong opp = color == PieceColor.White ? board.BlackPieces : board.WhitePieces;

        while (pawns != 0)
        {
            int square = BitBoardUtils.PopMS1B(ref pawns);

            ulong fromBB = 1UL << square;
            ulong moveTargets = 0;

            if (color == PieceColor.White)
            {
                // Pushes
                ulong oneAhead = fromBB << 8;
                if ((oneAhead & empty) != 0)
                {
                    moveTargets |= oneAhead;
                    if ((fromBB & 0x000000000000FF00UL) != 0 && ((oneAhead << 8) & empty) != 0)
                        moveTargets |= oneAhead << 8;
                }

                // Captures
                if ((fromBB & Board.FileA) == 0)
                    moveTargets |= (fromBB << 9) & opp;
                if ((fromBB & Board.FileH) == 0)
                    moveTargets |= (fromBB << 7) & opp;
            }
            else
            {
                ulong oneAhead = fromBB >> 8;
                if ((oneAhead & empty) != 0)
                {
                    moveTargets |= oneAhead;
                    if ((fromBB & 0x00FF000000000000UL) != 0 && ((oneAhead >> 8) & empty) != 0)
                        moveTargets |= oneAhead >> 8;
                }

                if ((fromBB & Board.FileA) == 0)
                    moveTargets |= (fromBB >> 7) & opp;
                if ((fromBB & Board.FileH) == 0)
                    moveTargets |= (fromBB >> 9) & opp;
            }

            if (moveTargets != 0)
                result[square] = moveTargets;
        }

        return result;
    }

    public static Dictionary<int, ulong> KnightMoves(Board board, PieceColor color)
    {
        Dictionary<int, ulong> result = new Dictionary<int, ulong>();
        ulong knights = color == PieceColor.White ? board.WhiteKnights : board.BlackKnights;
        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (knights != 0)
        {
            int square = BitBoardUtils.PopMS1B(ref knights);
            result[square] = KnightLookUpTable[square] & ~ownPieces;
        }

        return result;
    }

    public static Dictionary<int, ulong> KingMoves(Board board, PieceColor color)
    {
        Dictionary<int, ulong> result = new Dictionary<int, ulong>();

        ulong bitboard = color == PieceColor.White ? board.WhiteKing : board.BlackKing;
        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (bitboard != 0)
        {
            int square = BitOperations.TrailingZeroCount(bitboard);
            bitboard = BitBoardUtils.ClearBit(bitboard, square);

            result[square] = KingLookUpTable[square] & ~ownPieces;
        }

        return result;
    }

    public static Dictionary<int, ulong> SlidingMoves(Board board, PieceColor color, PieceType type)
    {
        Dictionary<int, ulong> result = new Dictionary<int, ulong>();

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
            ulong moves = 0UL;

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

                    if ((bit & ownPieces) != 0) break;

                    moves |= bit;

                    if ((bit & opponentPieces) != 0) break;
                }
            }


            result[square] = moves;
        }

        return result;
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