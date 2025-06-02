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
}