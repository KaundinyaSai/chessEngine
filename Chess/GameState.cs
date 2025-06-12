
public class GameState
{
    public Board board;

    public Stack<MoveInfo> moves;
    public int halfMoveClock; // for 50 move rule

    public bool whiteCanShortCastle;
    public bool whiteCanLongCastle;
    public bool blackCanShortCastle;
    public bool blackCanLongCastle;

    public int enPassantSquare; // null if none, 0-63 is not none.

    public int plyNum; // a ply is half of a move

    public bool isWhiteTurn => plyNum % 2 == 0;

    public GameState(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        board = new Board(fen);
        board.PopulateFromFen(fen);

        moves = new Stack<MoveInfo>();

        BoardUtils.ParseFen(this, fen);
    }

    public void MakeMove(Move move)
    {
        board.MakeMove(move, enPassantSquare, out MoveInfo moveInfo, out Piece pieceToMove);
        moves.Push(moveInfo);

        // Check if it was a double pawn push and update the en passant square accorginly
        if (pieceToMove.type == PieceType.Pawn && MathF.Abs(move.toIndex - move.fromIndex) == 16)
        {
            int enPassantOffset = pieceToMove.color == PieceColor.White ? -16 : 16;
            enPassantSquare = move.toIndex + enPassantOffset;
        }
        else
        {
            enPassantSquare = -1;
        }

        plyNum++;
    }

    public void UnmakeLastMove()
    {
        if (moves.Count <= 0)
        {
            throw new Exception("No moves to unmake");
        }

        MoveInfo moveToUnmake = moves.Pop();
        board.UnmakeMove(moveToUnmake);
    }
}