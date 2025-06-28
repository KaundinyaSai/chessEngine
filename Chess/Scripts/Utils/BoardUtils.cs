
using System.Numerics;

public static class BoardUtils
{
    public static void PrintBitboard(ulong bitboard)
    {
        for (int rank = 7; rank >= 0; rank--)
        {
            Console.Write($"{rank + 1}  "); // Print rank number on the side
            for (int file = 0; file < 8; file++)
            {
                int squareIndex = rank * 8 + file;
                bool bitSet = (bitboard & (1UL << squareIndex)) != 0;
                Console.Write(bitSet ? "1 " : ". ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("   a b c d e f g h"); // Print file letters
        Console.WriteLine();
    }

    public static void PrintBoard(Board board)
    {
        for (int rank = 7; rank >= 0; rank--)
        {
            Console.Write($"{rank + 1}  ");
            for (int file = 0; file < 8; file++)
            {
                int squareIndex = rank * 8 + file;
                ulong mask = 1UL << squareIndex;
                if ((board.AllPieces & mask) == 0)
                {
                    Console.Write(". ");
                    continue;
                }
                try
                {
                    Piece piece = GetPieceAt(board, squareIndex);
                    char symbol = piece.type switch
                    {
                        PieceType.Pawn => piece.color == PieceColor.White ? 'P' : 'p',
                        PieceType.Knight => piece.color == PieceColor.White ? 'N' : 'n',
                        PieceType.Bishop => piece.color == PieceColor.White ? 'B' : 'b',
                        PieceType.Rook => piece.color == PieceColor.White ? 'R' : 'r',
                        PieceType.Queen => piece.color == PieceColor.White ? 'Q' : 'q',
                        PieceType.King => piece.color == PieceColor.White ? 'K' : 'k',
                        _ => '?'
                    };
                    Console.Write($"{symbol} ");
                }
                catch
                {
                    Console.Write("? ");
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        Console.WriteLine("   a b c d e f g h");
        Console.WriteLine();
    }

    public static ulong[] KnightLookUpInit()
    {
        ulong[] KnightLookUpTable = new ulong[64];
        for (int i = 0; i < 64; i++)
        {
            ulong moves = 0UL;
            ulong from = 1UL << i;

            // 8 possible moves with masks
            if ((from & ~Board.FileH) != 0) moves |= from << 17;
            if ((from & ~Board.FileA) != 0) moves |= from << 15;
            if ((from & ~(Board.FileH | Board.FileG)) != 0) moves |= from << 10;
            if ((from & ~(Board.FileA | Board.FileB)) != 0) moves |= from << 6;

            if ((from & ~Board.FileH) != 0) moves |= from >> 15;
            if ((from & ~Board.FileA) != 0) moves |= from >> 17;
            if ((from & ~(Board.FileH | Board.FileG)) != 0) moves |= from >> 6;
            if ((from & ~(Board.FileA | Board.FileB)) != 0) moves |= from >> 10;

            KnightLookUpTable[i] = moves;
        }

        return KnightLookUpTable;
    }

    public static ulong[] KingLookUpInit()
    {
        ulong[] KingLookUpTable = new ulong[64];
        for (int i = 0; i < 64; i++)
        {
            ulong moves = 0UL;
            ulong from = 1UL << i;

            if ((from & ~Board.FileH) != 0) moves |= from << 1;   // East
            if ((from & ~Board.FileA) != 0) moves |= from >> 1;   // West
            moves |= from << 8; // North
            moves |= from >> 8; // South

            if ((from & ~Board.FileH) != 0) moves |= from << 9;   // NE = N + E
            if ((from & ~Board.FileA) != 0) moves |= from << 7;   // NW = N + W
            if ((from & ~Board.FileH) != 0) moves |= from >> 7;   // SE = S + E
            if ((from & ~Board.FileA) != 0) moves |= from >> 9;

            KingLookUpTable[i] = moves;
        }
        return KingLookUpTable;
    }

    public static ulong[] PawnAttacksInit(PieceColor color)
    {
        ulong[] result = new ulong[64];
        for (int i = 0; i < 64; i++)
        {
            ulong moves = 0UL;
            ulong from = 1UL << i;

            if (color == PieceColor.White)
            {
                if ((from & Board.FileA) == 0) moves |= from << 7;
                if ((from & Board.FileH) == 0) moves |= from << 9;
                result[i] = moves;
            }
            else
            {
                if ((from & Board.FileH) == 0) moves |= from >> 7;
                if ((from & Board.FileA) == 0) moves |= from >> 9;
                result[i] = moves;
            }
        }

        return result;
    }

    public static ulong[] PawnPushTableInit(PieceColor color)
    {
        ulong[] result = new ulong[64];

        for (int i = 0; i < 64; i++)
        {
            ulong moves = 0UL;
            ulong from = 1UL << i;

            if (color == PieceColor.White)
            {
                moves |= from << 8;
                result[i] = moves;
            }
            else
            {
                moves |= from >> 8;
                result[i] = moves;
            }
        }

        return result;
    }

    public static Piece GetPieceAt(Board board, int squareIndex)
    {
        return board.Pieces[squareIndex] ?? throw new Exception($"No piece at {squareIndex}");
    }

    public static ref ulong GetBitboardFromPiece(Board board, Piece piece)
    {
        switch (piece.type)
        {
            case PieceType.Pawn:
                return ref piece.color == PieceColor.White ? ref board.WhitePawns : ref board.BlackPawns;
            case PieceType.Knight:
                return ref piece.color == PieceColor.White ? ref board.WhiteKnights : ref board.BlackKnights;
            case PieceType.Bishop:
                return ref piece.color == PieceColor.White ? ref board.WhiteBishops : ref board.BlackBishops;
            case PieceType.Rook:
                return ref piece.color == PieceColor.White ? ref board.WhiteRooks : ref board.BlackRooks;
            case PieceType.Queen:
                return ref piece.color == PieceColor.White ? ref board.WhiteQueens : ref board.BlackQueens;
            case PieceType.King:
                return ref piece.color == PieceColor.White ? ref board.WhiteKing : ref board.BlackKing;
            default:
                throw new ArgumentException("Invalid piece type");
        }
    }

    public static void ParseFen(GameState gameState, string fen)
    {
        // starting fen: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
        string[] parts = fen.Split(' ');
        if (parts.Length < 5)
            throw new ArgumentException("Invalid FEN: Not enough parts.");

        string activeColor = parts[1];
        string castlingRights = parts[2];
        string enPassantSquare = parts[3];
        string halfMoveClock = parts[4];

        CastlingRights rights = CastlingRights.None;

        if (castlingRights.Contains('K')) rights |= CastlingRights.WhiteKingside;
        if (castlingRights.Contains('Q')) rights |= CastlingRights.WhiteQueenside;
        if (castlingRights.Contains('k')) rights |= CastlingRights.BlackKingside;
        if (castlingRights.Contains('q')) rights |= CastlingRights.BlackQueenside;

        gameState.castlingRights = rights;


        gameState.halfMoveClock = int.TryParse(halfMoveClock, out var clock) ? clock : 0;

        gameState.plyNum = activeColor == "w" ? 0 : 1;
        gameState.enPassantSquare = ParseEnPassant(enPassantSquare);
    }

    static int ParseEnPassant(string ep)
    {
        if (ep == "-")
            return -1;

        int file = ep[0] - 'a';
        int rank = ep[1] - '1';

        return rank * 8 + file;
    }

    public static PieceType getPromotionType(int promotionType)
    {
        switch (promotionType)
        {
            case 0b0001: return PieceType.Knight;
            case 0b0010: return PieceType.Bishop;
            case 0b0100: return PieceType.Rook;
            case 0b1000: return PieceType.Queen;
            default: throw new ArgumentException($"Invalid promotion: {promotionType}");
        }
    }

    public static bool IsSquareAttacked(Board board, int square, PieceColor attackerColor)
    {
        // 1. Pawn attacks
        if ((attackerColor == PieceColor.White) && ((MoveGen.BlackPawnAttackTable[square] & board.WhitePawns) != 0)) return true;
        if ((attackerColor == PieceColor.Black) && (MoveGen.WhitePawnAttackTable[square] & board.BlackPawns) != 0) return true;

        // 2. Knight attacks
        if ((MoveGen.KnightLookUpTable[square] & (attackerColor == PieceColor.White ? board.WhiteKnights : board.BlackKnights)) != 0) return true;

        // 3. King attacks
        if ((MoveGen.KingLookUpTable[square] & (attackerColor == PieceColor.White ? board.WhiteKing : board.BlackKing)) != 0) return true;

        // 4. Sliding pieces
        if (IsAttackedBySlidingPieces(board, square, attackerColor)) return true;

        return false;
    }

    public static bool IsAttackedBySlidingPieces(Board board, int square, PieceColor attackerColor)
    {
        ulong occupancy = board.AllPieces;

        ulong rooks = attackerColor == PieceColor.White ? board.WhiteRooks : board.BlackRooks;
        ulong bishops = attackerColor == PieceColor.White ? board.WhiteBishops : board.BlackBishops;
        ulong queens = attackerColor == PieceColor.White ? board.WhiteQueens : board.BlackQueens;

        // Rook and queen attacks (orthogonal)
        ulong rookAttackers = rooks | queens;
        ulong rookAttacks = Magic.GetRookAttacks(square, occupancy);
        if ((rookAttacks & rookAttackers) != 0)
            return true;

        // Bishop and queen attacks (diagonal)
        ulong bishopAttackers = bishops | queens;
        ulong bishopAttacks = Magic.GetBishopAttacks(square, occupancy);
        if ((bishopAttacks & bishopAttackers) != 0)
            return true;

        return false;
    }

    public static string ConvertToAlg(Move move)
    {
        // eg: Move(12, 28) = e2e4
        string fromFile = ((char)('a' + (move.fromIndex % 8))).ToString();
        string fromRank = ((move.fromIndex / 8) + 1).ToString();
        string toFile = ((char)('a' + (move.toIndex % 8))).ToString();
        string toRank = ((move.toIndex / 8) + 1).ToString();

        string alg = $"{fromFile}{fromRank}{toFile}{toRank}";

        if (move.promotion != 0)
        {
            char promoChar = getPromotionType(move.promotion) switch
            {
                PieceType.Queen => 'q',
                PieceType.Rook => 'r',
                PieceType.Bishop => 'b',
                PieceType.Knight => 'n',
                _ => '?'
            };
            alg += promoChar;
        }

        return alg;
    }

    public static int GetKingSquare(Board board, PieceColor color)
    {
        ulong bb = color == PieceColor.White ? board.WhiteKing : board.BlackKing;

        return BitOperations.TrailingZeroCount(bb);
    }

    public static bool IsMoveCapture(Move move, Board board)
    {
        // A move is a capture if the destination square is occupied by an opponent's piece
        Piece piece = GetPieceAt(board, move.fromIndex);

        ulong targetSquare = 1UL << move.toIndex;
        ulong opponentPieces = piece.color == PieceColor.White ? board.BlackPieces : board.WhitePieces;

        return (targetSquare & opponentPieces) != 0;
    }

    public static bool IsMovePromotion(Move move, Board board)
    {
        // A move is a promotion if the piece is a pawn and it moves to the last rank

        Piece piece = GetPieceAt(board, move.fromIndex);
        return (piece.type == PieceType.Pawn) &&
               ((move.toIndex / 8 == 0 && piece.color == PieceColor.White) ||
                (move.toIndex / 8 == 7 && piece.color == PieceColor.Black));
    }
    
    public static bool IsMoveCastle(Move move, Board board)
    {
        // A move is a castle if the piece is a king and it moves two squares towards a rook
        Piece piece = GetPieceAt(board, move.fromIndex);
        if (piece.type != PieceType.King) return false;

        int rank = move.fromIndex / 8;
        int fileDiff = Math.Abs(move.toIndex % 8 - move.fromIndex % 8);

        return fileDiff == 2 && (rank == 0 || rank == 7); 
    }

}