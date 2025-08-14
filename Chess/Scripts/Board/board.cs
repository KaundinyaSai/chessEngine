namespace ChessEngine;

public class Board
{
    // So the position will be stored in bitboards. Basically 64 bit unsigned integers, where if a bit is set to 0 the square at
    // that index will not be occupied by any piece. If the bit is set to 1, then the square at that index is occupied.
    // Of course, just a 1 wont be able to tell info like which piece it is at that index, so we use diffrent bitboards for each
    // type of piece (including colors).
    // So that means 12 bitboards (6 pieces x 2 colors) in total

    // ulong in c# means unsigned long (ints can store at max 32 bits, so uint isnt an option)
    public ulong WhitePawns;
    public ulong WhiteKnights;
    public ulong WhiteBishops;
    public ulong WhiteRooks;
    public ulong WhiteQueens;
    public ulong WhiteKing;

    public ulong BlackPawns;
    public ulong BlackKnights;
    public ulong BlackBishops;
    public ulong BlackRooks;
    public ulong BlackQueens;
    public ulong BlackKing;

    // And then three more bitboards for all the white and black pieces, and then for all pieces.
    public ulong WhitePieces => WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKing;
    public ulong BlackPieces => BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKing;
    public ulong AllPieces => WhitePieces | BlackPieces;

    public ulong EmptySquares => ~AllPieces;

    // The | symbol is the bitwise OR operator. It looks at each corresponding bit in the number, returns 0 if neither is 1 and
    // 1 if either or both are 1.

    // Oh and the LSB (least significant bit or the right most bit) is for square a1 and so on till MSB is h8.

    // for preventing wraparound
    public const ulong FileA = 0x0101010101010101UL;
    public const ulong FileH = 0x8080808080808080UL;
    public const ulong FileB = 0x0202020202020202UL;
    public const ulong FileG = 0x4040404040404040UL;

    public Piece?[] Pieces = new Piece?[64];
    
    public Board(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        // Clear bitboards
        WhitePawns = WhiteKnights = WhiteBishops = WhiteRooks = WhiteQueens = WhiteKing = 0;
        BlackPawns = BlackKnights = BlackBishops = BlackRooks = BlackQueens = BlackKing = 0;

        PopulateFromFen(fen);
    }

    public void PopulateFromFen(string fen)
    {
        string piecePlacement = fen.Split(" ")[0];
        string[] ranks = piecePlacement.Split("/");

        for (int fenRank = 0; fenRank < 8; fenRank++)
        {
            int file = 0;
            foreach (char piece in ranks[fenRank])
            {
                if (char.IsDigit(piece))
                {
                    file += int.Parse(piece.ToString());
                }
                else
                {
                    int rank = 7 - fenRank; // FEN starts from rank 8 (top), index 0, so we have to reverse.
                    int index = rank * 8 + file;
                    ulong mask = 1UL << index; // Make a ulong with one bit at index to one

                    switch (piece)
                    {
                        case 'P':
                            WhitePawns |= mask;
                            Pieces[index] = new Piece(PieceType.Pawn, PieceColor.White);
                            break;
                        case 'N':
                            WhiteKnights |= mask;
                            Pieces[index] = new Piece(PieceType.Knight, PieceColor.White);
                            break;
                        case 'B':
                            WhiteBishops |= mask;
                            Pieces[index] = new Piece(PieceType.Bishop, PieceColor.White);
                            break;
                        case 'R':
                            WhiteRooks |= mask;
                            Pieces[index] = new Piece(PieceType.Rook, PieceColor.White);
                            break;
                        case 'Q':
                            WhiteQueens |= mask;
                            Pieces[index] = new Piece(PieceType.Queen, PieceColor.White);
                            break;
                        case 'K':
                            WhiteKing |= mask;
                            Pieces[index] = new Piece(PieceType.King, PieceColor.White);
                            break;
                        case 'p':
                            BlackPawns |= mask;
                            Pieces[index] = new Piece(PieceType.Pawn, PieceColor.Black);
                            break;
                        case 'n':
                            BlackKnights |= mask;
                            Pieces[index] = new Piece(PieceType.Knight, PieceColor.Black);
                            break;
                        case 'b':
                            BlackBishops |= mask;
                            Pieces[index] = new Piece(PieceType.Bishop, PieceColor.Black);
                            break;
                        case 'r':
                            BlackRooks |= mask;
                            Pieces[index] = new Piece(PieceType.Rook, PieceColor.Black);
                            break;
                        case 'q':
                            BlackQueens |= mask;
                            Pieces[index] = new Piece(PieceType.Queen, PieceColor.Black);
                            break;
                        case 'k':
                            BlackKing |= mask;
                            Pieces[index] = new Piece(PieceType.King, PieceColor.Black);
                            break;
                        default:
                            throw new ArgumentException("Invalid FEN character");
                    }

                    file++;
                }
            }
        }

    }

