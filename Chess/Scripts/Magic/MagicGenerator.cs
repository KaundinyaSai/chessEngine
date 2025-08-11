namespace ChessEngine;

public static class MagicGenerator
{
    public static ulong FindMagic(int square, bool isRook, int relevantBits)
    {
        ulong mask = isRook ? Magic.RookMasks[square] : Magic.BishopMasks[square];
        var blockers = AllBlockerCombinations(mask);
        var used = new Dictionary<int, ulong>();
        int shift = 64 - relevantBits;

        Random rng = new();

        for (int attempt = 0; attempt < 10000000; attempt++)
        {
            ulong magic = RandomMagic(rng);

            used.Clear();
            bool fail = false;

            foreach (ulong b in blockers)
            {
                ulong masked = b & mask;
                int index = (int)((masked * magic) >> shift);

                if (used.ContainsKey(index))
                {
                    fail = true;
                    break;
                }
                used[index] = masked;
            }

            if (!fail)
            {
                Console.WriteLine($"0x{magic:X}, ");
                return magic;
            }
        }

        throw new Exception($"Failed to find magic for square {square}");
    }

    public static List<ulong> AllBlockerCombinations(ulong mask)
    {
        List<int> bits = BitBoardUtils.GetSetBits(mask);
        List<ulong> blockers = new();
        int count = bits.Count;

        int total = 1 << count;
        for (int i = 0; i < total; i++)
        {
            ulong blocker = 0;
            for (int j = 0; j < count; j++)
            {
                if ((i & (1 << j)) != 0)
                    blocker |= 1UL << bits[j];
            }
            blockers.Add(blocker);
        }

        return blockers;
    }

    static ulong RandomMagic(Random rng)
    {
        // Sparse magic pattern
        return (ulong)(rng.NextInt64() & rng.NextInt64() & rng.NextInt64());
    }
}

