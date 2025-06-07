

Board board = new Board("2Q5/5k2/3K4/5r1K/2b5/3BN2n/4q3/8");

var pawnMoves = MoveGen.QueenMoves(board, PieceColor.White);

if (pawnMoves.TryGetValue(58, out ulong movesForPawn))
{
    Console.WriteLine("Moves for pawn at square 12:");
    BoardUtils.PrintBitboard(movesForPawn);
}
else
{
    Console.WriteLine("No legal moves for pawn at square 12.");
}




