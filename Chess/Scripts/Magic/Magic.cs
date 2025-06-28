
public static class Magic
{
    // Magic numbers
    public static readonly ulong[] ROOK_MAGICS = [
        0x4080002114804004,
        0x1340002141481000,
        0x80200104708080,
        0x8004080080104A,
        0x100080083140D00,
        0x200090200140810,
        0xA0020742018200,
        0xE00044820820104,
        0x801002044800105,
        0x261400220007000,
        0x808011888020A0,
        0x9001001002088,
        0x4101400100800,
        0x1001204010008,
        0x40B002200010044,
        0x2802440800100,
        0x218000804000,
        0x4100420061020080,
        0x10008020001080,
        0x200808010005804,
        0x12041020200A0010,
        0x420081003000440,
        0x1408040002081001,
        0x4041420020804401,
        0x412084800A4000,
        0x4200080400080,
        0x20A1008A0020111,
        0x424080010001200,
        0x2000A80280040080,
        0x80400802A0080,
        0xA8100010012000C,
        0x28030200004084,
        0x800020C6200010,
        0x648101004000,
        0x1B0002000820080,
        0xA0100141041800,
        0x10042216001002,
        0x244000201000824,
        0x501104104001A08,
        0x147000045000082,
        0x1004074413000,
        0x224002A001414010,
        0x402034080120020,
        0x100900059010020,
        0x40000C0202460022,
        0x411040010C804002,
        0x8030002008080,
        0x200140A4020001,
        0x1020210600844200,
        0x40014221018300,
        0x200200010018080,
        0x2004A9000610100,
        0x1880050408000B00,
        0x120020011040080,
        0x1000E00040100,
        0x2080090080C40200,
        0x210420109102082,
        0x1080C00081210011,
        0x40502001010941,
        0x143041000282101,
        0x502F001088000205,
        0x40A000410080112,
        0x15450180082032C,
        0x4052405098242,
    ];
    public static readonly ulong[] BISHOP_MAGICS = { 0x2008810108110100,
        0x282042904090000,
        0x4080081028080,
        0xA4040082180000,
        0x902021020002202,
        0x901008020000,
        0x442010120100800,
        0x2800910801046002,
        0x290401094110050,
        0x4800241020920080,
        0x1010824811002020,
        0xC0186110A0200,
        0x48240148184010,
        0xB00060084096041,
        0x110404426084004,
        0x100022110C6002,
        0x40204010A0090904,
        0x1100802302280500,
        0x108201210202020,
        0x4200A21440108000,
        0x120900082008120,
        0x1805072088804080,
        0x2110088008A10,
        0x400840C484242104,
        0x1102801400224D8,
        0x818002040980,
        0x202480120872050,
        0x810040048C00108,
        0xA802020040504,
        0x22440205100080,
        0x808025089820102,
        0x102822002030400,
        0x1408201000080200,
        0x401011014080080,
        0x201204004080,
        0x200032200000808,
        0xB001004510040,
        0x4044189080158828,
        0x1000422800A122,
        0x4004940100008081,
        0x20880808814000,
        0x205905080808800,
        0x40D42080C00800,
        0x24160200800161,
        0x3000148A00800040,
        0x699200A00100080,
        0x40804382B0100,
        0x2006020401000060,
        0x1007040206405000,
        0x5082024248040048,
        0x4058020630080,
        0x840082024011C02,
        0x4000480902000,
        0x1802083000481000,
        0x1520201202006D20,
        0x8080104002008,
        0x202218200820,
        0x4401108580909021,
        0x801100100451000,
        0x2000420610,
        0x100108000C104C00,
        0x400488100900,
        0x409001910100,
        0x20206082014045, };

