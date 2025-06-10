
using System.Numerics;
public static class MoveGen
{
    static ulong[] KnightLookUpTable = BoardUtils.KnightLookUpInit();
    static ulong[] KingLookUpTable = BoardUtils.KingLookUpInit();

    public static Dictionary<int, ulong> PawnMoves(GameState game, PieceColor color)
    {
        Board board = game.board;

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

                    // Double push from rank 2 (0x000000000000FF00)
                    if ((fromBB & 0x000000000000FF00UL) != 0)
                    {
                        ulong twoAhead = fromBB << 16;
                        if ((twoAhead & empty) != 0)
                            moveTargets |= twoAhead;
                    }
                }

                // Captures (only if not wrapping)
                if ((fromBB & Board.FileA) == 0)
                    moveTargets |= (fromBB << 9) & opp;
                if ((fromBB & Board.FileH) == 0)
                    moveTargets |= (fromBB << 7) & opp;

                // En Passant
                if (game.enPassantSquare != -1)
                {
                    int fileFrom = square % 8;
                    int rankFrom = square / 8;

                    if (rankFrom == 4) // 5th rank
                    {
                        if (fileFrom > 0 && square + 7 == game.enPassantSquare)
                            moveTargets |= 1UL << game.enPassantSquare;
                        if (fileFrom < 7 && square + 9 == game.enPassantSquare)
                            moveTargets |= 1UL << game.enPassantSquare;
                    }
                }

            }
            else
            {
                // Black pawn movement
                ulong oneAhead = fromBB >> 8;
                if ((oneAhead & empty) != 0)
                {
                    moveTargets |= oneAhead;

                    // Double push from rank 7 (0x00FF000000000000)
                    if ((fromBB & 0x00FF000000000000UL) != 0)
                    {
                        ulong twoAhead = fromBB >> 16;
                        if ((twoAhead & empty) != 0)
                            moveTargets |= twoAhead;
                    }
                }

                // Captures
                if ((fromBB & Board.FileA) == 0)
                    moveTargets |= (fromBB >> 7) & opp;
                if ((fromBB & Board.FileH) == 0)
                    moveTargets |= (fromBB >> 9) & opp;

               if (game.enPassantSquare != -1)
                {
                    int fileFrom = square % 8;
                    int rankFrom = square / 8;

                    if (rankFrom == 3) // 4th rank
                    {
                        if (fileFrom > 0 && square - 9 == game.enPassantSquare)
                            moveTargets |= 1UL << game.enPassantSquare;
                        if (fileFrom < 7 && square - 7 == game.enPassantSquare)
                            moveTargets |= 1UL << game.enPassantSquare;
                    }
                }

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

    public static Dictionary<int, ulong> KingMoves(GameState game, PieceColor color)
    {
        Board board = game.board;

        Dictionary<int, ulong> result = new Dictionary<int, ulong>();

        ulong bitboard = color == PieceColor.White ? board.WhiteKing : board.BlackKing;
        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (bitboard != 0)
        {
            int square = BitOperations.TrailingZeroCount(bitboard);
            bitboard = BitBoardUtils.ClearBit(bitboard, square);

            result[square] = KingLookUpTable[square] & ~ownPieces;
        }

        // Castling

        if (color == PieceColor.White)
        {
            // KingSide
            if (game.whiteCanShortCastle &&
                (board.WhiteKing & (1UL << 4)) != 0 && // King on e1
                (board.WhiteRooks & (1UL << 7)) != 0 && // Rook on h1
                (board.AllPieces & ((1UL << 5) | (1UL << 6))) == 0) // Squares f1, g1 empty
            {
                result[4] |= 1UL << 6; // King moves to g1
            }

            // Queen side
            if (game.whiteCanLongCastle &&
                (board.WhiteKing & (1UL << 4)) != 0 && // King on e1
                (board.WhiteRooks & (1UL << 0)) != 0 && // Rook on a1
                (board.AllPieces & ((1UL << 1) | (1UL << 2) | (1UL << 3))) == 0) // Squares b1, c1, d1 empty
            {
                result[4] |= 1UL << 2; // King moves to c1
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
                result[60] |= 1UL << 62; // King moves to g8
            }

            // QueenSide
            if (game.blackCanLongCastle &&
                (board.BlackKing & (1UL << 60)) != 0 && // King on e8
                (board.BlackRooks & (1UL << 56)) != 0 && // Rook on a8
                (board.AllPieces & ((1UL << 57) | (1UL << 58) | (1UL << 59))) == 0) // Squares b8, c8, d8 empty
            {
                result[60] |= 1UL << 58; // King moves to c8
            }
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