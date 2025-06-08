
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

    public static Piece GetPieceAt(Board board, int squareIndex)
    {
        ulong mask = 1UL << squareIndex;

        if ((board.WhitePawns & mask) != 0)
            return new Piece(PieceType.Pawn, PieceColor.White);
        if ((board.WhiteKnights & mask) != 0)
            return new Piece(PieceType.Knight, PieceColor.White);
        if ((board.WhiteBishops & mask) != 0)
            return new Piece(PieceType.Bishop, PieceColor.White);
        if ((board.WhiteRooks & mask) != 0)
            return new Piece(PieceType.Rook, PieceColor.White);
        if ((board.WhiteQueens & mask) != 0)
            return new Piece(PieceType.Queen, PieceColor.White);
        if ((board.WhiteKing & mask) != 0)
            return new Piece(PieceType.King, PieceColor.White);

        if ((board.BlackPawns & mask) != 0)
            return new Piece(PieceType.Pawn, PieceColor.Black);
        if ((board.BlackKnights & mask) != 0)
            return new Piece(PieceType.Knight, PieceColor.Black);
        if ((board.BlackBishops & mask) != 0)
            return new Piece(PieceType.Bishop, PieceColor.Black);
        if ((board.BlackRooks & mask) != 0)
            return new Piece(PieceType.Rook, PieceColor.Black);
        if ((board.BlackQueens & mask) != 0)
            return new Piece(PieceType.Queen, PieceColor.Black);
        if ((board.BlackKing & mask) != 0)
            return new Piece(PieceType.King, PieceColor.Black);

        throw new Exception($"No piece found at index {squareIndex}");
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
}