    // Shifts
    public static readonly int[] RookShifts = [52, 51, 51, 51, 51, 51, 51, 52, 51, 50, 50, 50, 50, 50, 50, 51, 51, 50, 50, 50, 50, 50, 50, 51, 51, 50, 50, 50, 50, 50, 50, 51, 51, 50, 50, 50, 50, 50, 50, 51, 51, 50, 50, 50, 50, 50, 50, 51, 51, 50, 50, 50, 50, 50, 50, 51, 52, 51, 51, 51, 51, 51, 51, 52];
    public static readonly int[] BishopShifts = [
        58, 59, 59, 59, 59, 59, 59, 58, 59, 59, 57, 57, 57, 57, 59, 59, 59, 57, 55, 55, 55, 55, 57, 59, 59, 57, 55, 53, 53, 55, 57, 59, 59, 57, 55, 53, 53, 55, 57, 59, 59, 57, 55, 55, 55, 55, 57, 59, 59, 59, 57, 57, 57, 57, 59, 59, 58, 59, 59, 59, 59, 59, 59, 58
    ];

    // Relevant bits
    public static readonly int[] RookRelevantBits = [
        12, 11, 11, 11, 11, 11, 11, 12,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        12, 11, 11, 11, 11, 11, 11, 12
    ];

    public static readonly int[] BishopRelevantBits = [
        6, 5, 5, 5, 5, 5, 5, 6,
        5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 5, 5, 5, 5, 5, 5,
        6, 5, 5, 5, 5, 5, 5, 6
    ];

    // Masks (For each square, a bitboard of all the blockers, excluding edge pieces)

    public static readonly ulong[] RookMasks = new ulong[64]
    {
        0x000101010101017E, 0x000202020202027C, 0x000404040404047A, 0x0008080808080876,
        0x001010101010106E, 0x002020202020205E, 0x004040404040403E, 0x008080808080807E,
        0x0001010101017E00, 0x0002020202027C00, 0x0004040404047A00, 0x0008080808087600,
        0x0010101010106E00, 0x0020202020205E00, 0x0040404040403E00, 0x0080808080807E00,
        0x00010101017E0100, 0x00020202027C0200, 0x00040404047A0400, 0x0008080808760800,
        0x00101010106E1000, 0x00202020205E2000, 0x00404040403E4000, 0x00808080807E8000,
        0x000101017E010100, 0x000202027C020200, 0x000404047A040400, 0x0008080876080800,
        0x001010106E101000, 0x002020205E202000, 0x004040403E404000, 0x008080807E808000,
        0x0001017E01010100, 0x0002027C02020200, 0x0004047A04040400, 0x0008087608080800,
        0x0010106E10101000, 0x0020205E20202000, 0x0040403E40404000, 0x0080807E80808000,
        0x00017E0101010100, 0x00027C0202020200, 0x00047A0404040400, 0x0008760808080800,
        0x00106E1010101000, 0x00205E2020202000, 0x00403E4040404000, 0x00807E8080808000,
        0x007E010101010100, 0x007C020202020200, 0x007A040404040400, 0x0076080808080800,
        0x006E101010101000, 0x005E202020202000, 0x003E404040404000, 0x007E808080808000,
        0x7E01010101010100, 0x7C02020202020200, 0x7A04040404040400, 0x7608080808080800,
        0x6E10101010101000, 0x5E20202020202000, 0x3E40404040404000, 0x7E80808080808000
    };

    public static readonly ulong[] BishopMasks = new ulong[64]
    {
        18049651735527936,  70506452091904,     275415828992,      1075975168,
        38021120,           8657588224,         2216338399232,     567382630219776,
        9024825867763712,   18049651735527424,  70506452221952,    275449643008,
        9733406720,         2216342585344,      567382630203392,   1134765260406784,
        4512412933816832,   9024825867633664,   18049651768822272, 70515108615168,
        2491752130560,      567383701868544,    1134765256220672,  2269530512441344,
        2256206450263040,   4512412900526080,   9024834391117824,  18051867805491712,
        637888545440768,    1135039602493440,   2269529440784384,  4539058881568768,
        1128098963916800,   2256197927833600,   4514594912477184,  9592139778506752,
        19184279556981248,  2339762086609920,   4538784537380864,  9077569074761728,
        562958610993152,    1125917221986304,   2814792987328512,  5629586008178688,
        11259172008099840,  22518341868716544,  9007336962655232,  18014673925310464,
        2216338399232,      4432676798464,      11064376819712,    22137335185408,
        44272556441600,     87995357200384,     35253226045952,    70506452091904,
        567382630219776,    1134765260406784,   2832480465846272,  5667157807464448,
        11333774449049600,  22526811443298304,  9024825867763712,  18049651735527936
    };



