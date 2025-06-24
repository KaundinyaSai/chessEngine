
public static class MoveGen
{
    public static readonly ulong[] KnightLookUpTable = BoardUtils.KnightLookUpInit();
    public static readonly ulong[] KingLookUpTable = BoardUtils.KingLookUpInit();
    public static readonly ulong[] WhitePawnAttackTable = BoardUtils.PawnAttacksInit(PieceColor.White);
    public static readonly ulong[] BlackPawnAttackTable = BoardUtils.PawnAttacksInit(PieceColor.Black);

    public static void AllPseudoLegalMoves(GameState game, Span<Move> moves, ref int count)
    {
        PieceColor colorToMove = game.sideToMove;

        foreach (PieceType type in Enum.GetValues(typeof(PieceType)))
        {
            Piece piece = new Piece(type, colorToMove);
            MovesForPiece(game, piece, false, moves, ref count);
        }
    }

    public static void MovesForPiece(GameState game, Piece piece, bool forAttackMap, Span<Move> moves, ref int count)
    {
        switch (piece.type)
        {
            case PieceType.Pawn:
                PawnMoves(game, piece.color, moves, ref count);
                break;
            case PieceType.Knight:
                KnightMoves(game.board, piece.color, forAttackMap, moves, ref count);
                break;
            case PieceType.Bishop:
                BishopMovesMagic(game.board, piece.color, forAttackMap, moves, ref count);
                break;
            case PieceType.Rook:
                RookMovesMagic(game.board, piece.color, forAttackMap, moves, ref count);
                break;
            case PieceType.Queen:
                QueenMovesMagic(game.board, piece.color, forAttackMap, moves, ref count);
                break;
            case PieceType.King:
                KingMoves(game, piece.color, forAttackMap, moves, ref count);
                break;
        }
    }

