namespace Chess;

public static class Utils
{
    public static int GetIndexFromFileRank(int file, int rank)
    {
        return (rank * 8) + file;
    }
    public static (int file, int rank) GetFileRankFromIndex(int index, out int file, out int rank)
    {
        file = index % 8;
        rank = index / 8;
        return (file, rank);
    }

    public static List<int>[] LookUpTableInit((int, int)[] deltas) // deltas means offsets
    {
        List<int>[] result = new List<int>[64];
        for (int i = 0; i < 64; i++)
        {
            result[i] = new List<int>();

            int row = i / 8;
            int col = i % 8;

            foreach (var (dr, df) in deltas)
            {
                int newRow = row + dr;
                int newCol = col + df;

                // Prevents horizontal wrap-around
                if (newRow is >= 0 and < 8 && newCol is >= 0 and < 8)
                {
                    int targetIndex = newRow * 8 + newCol;
                    result[i].Add(targetIndex);
                }
            }
        }

        return result;
    }

    public static ulong GetBitboardFromIndex(int index)
    {
        // 1UL creates a ulong with the least significant bit set to 1, meaning 1 in binary (00....1)
        // When you shift it left (<< is the LEFT SHIFT bitwise operator, which basically shifts bits by some number) by 'index' 
        // positions, it effectively sets the bit at that index to 1.
        // It will shift the 1 at index 0 (Least significant bit) to the left by 'index' positions.
        // For example, if index is 3, it will result in 0000...0001000 (1 at index 3).

        return 1UL << index; // UL just means "unsigned long", or a unsigned 64 bit integer (ulong in c#).
    }
}