    // Generate all blocker combinations
    static List<ulong> AllBlockerCombinations(ulong mask)
    {
        List<int> relevantBits = BitBoardUtils.GetSetBits(mask); // indexes of bits that are 1 in mask
        int numBits = relevantBits.Count;
        List<ulong> blockers = new List<ulong>();

        int totalCombinations = 1 << numBits; // 2^n

        for (int i = 0; i < totalCombinations; i++)
        {
            ulong blocker = 0;
            for (int j = 0; j < numBits; j++)
            {
                if ((i & (1 << j)) != 0)
                    blocker |= 1UL << relevantBits[j];
            }
            blockers.Add(blocker);
        }

        return blockers;
    }

    // The attack tables
    public static ulong[][] RookAttackTable = new ulong[64][];
    public static ulong[][] BishopAttackTable = new ulong[64][];

    public static void AttackTablesInit()
    {
        RookAttackTableInit();
        BishopAttackTableInit();
    }

    static void RookAttackTableInit()
    {
        for (int i = 0; i < 64; i++)
        {
            var blockers = AllBlockerCombinations(RookMasks[i]);
            int shift = RookShifts[i];
            int tableSize = 1 << (64 - shift);
            RookAttackTable[i] = new ulong[tableSize];

            foreach (var blocker in blockers)
            {
                ulong masked = blocker & RookMasks[i];
                int index = GetMagicIndex(masked, ROOK_MAGICS[i], shift);

                if (RookAttackTable[i][index] != 0)
                {
                    Console.WriteLine($"[Rook {i}] Collision at index {index}");
                    throw new Exception("Magic index collision — bad magic number?");
                }

                RookAttackTable[i][index] = MoveGen.SlidingAttack(i, blocker, isRook: true);
            }
        }
    }

    static void BishopAttackTableInit()
    {
        for (int i = 0; i < 64; i++)
        {
            var blockers = AllBlockerCombinations(BishopMasks[i]);
            int shift = BishopShifts[i];
            int tableSize = 1 << (64 - shift);
            BishopAttackTable[i] = new ulong[tableSize];

            foreach (var blocker in blockers)
            {
                ulong masked = blocker & BishopMasks[i];
                int index = GetMagicIndex(masked, BISHOP_MAGICS[i], shift);

                if (BishopAttackTable[i][index] != 0)
                {
                    Console.WriteLine($"[Bishsop {i}] Collision at index {index}");
                    throw new Exception("Magic index collision — bad magic number?");
                }

                BishopAttackTable[i][index] = MoveGen.SlidingAttack(i, blocker, isRook: false);
            }
        }
    }

    public static int GetMagicIndex(ulong maskedBlockers, ulong magic, int shift)
    {
        return (int)((maskedBlockers * magic) >> shift);
    }

    public static int GetIndex(ulong blockers, ulong mask)
    {
        int index = 0;
        int bit = 0;
        for (int i = 0; i < 64; i++)
        {
            if ((mask & (1UL << i)) != 0)
            {
                if ((blockers & (1UL << i)) != 0)
                    index |= 1 << bit;
                bit++;
            }
        }
        return index;
    }
    
    public static ulong GetRookAttacks(int square, ulong occupancy)
    {
        ulong blockers = occupancy & RookMasks[square];
        int index = GetMagicIndex(blockers, ROOK_MAGICS[square], RookShifts[square]);
        return RookAttackTable[square][index];
    }

    public static ulong GetBishopAttacks(int square, ulong occupancy)
    {
        ulong blockers = occupancy & BishopMasks[square];
        int index = GetMagicIndex(blockers, BISHOP_MAGICS[square], BishopShifts[square]);
        return BishopAttackTable[square][index];
    }


}