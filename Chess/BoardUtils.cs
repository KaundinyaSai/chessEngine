
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
}