    public void MakeMove(Move move, int enPassantSquare, out MoveInfo moveInfo, out Piece pieceToMove)
    {
        pieceToMove = BoardUtils.GetPieceAt(this, move.fromIndex);
        Piece? capturedPiece = null;

        moveInfo = new MoveInfo { move = move };

        if (BitBoardUtils.IsSquareOccupied(AllPieces, move.toIndex))
            capturedPiece = BoardUtils.GetPieceAt(this, move.toIndex);

        ref ulong bitboard = ref BoardUtils.GetBitboardFromPiece(this, pieceToMove);
        bitboard = BitBoardUtils.ClearBit(bitboard, move.fromIndex);

        Pieces[move.fromIndex] = null;
        Pieces[move.toIndex] = pieceToMove;

        bool isPromotion = pieceToMove.type == PieceType.Pawn &&
                        (move.toIndex / 8 == (pieceToMove.color == PieceColor.White ? 7 : 0));

        if (isPromotion)
        {
            if (move.promotion == 0b0000)
            {
                Console.WriteLine($"DEBUG: Pawn move to last rank without promotion! from={move.fromIndex} to={move.toIndex}");
                throw new Exception("Upon reaching the last rank, a pawn must promote");
            }

            Piece promotionPiece = new Piece(BoardUtils.getPromotionType(move.promotion), pieceToMove.color);
            ref ulong promotionBitboard = ref BoardUtils.GetBitboardFromPiece(this, promotionPiece);
            promotionBitboard = BitBoardUtils.SetBit(promotionBitboard, move.toIndex);
            Pieces[move.toIndex] = promotionPiece;

            moveInfo.promotionType = promotionPiece.type;
        }
        else
        {
            bitboard = BitBoardUtils.SetBit(bitboard, move.toIndex);
        }

        if (capturedPiece != null)
        {
            ref ulong capturedPieceBitboard = ref BoardUtils.GetBitboardFromPiece(this, capturedPiece.Value);
            capturedPieceBitboard = BitBoardUtils.ClearBit(capturedPieceBitboard, move.toIndex);
            moveInfo.capturedPiece = capturedPiece;
        }

        // Castling
        if (pieceToMove.type == PieceType.King)
        {
            switch ((move.fromIndex, move.toIndex, pieceToMove.color))
            {
                case (4, 6, PieceColor.White): // white short castle
                    Pieces[7] = null;
                    Pieces[5] = new Piece(PieceType.Rook, PieceColor.White);
                    WhiteRooks = BitBoardUtils.ClearBit(WhiteRooks, 7);
                    WhiteRooks = BitBoardUtils.SetBit(WhiteRooks, 5);
                    moveInfo.shortCastle = true;
                    break;

                case (4, 2, PieceColor.White): // white long castle
                    Pieces[0] = null;
                    Pieces[3] = new Piece(PieceType.Rook, PieceColor.White);
                    WhiteRooks = BitBoardUtils.ClearBit(WhiteRooks, 0);
                    WhiteRooks = BitBoardUtils.SetBit(WhiteRooks, 3);
                    moveInfo.longCastle = true;
                    break;

                case (60, 62, PieceColor.Black): // black short castle
                    Pieces[63] = null;
                    Pieces[61] = new Piece(PieceType.Rook, PieceColor.Black);
                    BlackRooks = BitBoardUtils.ClearBit(BlackRooks, 63);
                    BlackRooks = BitBoardUtils.SetBit(BlackRooks, 61);
                    moveInfo.shortCastle = true;
                    break;

                case (60, 58, PieceColor.Black): // black long castle
                    Pieces[56] = null;
                    Pieces[59] = new Piece(PieceType.Rook, PieceColor.Black);
                    BlackRooks = BitBoardUtils.ClearBit(BlackRooks, 56);
                    BlackRooks = BitBoardUtils.SetBit(BlackRooks, 59);
                    moveInfo.longCastle = true;
                    break;
            }
        }

        // En Passant
        if (pieceToMove.type == PieceType.Pawn && move.toIndex == enPassantSquare)
        {
            int epOffset = pieceToMove.color == PieceColor.White ? -8 : 8;
            int capturedPawnIndex = move.toIndex + epOffset;
            Piece pieceToEnPassant = BoardUtils.GetPieceAt(this, capturedPawnIndex);

            Pieces[capturedPawnIndex] = null;

            if (pieceToEnPassant.type != PieceType.Pawn || pieceToEnPassant.color == pieceToMove.color)
                throw new Exception("Invalid en passant");

            if (pieceToMove.color == PieceColor.White)
                BlackPawns = BitBoardUtils.ClearBit(BlackPawns, capturedPawnIndex);
            else
                WhitePawns = BitBoardUtils.ClearBit(WhitePawns, capturedPawnIndex);

            moveInfo.enPassant = true;
            moveInfo.capturedPiece = pieceToEnPassant;
        }
    }

