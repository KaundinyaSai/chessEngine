namespace Chess;

public static class MoveGen
{
    // Adding offsets for sliding pieces
    static readonly int[] rookOffsets = { -1, 1, -8, 8 };
    static readonly int[] bishopOffsets = { -9, -7, 7, 9 };
    static readonly int[] queenOffsets = { -1, 1, -8, 8, -9, -7, 7, 9 };

    // Lookup tables for kings and knights, makes things faster
    public static List<int>[] KnightLookUpTable { get; } = Utils.LookUpTableInit(
        [
            (-2, -1), (-2, 1),
            (-1, -2), (-1, 2),
            (1, -2), (1, 2),
            (2, -1), (2, 1)
        ]);
    public static List<int>[] KingLookUpTable { get; } = Utils.LookUpTableInit(
        [
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1),           (0, 1),
            (1, -1), (1, 0), (1, 1)
        ]);


    public static void GenerateMovesForAllPieces(Piece[] pieces, Move? lastMove)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].pieceType != Type.None)
            {
                GenerateMovesForPiece(i, pieces, lastMove);
            }
        }
    }

    public static void GenerateMovesForPiece(int startIndex, Piece[] pieces, Move? lastMove)
    {
        Piece thisPiece = pieces[startIndex];
        thisPiece.legalMoves.Clear();

        switch (thisPiece.pieceType)
        {
            case Type.Pawn:
                thisPiece.legalMoves.AddRange(GeneratePawnMoves(startIndex, pieces, lastMove));
                break;
            case Type.Rook:
                thisPiece.legalMoves.AddRange(GenerateSlideMoves(startIndex, rookOffsets, pieces));
                break;
            case Type.Bishop:
                thisPiece.legalMoves.AddRange(GenerateSlideMoves(startIndex, bishopOffsets, pieces));
                break;
            case Type.Queen:
                thisPiece.legalMoves.AddRange(GenerateSlideMoves(startIndex, queenOffsets, pieces));
                break;
            case Type.Knight:
                thisPiece.legalMoves.AddRange(GenerateLeaperMoves(startIndex, pieces, KnightLookUpTable));
                break;
            case Type.King:
                thisPiece.legalMoves.AddRange(GenerateKingMoves(startIndex, pieces));
                break;
            default:
                throw new ArgumentException("Cannot generate moves for an empty square.");
        }
    }

    public static List<Move> GenerateSlideMoves(int startIndex, int[] offsets, Piece[] pieces)
    {
        List<Move> moves = new();
        int startRow = startIndex / 8;
        int startCol = startIndex % 8;

        Piece thisPiece = pieces[startIndex];

        foreach (int offset in offsets)
        {
            int currentIndex = startIndex;

            for (int i = 1; i < 8; i++)
            {
                int targetIndex = currentIndex + offset;

                if (targetIndex < 0 || targetIndex >= 64)
                    break;

                int targetRow = targetIndex / 8;
                int targetCol = targetIndex % 8;

                // Prevent horizontal wraparound
                if (offset == 1 || offset == -1)
                {
                    if (targetRow != startRow)
                        break;
                }

                // Prevent diagonal wraparound
                if (offset == 9 || offset == -9 || offset == 7 || offset == -7)
                {
                    // Checks if both the row and column increase by the same amount
                    // if not, there is diagonal wraparound
                    if (Math.Abs(targetRow - startRow) != i || Math.Abs(targetCol - startCol) != i)
                        break;
                }

                Piece targetPiece = pieces[targetIndex];

                if (targetPiece.pieceType != Type.None)
                {
                    if (targetPiece.IsEnemy(thisPiece))
                        moves.Add(new Move(startIndex, targetIndex));
                    break;
                }

                moves.Add(new Move(startIndex, targetIndex));
                currentIndex = targetIndex;
            }
        }

        return moves;
    }


    public static List<Move> GenerateLeaperMoves(int startIndex, Piece[] pieces, List<int>[] LookUpTable)
    // leaper is the term i found online for kings and knights
    {
        List<Move> moves = new();
        Piece thisPiece = pieces[startIndex];

        foreach (int targetIndex in LookUpTable[startIndex])
        {
            // Check if the target index is within bounds
            if (targetIndex < 0 || targetIndex >= 64)
                continue;

            Piece targetPiece = pieces[targetIndex];

            // If the target square is empty or occupied by an enemy piece, add the move
            if (targetPiece.pieceType == Type.None || targetPiece.IsEnemy(thisPiece))
            {
                moves.Add(new Move(startIndex, targetIndex));
            }
        }

        return moves;
    }

    public static List<Move> GenerateKingMoves(int startIndex, Piece[] pieces)
    {
        List<Move> moves = GenerateLeaperMoves(startIndex, pieces, KingLookUpTable);
        Piece king = pieces[startIndex];
        if (king.movedNum > 0)
            return moves;

        bool isWhite = king.pieceColor == Color.White;

        int kingSideRookIndex = isWhite ? 7 : 63;
        int queenSideRookIndex = isWhite ? 0 : 56;
        int kingIndex = isWhite ? 4 : 60;

        // King-side castling
        if (pieces[kingSideRookIndex].pieceType == Type.Rook && pieces[kingSideRookIndex].movedNum == 0)
        {
            if (pieces[kingIndex + 1].pieceType == Type.None && pieces[kingIndex + 2].pieceType == Type.None)
            {
                moves.Add(new Move(startIndex, kingIndex + 2));
            }
        }

        // Queen-side castling
        if (pieces[queenSideRookIndex].pieceType == Type.Rook && pieces[queenSideRookIndex].movedNum == 0)
        {
            if (pieces[kingIndex - 1].pieceType == Type.None &&
                pieces[kingIndex - 2].pieceType == Type.None &&
                pieces[kingIndex - 3].pieceType == Type.None)
            {
                moves.Add(new Move(startIndex, kingIndex - 2));
            }
        }

        return moves;
    }


    public static List<Move> GeneratePawnMoves(int startIndex, Piece[] pieces, Move? lastMove)
    {
        List<Move> moves = new();
        Piece thisPiece = pieces[startIndex];
        int singleSquareOffset = thisPiece.pieceColor == Color.White ? 8 : -8; // White moves up, Black moves down
        int doubleSquareOffset = thisPiece.pieceColor == Color.White ? 16 : -16; // Double move for pawns

        // Check for single square move
        int singleSquareIndex = startIndex + singleSquareOffset;
        if (singleSquareIndex >= 0 && singleSquareIndex < 64)
        {
            Piece targetPiece = pieces[singleSquareIndex];
            if (targetPiece.pieceType == Type.None)
            {
                moves.Add(new Move(startIndex, singleSquareIndex));

                // Check for double square move from the starting position
                int startRank = thisPiece.pieceColor == Color.White ? 1 : 6; // Starting ranks for pawns
                if (thisPiece.rank == startRank)
                {
                    // Check if the double square move is valid
                    if (startIndex + doubleSquareOffset >= 0 && startIndex + doubleSquareOffset < 64)
                    {
                        Piece doubleSquarePiece = pieces[startIndex + doubleSquareOffset];
                        if (doubleSquarePiece.pieceType == Type.None)
                        {
                            moves.Add(new Move(startIndex, startIndex + doubleSquareOffset));
                        }
                    }
                }
            }
        }

        // Check for captures               
        int[] captureOffsets = { singleSquareOffset - 1, singleSquareOffset + 1 }; // Left and right captures
        foreach (int offset in captureOffsets)
        {
            int captureIndex = startIndex + offset;
            if (captureIndex >= 0 && captureIndex < 64)
            {
                Piece targetPiece = pieces[captureIndex];
                if (targetPiece.pieceType != Type.None && targetPiece.IsEnemy(thisPiece))
                {
                    moves.Add(new Move(startIndex, captureIndex));
                }
            }
        }

        // Check for en passant captures
        int enPassantRank = thisPiece.pieceColor == Color.White ? 4 : 3;

        if (thisPiece.rank == enPassantRank && lastMove != null)
        {
            int lastFrom = lastMove.fromIndex;
            int lastTo = lastMove.toIndex;

            Piece lastMovedPiece = pieces[lastTo];

            if (lastMovedPiece.pieceType == Type.Pawn && Math.Abs(lastTo - lastFrom) == 16)
            {
                int diff = lastTo - startIndex;

                if (diff == 1 || diff == -1) // The pawn is directly to the left or right
                {
                    int enPassantCaptureIndex = lastTo + (thisPiece.pieceColor == Color.White ? 8 : -8);

                    if (pieces[enPassantCaptureIndex].pieceType == Type.None)
                    {
                        moves.Add(new Move(startIndex, enPassantCaptureIndex));
                    }
                }
            }
        }


        return moves;
    }

}