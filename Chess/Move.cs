
public struct Move
{
    // Just a 16 bit integer, storing stuff.
    public ushort rawMove;

    // 0000 000000 000000
    // ^^^^ ^^^^^^ ^^^^^^
    // |    |      +-------- bits 0-5 -> from square
    // |    +--------------- bits 6-11 -> to square
    // +-------------------- bits 12-15 -> promotion (0000 - none, 0001 - knight, 0010 - bishop, 0100 - rook, 1000 - queen)

    public int fromIndex => rawMove & 0b111111;
    public int toIndex => (rawMove >> 6) & 0b111111;
    public int promotion => (rawMove >> 12) & 0b1111;

    public Move(int from, int to, PieceType? promotionType = null)
    {
        rawMove = 0;

        rawMove |= (ushort)from;
        rawMove |= (ushort)(to << 6);

        rawMove |= (ushort)(BitBoardUtils.GetBinaryFromPromotionType(promotionType) << 12);
    }

    // Using 16 bit integers like this can speed up search and save memory (probably). And bit operations are VERY cheap.

    public override bool Equals(object? obj) =>
        obj is Move m && m.rawMove == this.rawMove;

    public override int GetHashCode() => rawMove.GetHashCode();

    public static bool operator ==(Move a, Move b) => a.rawMove == b.rawMove;
    public static bool operator !=(Move a, Move b) => a.rawMove != b.rawMove;
}

public struct MoveInfo
{
    public Move move;
    public Piece? capturedPiece;
    public bool shortCastle;
    public bool longCastle;
    public bool enPassant;
    public PieceType? promotionType;
}