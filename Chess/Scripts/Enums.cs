public enum PieceColor
{
    White,
    Black
}

public enum PieceType
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

public enum CastlingRights : byte
{
    None              = 0,
    WhiteKingside     = 1 << 0, // K
    WhiteQueenside    = 1 << 1, // Q
    BlackKingside     = 1 << 2, // k
    BlackQueenside    = 1 << 3  // q
}
