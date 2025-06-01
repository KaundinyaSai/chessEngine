namespace Chess;

public class Piece
{
    public Color pieceColor;
    public Type pieceType;

    public int file;
    public int rank;
    public int index => (rank * 8) + file; // formula to convert file and rank to a single index
    public int movedNum = 0; // number of moves made by the piece

    public Piece(Color color, Type type, int file, int rank)
    {
        pieceColor = color;
        pieceType = type;
        this.file = file;
        this.rank = rank;
    }

    public void changePosition(int newFile, int newRank)
    {
        file = newFile;
        rank = newRank;
    }

    /// <summary>
    /// Converts a character representation of a piece to a Piece object.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="file"></param>
    /// <param name="rank"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Piece GetPieceFromChar(char c, int file, int rank)
    {
        if (char.IsDigit(c))
        {
            return new Piece(Color.None, Type.None, file, rank);
        }

        Color color = char.IsUpper(c) ? Color.White : Color.Black;
        Type type = char.ToLower(c) switch
        {
            'p' => Type.Pawn,
            'r' => Type.Rook,
            'n' => Type.Knight,
            'b' => Type.Bishop,
            'q' => Type.Queen,
            'k' => Type.King,
            _ => throw new ArgumentException("Invalid piece character")
        };

        return new Piece(color, type, file, rank);
    }

    public static char GetCharFromPiece(Piece piece)
    {
        if (piece.pieceType == Type.None)
        {
            return '-'; // Represents an empty square
        }

        char c = piece.pieceType switch
        {
            Type.Pawn => 'p',
            Type.Rook => 'r',
            Type.Knight => 'n',
            Type.Bishop => 'b',
            Type.Queen => 'q',
            Type.King => 'k',
            _ => throw new ArgumentException("Invalid piece type")
        };

        return piece.pieceColor == Color.White ? char.ToUpper(c) : c;
    }
}
