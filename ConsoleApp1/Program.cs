

GameState game = new GameState();

BoardUtils.PrintBoard(game.board);
Console.WriteLine(game.enPassantSquare);

game.MakeMove(new Move(12, 28));
BoardUtils.PrintBoard(game.board);

Console.WriteLine(game.enPassantSquare);










