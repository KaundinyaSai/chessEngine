
public static class MoveGen
{
    public static readonly ulong[] KnightLookUpTable = BoardUtils.KnightLookUpInit();
    public static readonly ulong[] KingLookUpTable = BoardUtils.KingLookUpInit();
    public static readonly ulong[] WhitePawnAttackTable = BoardUtils.PawnAttacksInit(PieceColor.White);
    public static readonly ulong[] BlackPawnAttackTable = BoardUtils.PawnAttacksInit(PieceColor.Black);

    public static List<Move> AllPseudoLegalMoves(GameState game)
    {
        List<Move> moves = new List<Move>();
        PieceColor colorToMove = game.sideToMove;

        foreach (PieceType type in Enum.GetValues(typeof(PieceType)))
        {
            Piece piece = new Piece(type, colorToMove);
            moves.AddRange(MovesForPiece(game, piece, false));
        }

        return moves;
    }

    public static List<Move> MovesForPiece(GameState game, Piece piece, bool forAttackMap)
    {
        switch (piece.type)
        {
            case PieceType.Pawn:
                return PawnMoves(game, piece.color, forAttackMap);
            case PieceType.Knight:
                return KnightMoves(game.board, piece.color, forAttackMap);
            case PieceType.Bishop:
                return SlidingMovesMagic(game.board, piece.color, PieceType.Bishop, forAttackMap);
            case PieceType.Rook:
                return SlidingMovesMagic(game.board, piece.color, PieceType.Rook, forAttackMap);
            case PieceType.Queen:
                return SlidingMovesMagic(game.board, piece.color, PieceType.Queen, forAttackMap);
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
        if (pawns == 0)
        {
            return new List<Move>();
        }
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
                if (to / 8 == promotionRank)
                {
                    // Only promotions allowed to last rank
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
                    if (captureTo / 8 == promotionRank)
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
                    // Prevent en passant to promotion rank
                    if (epTo == game.enPassantSquare && (epTo / 8 != promotionRank))
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
        if (knights == 0)
        {
            return new List<Move>();
        }
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
        if (bitboard == 0)
        {
            return new List<Move>();
        }
        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (bitboard != 0)
        {
            int square = BitBoardUtils.PopMS1B(ref bitboard);
            ulong attacks = KingLookUpTable[square];
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
        
        if (color == PieceColor.White)
        {
            // KingSide
            if (game.whiteCanShortCastle &&
                (board.WhiteKing & (1UL << 4)) != 0 && // King on e1
                (board.WhiteRooks & (1UL << 7)) != 0 && // Rook on h1
                (board.AllPieces & ((1UL << 5) | (1UL << 6))) == 0 && // Squares f1, g1 empty
                !BoardUtils.IsSquareAttacked(board, 4, PieceColor.Black) && // e1 not attacked
                !BoardUtils.IsSquareAttacked(board, 5, PieceColor.Black) && // f1 not attacked
                !BoardUtils.IsSquareAttacked(board, 6, PieceColor.Black))   // g1 not attacked
            {
                moves.Add(new Move(4, 6)); // King moves to g1
            }

            // Queen side
            if (game.whiteCanLongCastle &&
                (board.WhiteKing & (1UL << 4)) != 0 && // King on e1
                (board.WhiteRooks & (1UL << 0)) != 0 && // Rook on a1
                (board.AllPieces & ((1UL << 1) | (1UL << 2) | (1UL << 3))) == 0 && // Squares b1, c1, d1 empty
                !BoardUtils.IsSquareAttacked(board, 4, PieceColor.Black) && // e1 not attacked
                !BoardUtils.IsSquareAttacked(board, 3, PieceColor.Black) && // d1 not attacked
                !BoardUtils.IsSquareAttacked(board, 2, PieceColor.Black))   // c1 not attacked
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
                (board.AllPieces & ((1UL << 61) | (1UL << 62))) == 0 && // Squares f8, g8 empty
                !BoardUtils.IsSquareAttacked(board, 60, PieceColor.White) && // e8 not attacked
                !BoardUtils.IsSquareAttacked(board, 61, PieceColor.White) && // f8 not attacked
                !BoardUtils.IsSquareAttacked(board, 62, PieceColor.White))   // g8 not attacked
            {
                moves.Add(new Move(60, 62)); // King moves to g8
            }

            // QueenSide
            if (game.blackCanLongCastle &&
                (board.BlackKing & (1UL << 60)) != 0 && // King on e8
                (board.BlackRooks & (1UL << 56)) != 0 && // Rook on a8
                (board.AllPieces & ((1UL << 57) | (1UL << 58) | (1UL << 59))) == 0 &&// Squares b8, c8, d8 empty
                !BoardUtils.IsSquareAttacked(board, 60, PieceColor.White) && // e8 not attacked
                !BoardUtils.IsSquareAttacked(board, 59, PieceColor.White) && // d8 not attacked
                !BoardUtils.IsSquareAttacked(board, 58, PieceColor.White))   // c8 not attacked
            {
                moves.Add(new Move(60, 58)); // King moves to c8
            }
        }
        return moves;
    }
    
    public static ulong SlidingAttack(int square, ulong blockers, bool isRook)
    {
        ulong attacks = 0UL;
        int[] directions = isRook
            ? new int[] { 8, -8, 1, -1 }
            : new int[] { 9, -9, 7, -7 };

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
    
    public static List<Move> SlidingMovesMagic(Board board, PieceColor color, PieceType type, bool forAttackMap)
    {
        List<Move> moves = new();

        ulong bitboard = type switch
        {
            PieceType.Bishop => color == PieceColor.White ? board.WhiteBishops : board.BlackBishops,
            PieceType.Rook => color == PieceColor.White ? board.WhiteRooks : board.BlackRooks,
            PieceType.Queen => color == PieceColor.White ? board.WhiteQueens : board.BlackQueens,
            _ => throw new ArgumentException("Not a sliding piece")
        };

        if (bitboard == 0) return moves;

        ulong ownPieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

        while (bitboard != 0)
        {
            int square = BitBoardUtils.PopMS1B(ref bitboard);
            ulong attacks = 0;

            if (type == PieceType.Bishop || type == PieceType.Queen)
            {
                ulong blockers = board.AllPieces;
                ulong masked = blockers & Magic.BishopMasks[square];
                int index = Magic.GetMagicIndex(masked, Magic.BISHOP_MAGICS[square], Magic.BishopShifts[square]);
                attacks |= Magic.BishopAttackTable[square][index];
            }

            if (type == PieceType.Rook || type == PieceType.Queen)
            {
                ulong blockers = board.AllPieces;
                ulong masked = blockers & Magic.RookMasks[square];
                int index = Magic.GetMagicIndex(masked, Magic.ROOK_MAGICS[square], Magic.RookShifts[square]);
                attacks |= Magic.RookAttackTable[square][index];
            }

            ulong legal = forAttackMap ? attacks : attacks & ~ownPieces;
            while (legal != 0)
            {
                int target = BitBoardUtils.PopMS1B(ref legal);
                moves.Add(new Move(square, target));
            }
        }

        return moves;
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