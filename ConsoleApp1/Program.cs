
using Chess;

Board board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
board.LoadFromFEN(board.fen);
board.PrintBoard();

