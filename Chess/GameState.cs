namespace Chess;

public class GameState
{
    public Board board;
    public Color currentTurn;
    public bool whiteCanCastleShort;
    public bool whiteCanCastleLong;
    public bool blackCanCastleShort;
    public bool blackCanCastleLong;
    public int halfMoveClock; // Half-move clock for the fifty-move rule
    public int fullMoveNumber; // Full move number in the game
    public List<Move> moveHistory; // History of moves made in the game

    public GameState(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        board = new Board(fen);
        board.LoadFromFEN(fen);

        currentTurn = Color.White;
        whiteCanCastleShort = true;
        whiteCanCastleLong = true;
        blackCanCastleShort = true;
        blackCanCastleLong = true;
        halfMoveClock = 0;
        fullMoveNumber = 1;
        moveHistory = new List<Move>();

        board.LoadFromFEN(fen);
    }

    public void MakeMove(Move move)
    {
        board.MakeMove(move);

        moveHistory.Add(move);
        currentTurn = currentTurn == Color.White ? Color.Black : Color.White;

        MoveGen.GenerateMovesForAllPieces(board.pieces_on_board, moveHistory.LastOrDefault());

        if (currentTurn == Color.White)
        {
            fullMoveNumber++;
        }
    }
}