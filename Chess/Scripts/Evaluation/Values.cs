public static class Values
{
    // Usual material values for each piece
    public static readonly int PAWN_VALUE = 100;
    public static readonly int KNIGHT_VALUE = 300;
    public static readonly int BISHOP_VALUE = 320; // Usally considred more valuable than a knight
    public static readonly int ROOK_VALUE = 500;
    public static readonly int QUEEN_VALUE = 900;
    public static readonly int KING_VALUE = 0; // 0 for evaluation


    // PIECE SQUARE TABLES (PSQTs)

    // Basically show what squares a piece is usally better on and which squares a piece isnt the best on
    // Ofc, where a piece is "good" depends alot on the positon,  but this is the genral idea.
    // Example: Centres are good for pawns (and almost all pieces), knights are shit on edges, kings should just be castles in 
    // the opening, etc...
    // For kings, we will have 2 tables: one for the opening, and one for the endgame, as in the endgame, kings should be alot
    // more active.

    // Values "borrowed" from Chess Programming Wiki.

    public static readonly int[] PawnTable = [
        // For pawns, the centre pawns are hevaily encouraged to occupy the centre (naturally, as one of the first principles 
        // we get taught in chess is to occupy the centre.)
        // The flank pawns arent encourages too much. The pawns on b, c, f and g ranks are discouraged to move 1 square or 2
        // as the king castles there and moving them too much might be bad for the king's safety.
        // If a pawn is already past the middle of the board, then they are HEAVAILY encouraged to advance because of promotion.

        // I thought of adding two pawn tables, one for the opening/middlegame and one for the endgame, but for now this should
        // suffice.

        0,  0,  0,  0,  0,  0,  0,  0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
        5,  5, 10, 25, 25, 10,  5,  5,
        0,  0,  0, 20, 20,  0,  0,  0,
        5, -5,-10,  0,  0,-10, -5,  5,
        5, 10, 10,-20,-20, 10, 10,  5,
        0,  0,  0,  0,  0,  0,  0,  0
    ];

    public static readonly int[] KnightTable = [
        // Knights are much simpler; Centre goood, edge bad (in a nutshell atleast)

        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50,
    ];

    public static readonly int[] BishopTable = [
        // Again, center good and corner bad.
        // The more squares a bishop can control from a square, the better.

        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  0,  0,  0,  0,  0, 0,-10,
        -20,-10,-10,-10,-10,-10,-10,-20,
    ];

    public static readonly int[] RookTable = [
        // AGAIN, center good.
        // The 7th rank is given bonuses (common chess principal)

        0,  0,  0,  0,  0,  0,  0,  0,
        5, 10, 10, 10, 10, 10, 10,  5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  5,  5,  0,  0, -5,
        -5,  0,  0,  5,  5,  0,  0, -5,
        -5,  0,  0,  5,  5,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        0,  0,  0,  5,  5,  0,  0,  0
    ];

    public static readonly int[] QueenTable = [
        // Shouldnt need to explain much here i think

        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
        -5,  0,  5,  5,  5,  5,  0, -5,
        0,  0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    ];

    public static readonly int[] KingTableOpening = [
        // Now for the king, in the opening, its mostly just to be castled and safe behind pawns, and to NEVER go in the centre.
        // b1 is marked 30 so the king can protect the unguarded pawn if it long castles

        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -30,-40,-40,-50,-50,-40,-40,-30,
        -20,-30,-30,-40,-40,-30,-30,-20,
        -10,-20,-20,-20,-20,-20,-20,-10,
        20, 20,  0,  0,  0,  0, 20, 20,
        20, 30, 50,  0,  0, 10, 50, 20
    ];

    public static readonly int[] KingTableEndgame = [
        // In the endgame, things change. Kings are a vital part of the endgame and need to be active (in things like pawn
        // endgames and such).

        -50,-40,-30,-20,-20,-30,-40,-50,
        -30,-20,-10,  0,  0,-10,-20,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-30,  0,  0,  0,  0,-30,-30,
        -50,-30,-30,-30,-30,-30,-30,-50
    ];

    // For black, just flip the rank
    public static int Mirror(int square)
    {
        int rank = square / 8;
        int file = square % 8;
        int mirroredRank = 7 - rank;
        return (mirroredRank * 8) + file;
    }

    // Value for mate
    public static readonly int MATE_VALUE = 100000;

}