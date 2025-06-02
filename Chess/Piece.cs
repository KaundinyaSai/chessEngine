namespace Chess;

public class Piece
{
    public Color pieceColor;
    public Type pieceType;

    public int file;
    public int rank;
    public int index => (rank * 8) + file; // formula to convert file and rank to a single index
    public int movedNum = 0; // number of moves made by the piece

    public List<Move> legalMoves = new List<Move>(); // list of legal moves for the piece

    public Piece(Color color, Type type, int file, int rank)
    {
        pieceColor = color;
        pieceType = type;
        this.file = file;
        this.rank = rank;
    }

    public void ChangePosition(int newFile, int newRank)
    {
        file = newFile;
        rank = newRank;
    }

    public void ChangePosition(int newIndex)
    {
        (file, rank) = Utils.GetFileRankFromIndex(newIndex, out file, out rank);
    }

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

    public bool IsEnemy(Piece other)
    {
        return other.pieceColor != pieceColor && other.pieceColor != Color.None;
    }
}
