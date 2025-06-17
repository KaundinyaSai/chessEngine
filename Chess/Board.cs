
using System.Drawing;

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

    // Attack tables (sqaures which are attacked by pieces)
    public ulong WhitePawnAttacks;
    public ulong WhiteKnightAttacks;
    public ulong WhiteBishopAttacks;
    public ulong WhiteRookAttacks;
    public ulong WhiteQueenAttacks;
    public ulong WhiteKingAttacks;

    public ulong BlackPawnAttacks;
    public ulong BlackKnightAttacks;
    public ulong BlackBishopAttacks;
    public ulong BlackRookAttacks;
    public ulong BlackQueenAttacks;
    public ulong BlackKingAttacks;
    public ulong WhiteAttacks =>
        WhitePawnAttacks |
        WhiteKnightAttacks |
        WhiteBishopAttacks |
        WhiteRookAttacks |
        WhiteQueenAttacks |
        WhiteKingAttacks;

    public ulong BlackAttacks =>
        BlackPawnAttacks |
        BlackKnightAttacks |
        BlackBishopAttacks |
        BlackRookAttacks |
        BlackQueenAttacks |
        BlackKingAttacks;


    // for preventing wraparound
    public const ulong FileA = 0x0101010101010101UL;
    public const ulong FileH = 0x8080808080808080UL;
    public const ulong FileB = 0x0202020202020202UL;
    public const ulong FileG = 0x4040404040404040UL;


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
                        // OR it into the corresponding bitboard
                        case 'P': WhitePawns |= mask; break;
                        case 'N': WhiteKnights |= mask; break;
                        case 'B': WhiteBishops |= mask; break;
                        case 'R': WhiteRooks |= mask; break;
                        case 'Q': WhiteQueens |= mask; break;
                        case 'K': WhiteKing |= mask; break;
                        case 'p': BlackPawns |= mask; break;
                        case 'n': BlackKnights |= mask; break;
                        case 'b': BlackBishops |= mask; break;
                        case 'r': BlackRooks |= mask; break;
                        case 'q': BlackQueens |= mask; break;
                        case 'k': BlackKing |= mask; break;
                        default: throw new ArgumentException("Invalid FEN character");
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

        moveInfo = new MoveInfo();
        moveInfo.move = move;

        if (BitBoardUtils.IsSquareOccupied(AllPieces, move.toIndex))
        {
            capturedPiece = BoardUtils.GetPieceAt(this, move.toIndex);
        }

        ref ulong bitboard = ref BoardUtils.GetBitboardFromPiece(this, pieceToMove);
        bitboard = BitBoardUtils.ClearBit(bitboard, move.fromIndex);

        // Promotion check
        bool isPromotion = pieceToMove.type == PieceType.Pawn &&
                        (move.toIndex / 8 == (pieceToMove.color == PieceColor.White ? 7 : 0));

        if (isPromotion)
        {
            if (move.promotion == 0b0000)
            {
                Console.WriteLine($"DEBUG: Pawn move to last rank without promotion! from={move.fromIndex} to={move.toIndex}");
                Console.WriteLine($"Piece type: {pieceToMove.type}");
                throw new Exception("Upon reaching the last rank, a pawn must promote");
            }

            // Add promoted piece to correct bitboard
            Piece promotionPiece = new Piece(BoardUtils.getPromotionType(move.promotion), pieceToMove.color);
            ref ulong promotionBitboard = ref BoardUtils.GetBitboardFromPiece(this, promotionPiece);
            promotionBitboard = BitBoardUtils.SetBit(promotionBitboard, move.toIndex);

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
        else
        {
            moveInfo.capturedPiece = null;
        }

        // Castling
        if (pieceToMove.type == PieceType.King)
        {
            if (move.fromIndex == 4 && move.toIndex == 6 && pieceToMove.color == PieceColor.White)
            {
                WhiteRooks = BitBoardUtils.ClearBit(WhiteRooks, 7);
                WhiteRooks = BitBoardUtils.SetBit(WhiteRooks, 5);
                moveInfo.shortCastle = true;
            }
            else if (move.fromIndex == 4 && move.toIndex == 2 && pieceToMove.color == PieceColor.White)
            {
                WhiteRooks = BitBoardUtils.ClearBit(WhiteRooks, 0);
                WhiteRooks = BitBoardUtils.SetBit(WhiteRooks, 3);
                moveInfo.longCastle = true;
            }
            else if (move.fromIndex == 60 && move.toIndex == 62 && pieceToMove.color == PieceColor.Black)
            {
                BlackRooks = BitBoardUtils.ClearBit(BlackRooks, 63);
                BlackRooks = BitBoardUtils.SetBit(BlackRooks, 61);
                moveInfo.shortCastle = true;
            }
            else if (move.fromIndex == 60 && move.toIndex == 58 && pieceToMove.color == PieceColor.Black)
            {
                BlackRooks = BitBoardUtils.ClearBit(BlackRooks, 56);
                BlackRooks = BitBoardUtils.SetBit(BlackRooks, 59);
                moveInfo.longCastle = true;
            }
        }

        // En passant
        if (pieceToMove.type == PieceType.Pawn && MathF.Abs(move.toIndex - move.fromIndex) is 9 or 7)
        {
            if (move.toIndex == enPassantSquare)
            {
                int epOffset = pieceToMove.color == PieceColor.White ? -8 : 8;
                int capturedPawnIndex = move.toIndex + epOffset;
                Piece pieceToEnPassant = BoardUtils.GetPieceAt(this, capturedPawnIndex);

                if (pieceToEnPassant.type != PieceType.Pawn)
                    throw new Exception("Can't en passant anything other than a pawn");

                if (pieceToEnPassant.color == pieceToMove.color)
                    throw new Exception("Not enemy piece, can't en passant");

                if (pieceToMove.color == PieceColor.White)
                    BlackPawns = BitBoardUtils.ClearBit(BlackPawns, capturedPawnIndex);
                else
                    WhitePawns = BitBoardUtils.ClearBit(WhitePawns, capturedPawnIndex);

                moveInfo.enPassant = true;
                moveInfo.capturedPiece = pieceToEnPassant;
            }
        }
    }


    public void UnmakeMove(MoveInfo moveInfoToUndo)
    {
        int from = moveInfoToUndo.move.fromIndex;
        int to = moveInfoToUndo.move.toIndex;

        Piece thisPiece;

        // Logic for promotion
        if (moveInfoToUndo.promotionType != null)
        {
            // Remove promoted piece from 'to'
            thisPiece = new Piece(moveInfoToUndo.promotionType.Value, BoardUtils.GetPieceAt(this, to).color);
            ref ulong promoBB = ref BoardUtils.GetBitboardFromPiece(this, thisPiece);
            promoBB = BitBoardUtils.ClearBit(promoBB, to);

            // Restore pawn to 'from'
            ref ulong pawnBB = ref thisPiece.color == PieceColor.White ? ref WhitePawns : ref BlackPawns;
            pawnBB = BitBoardUtils.SetBit(pawnBB, from);
        }
        else
        {
            // Standard piece move-back
            if (!BitBoardUtils.IsSquareOccupied(AllPieces, to))
                throw new Exception("No piece found to reverse");

            thisPiece = BoardUtils.GetPieceAt(this, to);
            ref ulong thisPieceBB = ref BoardUtils.GetBitboardFromPiece(this, thisPiece);

            thisPieceBB = BitBoardUtils.ClearBit(thisPieceBB, to);
            thisPieceBB = BitBoardUtils.SetBit(thisPieceBB, from);
        }

        // Restore captured piece
        if (moveInfoToUndo.capturedPiece != null)
        {
            ref ulong enemyBB = ref BoardUtils.GetBitboardFromPiece(this, moveInfoToUndo.capturedPiece.Value);
            if (moveInfoToUndo.enPassant)
            {
                int epOffset = thisPiece.color == PieceColor.White ? -8 : 8;
                enemyBB = BitBoardUtils.SetBit(enemyBB, to + epOffset);
            }
            else
            {
                enemyBB = BitBoardUtils.SetBit(enemyBB, to);
            }
        }

        // Logic for castling
        if (moveInfoToUndo.shortCastle)
        {
            int rookIndex = thisPiece.color == PieceColor.White ? 5 : 61;
            int originalIndex = thisPiece.color == PieceColor.White ? 7 : 63;
            ref ulong rookBB = ref thisPiece.color == PieceColor.White ? ref WhiteRooks : ref BlackRooks;
            rookBB = BitBoardUtils.ClearBit(rookBB, rookIndex);
            rookBB = BitBoardUtils.SetBit(rookBB, originalIndex);
        }

        if (moveInfoToUndo.longCastle)
        {
            int rookIndex = thisPiece.color == PieceColor.White ? 3 : 59;
            int originalIndex = thisPiece.color == PieceColor.White ? 0 : 56;
            ref ulong rookBB = ref thisPiece.color == PieceColor.White ? ref WhiteRooks : ref BlackRooks;
            rookBB = BitBoardUtils.ClearBit(rookBB, rookIndex);
            rookBB = BitBoardUtils.SetBit(rookBB, originalIndex);
        }
    }

}

