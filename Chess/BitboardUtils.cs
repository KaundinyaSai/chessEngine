
using System.Numerics;

public static class BitBoardUtils
{
    // Common shit:
    // 1UL is just binary representation of 1 (00..01)
    // 1UL << square index creates a ulong with one bit at specified index set to 1, by shifting the first bit to the 
    // specified index. (<< is the left shift operator, which.... shifts bits to the left)

    public static ulong SetBit(ulong bitboard, int squareIndex)
    {
        //OR 1UL << index into the bitboard (OR is a bitwise operator which sets the bit to one if either of the bits is 1 or both.)
        return bitboard | (1UL << squareIndex);
    }
    public static ulong ClearBit(ulong bitboard, int squareIndex)
    {
        // AND (&) bitwise operator returns one if the both the corresponding bits are 1, else it returns 0.
        // NOT (~) bitwise operator just switches all bits. 
        // ~(1UL << index) gives a ulong with only one bit set to zero at the specified index.
        // We AND it into the bitboard. So if the bit was 1, it would be set to zero. No change if it already was 0.

        return bitboard & ~(1UL << squareIndex);
    }

    public static int PopMS1B(ref ulong bitboard)
    {
        int square = BitOperations.TrailingZeroCount(bitboard);
        bitboard = ClearBit(bitboard, square);
        return square;
    }

    public static ulong ToggleBit(ulong bitboard, int squareIndex)
    {
        // XOR (^) is the bitwise operator which returns one if either of the bits are one, not neither or both.
        // XORing the bitboard with 1ul << i returns one at that index if that index was zero, or returns zero if it was 1.
        return bitboard ^ (1UL << squareIndex);
    }

    public static bool IsSquareOccupied(ulong bitboard, int squareIndex)
    {
        // Better to use an example to explain this one:
        // bitboard: 001010, index: 1
        // 1UL << i == 000010
        // So, 001010 & 000010 == 000010 (which is not equal to zero)
        // If that square was not occupied, then:
        // 001000 & 000010 == 000000 (equal to zero)

        return (bitboard & (1UL << squareIndex)) != 0;
    }

    public static int GetHammingWeight(ulong bitboard)
    {
        // Hamming weight is the number of set bits, ie the number of occupied squares.
        ulong x = bitboard; // make copy

        int result = 0;

        while (x > 0)
        {
            // Set the least significant set bit to 0.

            // Subtracting 1 from a binary number can do one of two things:
            // if it was odd (lsb was set to one), then it just sets the lsb to zero.
            // if it was even (lsb was zero), then it sets the least significant set bit to zero and all the bits to the right
            // of it to one.
            // When we AND it to the original bitboard, then it just sets the least significant set bit to 0.
            x &= x - 1;

            result++;

            // If x is o, then there is no more bits which are set to 1.
        }

        return result;
    }

    public static ushort GetBinaryFromPromotionType(PieceType? type)
    {
        switch (type)
        {
            case null: return 0b0000;
            case PieceType.Knight: return 0b0001;
            case PieceType.Bishop: return 0b0010;
            case PieceType.Rook: return 0b0100;
            case PieceType.Queen: return 0b1000;
            default: throw new ArgumentException("Not valid promotion type");
        }
    }
}