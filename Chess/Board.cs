namespace Chess;

public class Board
{
    public Piece[] pieces_on_board;
    public string fen;

    public Board(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") // Default starting position
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

        foreach (Piece piece in pieces_on_board)
        {
            if (piece.pieceType != Type.None)
            {
                MoveGen.GenerateMovesForPiece(piece.index, pieces_on_board);
            }
        }
    }

    public void PrintBoard()
    {
        string horizontalBorder = "   +" + string.Concat(Enumerable.Repeat("---+", 8));
        Console.WriteLine(horizontalBorder);
        
        for (int rank = 7; rank >= 0; rank--)
        {
            Console.Write($" {rank + 1} |");
            for (int file = 0; file < 8; file++)
            {
                Piece piece = pieces_on_board[(rank * 8) + file];
                char symbol = Piece.GetCharFromPiece(piece);
                if (symbol == '-') symbol = ' ';
                Console.Write($" {symbol} |");
            }
            Console.WriteLine($" {rank + 1}");
            Console.WriteLine(horizontalBorder);
        }

        Console.WriteLine("     a   b   c   d   e   f   g   h");
    }

    public void MakeMove(Move move)
    {
        int fromIndex = move.fromIndex;
        int toIndex = move.toIndex;

        if (fromIndex < 0 || fromIndex >= 64 || toIndex < 0 || toIndex >= 64)
        {
            throw new ArgumentException("Invalid move indices.");
        }

        // Move the piece
        Piece pieceToMove = pieces_on_board[fromIndex];
        if (!pieceToMove.legalMoves.Contains(move)) {
            throw new InvalidOperationException("Move is not legal for the piece.");
        }

        if (pieceToMove.pieceType == Type.None)
        {
            throw new InvalidOperationException("No piece at the fromIndex to move.");
        }

        pieces_on_board[toIndex] = pieceToMove;
        pieces_on_board[fromIndex] = new Piece(Color.None, Type.None, pieceToMove.index % 8, pieceToMove.index / 8); // Empty square

        // Update the piece's index
        pieceToMove.ChangePosition(toIndex);
        pieceToMove.movedNum++; // Increment the move count for the piece
    }
}