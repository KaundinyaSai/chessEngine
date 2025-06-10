

GameState game = new GameState("6k1/2P5/8/8/8/8/8/6K1 w - - 0 1");

BoardUtils.PrintBoard(game.board);
Console.WriteLine(game.enPassantSquare);

game.MakeMove(new Move(50, 58));
BoardUtils.PrintBoard(game.board);

Console.WriteLine(game.enPassantSquare);