    public static void PawnMoves(GameState game, PieceColor color, Span<Move> moves, ref int count)
    {
        Board board = game.board;
        ulong pawns = color == PieceColor.White ? board.WhitePawns : board.BlackPawns;
        if (pawns == 0) return;

        ulong empty = board.EmptySquares;
        ulong opp = color == PieceColor.White ? board.BlackPieces : board.WhitePieces;
        int forward = color == PieceColor.White ? 8 : -8;
        int startRank = color == PieceColor.White ? 1 : 6;
        int promotionRank = color == PieceColor.White ? 7 : 0;
        int enPassantRank = color == PieceColor.White ? 4 : 3;

        while (pawns != 0)
        {
            int square = BitBoardUtils.PopLS1B(ref pawns);
            int rank = square / 8;
            int file = square % 8;

            int to = square + forward;
            if (to >= 0 && to < 64 && ((empty & (1UL << to)) != 0))
            {
                if (to / 8 == promotionRank)
                {
                    foreach (var promo in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                        moves[count++] = new Move(square, to, promo);
                }
                else
                {
                    moves[count++] = new Move(square, to);
                    if (rank == startRank)
                    {
                        int to2 = square + 2 * forward;
                        int mid = square + forward;
                        if ((empty & (1UL << to2)) != 0 && (empty & (1UL << mid)) != 0)
                            moves[count++] = new Move(square, to2);
                    }
                }
            }

            for (int df = -1; df <= 1; df += 2)
            {
                int captureFile = file + df;
                if ((uint)captureFile > 7) continue;
                int captureTo = square + forward + df;
                if ((uint)captureTo >= 64) continue;

                if ((opp & (1UL << captureTo)) != 0)
                {
                    if (captureTo / 8 == promotionRank)
                    {
                        foreach (var promo in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
                            moves[count++] = new Move(square, captureTo, promo);
                    }
                    else
                    {
                        moves[count++] = new Move(square, captureTo);
                    }
                }
            }

            if (game.enPassantSquare != -1 && rank == enPassantRank)
            {
                for (int df = -1; df <= 1; df += 2)
                {
                    int epFile = file + df;
                    if ((uint)epFile > 7) continue;
                    int epTo = square + forward + df;
                    if (epTo == game.enPassantSquare && (epTo / 8 != promotionRank))
                    {
                        int epPawnSquare = square + df;
                        ulong epPawnMask = 1UL << epPawnSquare;
                        if ((opp & epPawnMask) != 0)
                            moves[count++] = new Move(square, epTo);
                    }
                }
            }
        }
    }


    public static void KnightMoves(Board board, PieceColor color, bool forAttackMap, Span<Move> moves, ref int count)
    {
        ulong knights = color == PieceColor.White ? board.WhiteKnights : board.BlackKnights;
        if (knights == 0) return;

        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (knights != 0)
        {
            int square = BitBoardUtils.PopLS1B(ref knights);
            ulong attacks = KnightLookUpTable[square];
            while (attacks != 0)
            {
                int squareToAdd = BitBoardUtils.PopLS1B(ref attacks);
                bool shouldAdd = forAttackMap || ((1UL << squareToAdd) & ownPieces) == 0;
                if (shouldAdd)
                {
                    moves[count++] = new Move(square, squareToAdd);
                }
            }
        }
    }


    public static void KingMoves(GameState game, PieceColor color, bool forAttackMap, Span<Move> moves, ref int count)
    {
        Board board = game.board;
        ulong bitboard = color == PieceColor.White ? board.WhiteKing : board.BlackKing;
        if (bitboard == 0) return;

        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (bitboard != 0)
        {
            int square = BitBoardUtils.PopLS1B(ref bitboard);
            ulong attacks = KingLookUpTable[square];
            while (attacks != 0)
            {
                int squareToAdd = BitBoardUtils.PopLS1B(ref attacks);
                bool shouldAdd = forAttackMap || ((1UL << squareToAdd) & ownPieces) == 0;
                if (shouldAdd)
                {
                    moves[count++] = new Move(square, squareToAdd);
                }
            }
        }

        if (forAttackMap) return;

        var rights = game.castlingRights;

        if (color == PieceColor.White)
        {
            // White kingside (O-O)
            if ((rights & CastlingRights.WhiteKingside) != 0 &&
                (board.WhiteKing & (1UL << 4)) != 0 &&
                (board.WhiteRooks & (1UL << 7)) != 0 &&
                (board.AllPieces & ((1UL << 5) | (1UL << 6))) == 0 &&
                !BoardUtils.IsSquareAttacked(board, 4, PieceColor.Black) &&
                !BoardUtils.IsSquareAttacked(board, 5, PieceColor.Black) &&
                !BoardUtils.IsSquareAttacked(board, 6, PieceColor.Black))
            {
                moves[count++] = new Move(4, 6); // O-O
            }

            // White queenside (O-O-O)
            if ((rights & CastlingRights.WhiteQueenside) != 0 &&
                (board.WhiteKing & (1UL << 4)) != 0 &&
                (board.WhiteRooks & (1UL << 0)) != 0 &&
                (board.AllPieces & ((1UL << 1) | (1UL << 2) | (1UL << 3))) == 0 &&
                !BoardUtils.IsSquareAttacked(board, 4, PieceColor.Black) &&
                !BoardUtils.IsSquareAttacked(board, 3, PieceColor.Black) &&
                !BoardUtils.IsSquareAttacked(board, 2, PieceColor.Black))
            {
                moves[count++] = new Move(4, 2); // O-O-O
            }
        }
        else
        {
            // Black kingside (O-O)
            if ((rights & CastlingRights.BlackKingside) != 0 &&
                (board.BlackKing & (1UL << 60)) != 0 &&
                (board.BlackRooks & (1UL << 63)) != 0 &&
                (board.AllPieces & ((1UL << 61) | (1UL << 62))) == 0 &&
                !BoardUtils.IsSquareAttacked(board, 60, PieceColor.White) &&
                !BoardUtils.IsSquareAttacked(board, 61, PieceColor.White) &&
                !BoardUtils.IsSquareAttacked(board, 62, PieceColor.White))
            {
                moves[count++] = new Move(60, 62); // O-O
            }

            // Black queenside (O-O-O)
            if ((rights & CastlingRights.BlackQueenside) != 0 &&
                (board.BlackKing & (1UL << 60)) != 0 &&
                (board.BlackRooks & (1UL << 56)) != 0 &&
                (board.AllPieces & ((1UL << 57) | (1UL << 58) | (1UL << 59))) == 0 &&
                !BoardUtils.IsSquareAttacked(board, 60, PieceColor.White) &&
                !BoardUtils.IsSquareAttacked(board, 59, PieceColor.White) &&
                !BoardUtils.IsSquareAttacked(board, 58, PieceColor.White))
            {
                moves[count++] = new Move(60, 58); // O-O-O
            }
        }
    }


    
    public static ulong SlidingAttack(int square, ulong blockers, bool isRook)
    {
        ulong attacks = 0UL;
        int[] directions = isRook
            ? [8, -8, 1, -1]
            : [9, -9, 7, -7];

        foreach (int dir in directions)
        {
            int next = square;

            while (true)
            {
                int from = next;
                next += dir;

                if (next < 0 || next >= 64 || IsWrapAround(dir, from))
                    break;

                ulong bit = 1UL << next;
                attacks |= bit;

                if ((blockers & bit) != 0)
                    break;
            }
        }

        return attacks;
    }
    public static void BishopMovesMagic(Board board, PieceColor color, bool forAttackMap, Span<Move> moves, ref int count)
    {
        ulong bitboard = color == PieceColor.White ? board.WhiteBishops : board.BlackBishops;
        if (bitboard == 0) return;

        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;
        ulong occupancy = board.AllPieces;

        while (bitboard != 0)
        {
            int square = BitBoardUtils.PopLS1B(ref bitboard);
            ulong masked = occupancy & Magic.BishopMasks[square];
            int index = Magic.GetMagicIndex(masked, Magic.BISHOP_MAGICS[square], Magic.BishopShifts[square]);
            ulong attacks = Magic.BishopAttackTable[square][index];
            ulong legal = forAttackMap ? attacks : attacks & ~ownPieces;

            while (legal != 0)
            {
                int target = BitBoardUtils.PopLS1B(ref legal);
                moves[count++] = new Move(square, target);
            }
        }
    }


    public static void RookMovesMagic(Board board, PieceColor color, bool forAttackMap, Span<Move> moves, ref int count)
    {
        ulong bitboard = color == PieceColor.White ? board.WhiteRooks : board.BlackRooks;
        if (bitboard == 0) return;

        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;
        ulong occupancy = board.AllPieces;

        while (bitboard != 0)
        {
            int square = BitBoardUtils.PopLS1B(ref bitboard);
            ulong masked = occupancy & Magic.RookMasks[square];
            int index = Magic.GetMagicIndex(masked, Magic.ROOK_MAGICS[square], Magic.RookShifts[square]);
            ulong attacks = Magic.RookAttackTable[square][index];
            ulong legal = forAttackMap ? attacks : attacks & ~ownPieces;

            while (legal != 0)
            {
                int target = BitBoardUtils.PopLS1B(ref legal);
                moves[count++] = new Move(square, target);
            }
        }
    }


   public static void QueenMovesMagic(Board board, PieceColor color, bool forAttackMap, Span<Move> moves, ref int count)
    {
        ulong bitboard = color == PieceColor.White ? board.WhiteQueens : board.BlackQueens;
        if (bitboard == 0) return;

        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;
        ulong occupancy = board.AllPieces;

        while (bitboard != 0)
        {
            int square = BitBoardUtils.PopLS1B(ref bitboard);

            ulong bMasked = occupancy & Magic.BishopMasks[square];
            int bIndex = Magic.GetMagicIndex(bMasked, Magic.BISHOP_MAGICS[square], Magic.BishopShifts[square]);
            ulong bAttacks = Magic.BishopAttackTable[square][bIndex];

            ulong rMasked = occupancy & Magic.RookMasks[square];
            int rIndex = Magic.GetMagicIndex(rMasked, Magic.ROOK_MAGICS[square], Magic.RookShifts[square]);
            ulong rAttacks = Magic.RookAttackTable[square][rIndex];

            ulong attacks = bAttacks | rAttacks;
            ulong legal = forAttackMap ? attacks : attacks & ~ownPieces;

            while (legal != 0)
            {
                int target = BitBoardUtils.PopLS1B(ref legal);
                moves[count++] = new Move(square, target);
            }
        }
    }


    public static bool IsWrapAround(int dir, int fromSquare)
    {
        ulong fromBB = 1UL << fromSquare;
        switch (dir)
        {
            case 1: return (fromBB & Board.FileH) != 0;
            case -1: return (fromBB & Board.FileA) != 0;
            case 9:
            case -7: return (fromBB & Board.FileH) != 0;
            case 7:
            case -9: return (fromBB & Board.FileA) != 0;
            default: return false;
        }
    }

}