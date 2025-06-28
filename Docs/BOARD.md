
# <ins> Board Representation </ins>

The first and one of the most important things to do for a chess engine is to represent the board through code.

There are a few approaches, a few of which include:
 - 2D array of pieces or squares
 - 1D array of pieces or squares
 - Bitboards

The first two are much more simpler and readable than bitboards but are much, much slower.

The (simple) reason is because they take much more memory and need to be looped over every time to find a piece. 2D arrays need to be looped over twice.

So, almost all mordern chess engines, including *stockfish*, uses bitboards.

## Bitboards

Instead of using an array to represent the 64 squares, we use numbers. To be specific, 64-bit numbers.

A chess board has 64 squares, and 64 is a nice number for computers to work with (as its 2^5).

The **Unsigned Long** data type stores a 64 bit number, i.e., a number with 64 bits in binary.
We can use this for representing boards. If a bit is set to 1, that sqaure at is occupied and if not, then not.

                00011100.......100010
                |                   |
                |                   | -> represents square 0, or a1.
                | -> represents square 63, or h8.

                The indices for squares go from 0 at a1 to 1 at b1.....8 at a2.....63 at h8.

Now obviously, a one or zero can just tell us if a square is occupied or not. I can't, however, tell us anything else such as what piece occupies it.
For that, we use diffrent bitboards, 12 to be exact. 6 for each piece type x 2 colors.

`
    public ulong WhitePawns;
    public ulong WhiteKnights;
    public ulong WhiteBishops;
    public ulong WhiteRooks;
    public ulong WhiteQueens;
    public ulong WhiteKing;

    public ulong BlackPawns;
    public ulong BlackKnights;
    public ulong BlackBishops;
    public ulong BlackRooks;
    public ulong BlackQueens;
    public ulong BlackKing;
`