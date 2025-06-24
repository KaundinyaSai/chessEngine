public static class PieceUtils
{
    public static int GetValue(PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn: return Values.PAWN_VALUE;
            case PieceType.Knight: return Values.KNIGHT_VALUE;
            case PieceType.Bishop: return Values.BISHOP_VALUE;
            case PieceType.Rook: return Values.ROOK_VALUE;
            case PieceType.Queen: return Values.QUEEN_VALUE;
            case PieceType.King: return Values.KING_VALUE;
            default: return 0;
        }
    }

    public static int PSQTFor(ulong bitboard, int[] table, bool mirror = false)
    {
        int total = 0;
        while (bitboard != 0)
        {
            int square = BitBoardUtils.PopLS1B(ref bitboard);
            int index = mirror ? Values.Mirror(square) : square;
            total += table[index];
        }
        return total;
    }

}