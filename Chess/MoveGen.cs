
using System.Numerics;
using System.Runtime.InteropServices;

public static class MoveGen
{
    static ulong[] KnightLookUpTable = BoardUtils.KnightLookUpInit();
    static ulong[] KingLookUpTable = BoardUtils.KingLookUpInit();

    public static ulong PawnMoves(Board board, PieceColor color)
    {
        ulong moves = 0;
        ulong bitboard = color == PieceColor.White ? board.WhitePawns : board.BlackPawns;
        ulong empty = board.EmptySquares;
        ulong oppBB = color == PieceColor.White ? board.BlackPieces : board.WhitePieces;

        if (color == PieceColor.White)
        {
            // Single pushes
            ulong singlePush = (bitboard << 8) & empty;
            moves |= singlePush;

            // Double pushes (must be on rank 2 and square in front must also be empty)
            ulong rank2 = 0x000000000000FF00UL; // bitboard for pawns on rank 2 in hex
            ulong doublePush = ((bitboard & rank2) << 8) & empty; // bitboard & ran 2 checks if pawn is on rank 2.
            doublePush = (doublePush << 8) & empty;
            moves |= doublePush;

            // Captures
            ulong leftCaptures = (bitboard << 7) & oppBB & ~Board.FileH;
            ulong rightCaptures = (bitboard << 9) & oppBB & ~Board.FileA;
            moves |= leftCaptures | rightCaptures;
        }
        else // Black
        {
            // Single pushes
            ulong singlePush = (bitboard >> 8) & empty; // Right shift for black
            moves |= singlePush;

            // Double pushes (must be on rank 7 and square in front must also be empty)
            ulong rank7 = 0x00FF000000000000UL; // bitboard for pawns on rank 7 in hex
            ulong doublePush = ((bitboard & rank7) >> 8) & empty;
            doublePush = (doublePush >> 8) & empty;
            moves |= doublePush;

            // Captures
            ulong leftCaptures = (bitboard >> 9) & oppBB & ~Board.FileH;
            ulong rightCaptures = (bitboard >> 7) & oppBB & ~Board.FileA;
            moves |= leftCaptures | rightCaptures;
        }

        return moves;
    }

    public static ulong KnightMoves(Board board, PieceColor color)
    {
        ulong moves = 0UL;
        ulong bitboard = color == PieceColor.White ? board.WhiteKnights : board.BlackKnights;
        ulong thisColorPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (bitboard != 0)
        {
            int square = BitOperations.TrailingZeroCount(bitboard); // Gets the least significant set bit
            bitboard = BitBoardUtils.ClearBit(bitboard, square);
            moves |= KnightLookUpTable[square] & ~thisColorPieces;
            // repeat till no knight left to add moves to.
        }

        return moves;
    }

    public static ulong KingMoves(Board board, PieceColor color)
    {
        ulong moves = 0UL;

        ulong bitboard = color == PieceColor.White ? board.WhiteKing : board.BlackKing;
        ulong thisColorPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (bitboard != 0)
        {
            int square = BitOperations.TrailingZeroCount(bitboard);
            bitboard = BitBoardUtils.ClearBit(bitboard, square);
            moves |= KingLookUpTable[square] & ~thisColorPieces;
        }

        return moves;
    }

}