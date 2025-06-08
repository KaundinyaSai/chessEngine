

Board board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");

Console.WriteLine("Before move:");
BoardUtils.PrintBoard(board);

Move move = new Move(12, 63);
board.MakeMove(move);

BoardUtils.PrintBoard(board);

board.MakeMove(new Move(63, 12));
BoardUtils.PrintBoard(board);







