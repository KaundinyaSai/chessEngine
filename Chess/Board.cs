
public class Board
{
    // So the position will be stored in bitboards. Basically 64 bit unsigned integers, where if a bit is set to 0 the square at
    // that index will not be occupied by any piece. If the bit is set to 1, then the square at that index is occupied.
    // Of course, just a 1 wont be able to tell info like which piece it is at that index, so we use diffrent bitboards for each
    // type of piece (including colors).
    // So that means 12 bitboards (6 pieces x 2 colors) in total

    // ulong in c# means unsigned long (ints can store at max 32 bits, so uint isnt an option)
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

    // And then three more bitboards for all the white and black pieces, and then for all pieces.
    public ulong WhitePieces => WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKing;
    public ulong BlackPieces => BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKing;
    public ulong AllPieces => WhitePieces | BlackPieces;

    public ulong EmptySquares => ~AllPieces;

    // The | symbol is the bitwise OR operator. It looks at each corresponding bit in the number, returns 0 if neither is 1 and
    // 1 if either or both are 1.

    // Oh and the LSB (least significant bit or the right most bit) is for square a1 and so on till MSB is h8.

    // for preventing wraparound
    public const ulong FileA = 0x0101010101010101UL;
    public const ulong FileH = 0x8080808080808080UL;
    public const ulong FileB = 0x0202020202020202UL;
    public const ulong FileG = 0x4040404040404040UL;

    public Board(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR")
    {
        // Clear bitboards
        WhitePawns = WhiteKnights = WhiteBishops = WhiteRooks = WhiteQueens = WhiteKing = 0;
        BlackPawns = BlackKnights = BlackBishops = BlackRooks = BlackQueens = BlackKing = 0;

        PopulateFromFen(fen);
    }

    public void PopulateFromFen(string fen)
    {
        string piecePlacement = fen.Split(" ")[0];
        string[] ranks = piecePlacement.Split("/");

        for (int fenRank = 0; fenRank < 8; fenRank++)
        {
            int file = 0;
            foreach (char piece in ranks[fenRank])
            {
                if (char.IsDigit(piece))
                {
                    file += int.Parse(piece.ToString());
                }
                else
                {
                    int rank = 7 - fenRank; // FEN starts from rank 8 (top), index 0, so we have to reverse.
                    int index = rank * 8 + file;
                    ulong mask = 1UL << index; // Make a ulong with one bit at index to one

                    switch (piece)
                    {
                        // OR it into the corresponding bitboard
                        case 'P': WhitePawns |= mask; break;
                        case 'N': WhiteKnights |= mask; break;
                        case 'B': WhiteBishops |= mask; break;
                        case 'R': WhiteRooks |= mask; break;
                        case 'Q': WhiteQueens |= mask; break;
                        case 'K': WhiteKing |= mask; break;
                        case 'p': BlackPawns |= mask; break;
                        case 'n': BlackKnights |= mask; break;
                        case 'b': BlackBishops |= mask; break;
                        case 'r': BlackRooks |= mask; break;
                        case 'q': BlackQueens |= mask; break;
                        case 'k': BlackKing |= mask; break;
                        default: throw new ArgumentException("Invalid FEN character");
                    }
                    file++;
                }
            }
        }
    }
}

public enum PieceColor
{
    White,
    Black
}