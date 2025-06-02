
// File for debugging

using Chess;

GameState game = new GameState("8/5P2/8/8/8/8/3p4/8");

game.board.PrintBoard();

game.MakeMove(new Move(53, 61, Type.Queen));
Console.WriteLine();
game.board.PrintBoard();

game.MakeMove(new Move(11, 3, Type.Knight));
Console.WriteLine();
game.board.PrintBoard();