    public void UnmakeMove(MoveInfo mi)
    {
        int from = mi.move.fromIndex;
        int to   = mi.move.toIndex;

        // Figure out the moving piece color & type at the time of the move.
        // If it was a promotion, the piece currently on 'to' is the promoted piece.
        Piece moverOnTo;
        if (mi.promotionType != null)
        {
            // The piece on 'to' *should* be the promoted piece.
            var promoted = BoardUtils.GetPieceAt(this, to);
            moverOnTo = new Piece(mi.promotionType.Value, promoted.color);

            // Remove promoted piece from its bitboard at 'to'
            ref ulong promoBB = ref BoardUtils.GetBitboardFromPiece(this, moverOnTo);
            promoBB = BitBoardUtils.ClearBit(promoBB, to);

            // We will place a pawn back on 'from' below.
        }
        else
        {
            // Non-promotion: moving piece sits on 'to'
            moverOnTo = BoardUtils.GetPieceAt(this, to);

            // Remove it from 'to' in its bitboard; will add back to 'from' below.
            ref ulong moverBB = ref BoardUtils.GetBitboardFromPiece(this, moverOnTo);
            moverBB = BitBoardUtils.ClearBit(moverBB, to);
        }

        // Handle captured piece restoration in bitboards and Pieces[].
        if (mi.capturedPiece != null)
        {
            ref ulong enemyBB = ref BoardUtils.GetBitboardFromPiece(this, mi.capturedPiece.Value);

            if (mi.enPassant)
            {
                // EP capture: captured pawn was NOT on 'to', it was behind it.
                int epOffset = moverOnTo.color == PieceColor.White ? -8 : 8;
                int epIndex  = to + epOffset;

                enemyBB = BitBoardUtils.SetBit(enemyBB, epIndex);
                Pieces[epIndex] = mi.capturedPiece;

                // IMPORTANT: 'to' square was empty before the move; clear it
                Pieces[to] = null;
            }
            else
            {
                // Normal capture: the captured piece sat on 'to'
                enemyBB = BitBoardUtils.SetBit(enemyBB, to);
                Pieces[to] = mi.capturedPiece;
            }
        }
        else
        {
            // No capture: 'to' square becomes empty
            Pieces[to] = null;
        }

        // Restore the moving side's piece back to 'from'
        if (mi.promotionType != null)
        {
            // The mover was a pawn that promoted; restore a pawn at 'from'
            var pawn = new Piece(PieceType.Pawn, moverOnTo.color);
            ref ulong pawnBB = ref BoardUtils.GetBitboardFromPiece(this, pawn);
            pawnBB = BitBoardUtils.SetBit(pawnBB, from);
            Pieces[from] = pawn;
        }
        else
        {
            // Non-promotion: restore the mover to 'from'
            ref ulong moverBB = ref BoardUtils.GetBitboardFromPiece(this, moverOnTo);
            moverBB = BitBoardUtils.SetBit(moverBB, from);
            Pieces[from] = moverOnTo;
        }

        // Undo rook movement for castling (king’s bitboard was already handled above)
        if (mi.shortCastle)
        {
            switch (moverOnTo.color)
            {
                case PieceColor.White:
                    // Rook went 7->5; put it back
                    WhiteRooks = BitBoardUtils.ClearBit(WhiteRooks, 5);
                    WhiteRooks = BitBoardUtils.SetBit(WhiteRooks, 7);
                    Pieces[5] = null;
                    Pieces[7] = new Piece(PieceType.Rook, PieceColor.White);
                    break;

                case PieceColor.Black:
                    // Rook went 63->61; put it back
                    BlackRooks = BitBoardUtils.ClearBit(BlackRooks, 61);
                    BlackRooks = BitBoardUtils.SetBit(BlackRooks, 63);
                    Pieces[61] = null;
                    Pieces[63] = new Piece(PieceType.Rook, PieceColor.Black);
                    break;
            }
        }

        if (mi.longCastle)
        {
            switch (moverOnTo.color)
            {
                case PieceColor.White:
                    // Rook went 0->3; put it back
                    WhiteRooks = BitBoardUtils.ClearBit(WhiteRooks, 3);
                    WhiteRooks = BitBoardUtils.SetBit(WhiteRooks, 0);
                    Pieces[3] = null;
                    Pieces[0] = new Piece(PieceType.Rook, PieceColor.White);
                    break;

                case PieceColor.Black:
                    // Rook went 56->59; put it back
                    BlackRooks = BitBoardUtils.ClearBit(BlackRooks, 59);
                    BlackRooks = BitBoardUtils.SetBit(BlackRooks, 56);
                    Pieces[59] = null;
                    Pieces[56] = new Piece(PieceType.Rook, PieceColor.Black);
                    break;
            }
        }
    }

}



