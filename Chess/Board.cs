namespace Chess;

public class Board
{
    public Piece[] pieces_on_board;
    public string fen;

    public Board(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        pieces_on_board = new Piece[64];
        this.fen = fen; // Initial FEN string
    }

    public void LoadFromFEN(string fen)
    {
        string piecePlacment = fen.Split(' ')[0];
        string[] ranks = piecePlacment.Split('/');

        for (int rank = 0; rank < ranks.Length; rank++)
        {
            string currentRank = ranks[rank];
            int file = 0;
            int boardRank = 7 - rank; // Reverse the rank
            foreach (char c in currentRank)
            {
                if (char.IsDigit(c))
                {
                    int emptySquares = c - '0';
                    for (int i = 0; i < emptySquares; i++)
                    {
                        pieces_on_board[(boardRank * 8) + file] = new Piece(Color.None, Type.None, file, boardRank);
                        file++;
                    }
                }
                else
                {
                    Piece piece = Piece.GetPieceFromChar(c, file, boardRank);
                    pieces_on_board[piece.index] = piece;
                    file++;
                }
            }
        }
    }

    public void PrintBoard()
    {
        string horizontalBorder = "   +" + string.Concat(Enumerable.Repeat("-----+", 8));
        Console.WriteLine(horizontalBorder);
        
        for (int rank = 7; rank >= 0; rank--)
        {
            Console.Write($" {rank + 1} |");
            for (int file = 0; file < 8; file++)
            {
                Piece piece = pieces_on_board[(rank * 8) + file];
                char symbol = Piece.GetCharFromPiece(piece);
                if (symbol == '-') symbol = ' ';
                Console.Write($"  {symbol}  |"); // <-- Wider cell
            }
            Console.WriteLine($" {rank + 1}");
            Console.WriteLine(horizontalBorder);
        }

        Console.WriteLine("     a     b     c     d     e     f     g     h");
    }

}