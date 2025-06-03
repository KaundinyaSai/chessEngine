namespace Chess;

public class Board
{
    public Piece[] pieces_on_board;
    public string fen;

    public int whiteKingIndex => Array.FindIndex(pieces_on_board, p => p.pieceType == Type.King && p.pieceColor == Color.White);
    public int blackKingIndex => Array.FindIndex(pieces_on_board, p => p.pieceType == Type.King && p.pieceColor == Color.Black);

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
        if (!pieceToMove.legalMoves.Contains(move))
        {
            throw new InvalidOperationException("Move is not legal for the piece.");
        }

        if (pieceToMove.pieceType == Type.None)
        {
            throw new InvalidOperationException("No piece at the fromIndex to move.");
        }

        // Capture info BEFORE updating the board
        move.movedPiece = pieceToMove;
        move.capturedPiece = pieces_on_board[toIndex];

        pieces_on_board[toIndex] = pieceToMove;
        pieces_on_board[fromIndex] = new Piece(Color.None, Type.None, pieceToMove.index % 8, pieceToMove.index / 8); // Empty square

        // Extra logic for castling
        if (pieceToMove.pieceType == Type.King && Math.Abs(fromIndex - toIndex) == 2)
        {
            // kingside
            if (toIndex == fromIndex + 2)
            {
                int rookFromIndex = fromIndex + 3; // Rook moves from the right
                int rookToIndex = toIndex - 1; // Rook moves to the left

                Piece rook = pieces_on_board[rookFromIndex];

                pieces_on_board[rookToIndex] = rook;
                pieces_on_board[rookFromIndex] = new Piece(Color.None, Type.None, rook.index % 8, rook.index / 8); // Empty square
                rook.ChangePosition(rookToIndex);
                rook.movedNum++; // Increment the move count for the rook
                move.shortCastle = true; // Indicate that this is a kingside castle
            }
            // queenside
            else if (toIndex == fromIndex - 2)
            {
                int rookFromIndex = fromIndex - 4; // Rook moves from the left
                int rookToIndex = toIndex + 1; // Rook moves to the right

                Piece rook = pieces_on_board[rookFromIndex];

                pieces_on_board[rookToIndex] = rook;
                pieces_on_board[rookFromIndex] = new Piece(Color.None, Type.None, rook.index % 8, rook.index / 8); // Empty square
                rook.ChangePosition(rookToIndex);
                rook.movedNum++; // Increment the move count for the rook
                move.longCastle = true; // Indicate that this is a queenside castle
            }
        }


        // Extra logic for en passant
        // Check if pieceToMove is of type pawn, the move is a diagonal move, and the captured piece is empty

        if (pieceToMove.pieceType == Type.Pawn && (Math.Abs(fromIndex - toIndex) == 9 || Math.Abs(fromIndex - toIndex) == 7))
        {
            // Check if the move is an en passant capture
            if (move.capturedPiece.pieceType == Type.None)
            {
                int enPassantIndex = toIndex + (pieceToMove.pieceColor == Color.White ? -8 : 8);
                move.isEnPassant = true;
                move.capturedPiece = pieces_on_board[enPassantIndex]; // The pawn that was captured en passant
                pieces_on_board[enPassantIndex] = new Piece(Color.None, Type.None, enPassantIndex % 8, enPassantIndex / 8); // Remove the captured pawn
            }
        }

        // Extra logic for promotion
        if (move.promotionType != null && pieceToMove.pieceType == Type.Pawn)
        {
            if (move.promotionType is Type.King or Type.Pawn)
            {
                throw new InvalidOperationException("Cannot promote to King or Pawn.");
            }

            int promotionRank = pieceToMove.pieceColor == Color.White ? 7 : 0; // Promotion rank for white is 7, for black is 0

            if (move.toIndex / 8 == promotionRank)
            {
                // Replace the pawn with the promoted piece
                pieces_on_board[toIndex] = new Piece(pieceToMove.pieceColor, move.promotionType.Value, toIndex % 8, toIndex / 8);
                move.movedPiece = pieces_on_board[toIndex]; // Update the moved piece in the move object
            }
        }

        // Update the piece's index
        pieceToMove.ChangePosition(toIndex);
        pieceToMove.movedNum++; // Increment the move count for the piece
    }

    public void UnmakeMove(Move move)
    {
        int fromIndex = move.fromIndex;
        int toIndex = move.toIndex;

        if (fromIndex < 0 || fromIndex >= 64 || toIndex < 0 || toIndex >= 64)
        {
            throw new ArgumentException("Invalid move indices.");
        }

        // Restore the piece at the fromIndex

        // ?? checks if piece is null, if it is, it assigns a new Piece with default values (Empty)
        pieces_on_board[fromIndex] = move.movedPiece ?? new Piece(Color.None, Type.None, fromIndex % 8, fromIndex / 8);

        pieces_on_board[toIndex] = move.capturedPiece ?? new Piece(Color.None, Type.None, toIndex % 8, toIndex / 8);


        // Restore the piece's position
        move.movedPiece?.ChangePosition(fromIndex);

        move.capturedPiece?.ChangePosition(toIndex);


        // Decrement the move count for the moved piece
        if (move.movedPiece != null)
        {
            move.movedPiece.movedNum--;
        }

        // If it was a castling move, restore the rook's position
        if (move.shortCastle || move.longCastle)
        {
            int rookFromIndex = move.shortCastle ? fromIndex + 3 : fromIndex - 4;
            int rookToIndex = move.shortCastle ? toIndex - 1 : toIndex + 1;

            Piece rook = pieces_on_board[rookToIndex];
            pieces_on_board[rookFromIndex] = rook;
            pieces_on_board[rookToIndex] = new Piece(Color.None, Type.None, rook.index % 8, rook.index / 8); // Empty square
            rook.ChangePosition(rookFromIndex);
            rook.movedNum--; // Decrement the move count for the rook
        }

        // If it was an en passant capture, restore the captured pawn
        if (move.isEnPassant)
        {
            int enPassantIndex = toIndex + (move.movedPiece?.pieceColor == Color.White ? -8 : 8);

            // Restore captured pawn
            if (move.capturedPiece != null)
            {
                pieces_on_board[enPassantIndex] = new Piece(move.capturedPiece.pieceColor, Type.Pawn, enPassantIndex % 8, enPassantIndex / 8);
            }

            // Clear the square the capturing pawn landed on
            pieces_on_board[toIndex] = new Piece(Color.None, Type.None, toIndex % 8, toIndex / 8);
        }
    }
    
    
}