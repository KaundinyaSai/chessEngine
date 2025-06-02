namespace Chess;

public class Move
{
    public int fromIndex;
    public int toIndex;
    public Piece? movedPiece;
    public Piece? capturedPiece;
    public bool isEnPassant;
    public bool shortCastle;
    public bool longCastle;

    public Type? promotionType; // Type of piece to promote to, if there

    public Move(int fromIndex, int toIndex, Type? promotionType = null)
    {
        this.fromIndex = fromIndex;
        this.toIndex = toIndex;

        movedPiece = null;
        capturedPiece = null;

        this.promotionType = promotionType;
    }

    public Move(int fromFile, int fromRank, int toFile, int toRank, Type? promotionType = null)
    {
        fromIndex = Utils.GetIndexFromFileRank(fromFile, fromRank);
        toIndex = Utils.GetIndexFromFileRank(toFile, toRank);

        movedPiece = null;
        capturedPiece = null;

        this.promotionType = promotionType;
    }

    // some functions so that we can check if move is in piece.legalMoves
    // (As move is a class, we cant just use if(piece.legalMoves.Contains(move) for some
    // reason, so we need to override Equals and GetHashCode)
    // (I didn't get this, its just chatgpt)
    public override bool Equals(object? obj)
    {
        if (obj is not Move other) return false;
        return fromIndex == other.fromIndex && toIndex == other.toIndex;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(fromIndex, toIndex);
    }
}