

GameState game = new GameState("5k2/4P3/5K2/8/8/8/8/8 w - - 0 1");

BoardUtils.PrintBoard(game.board);
Console.WriteLine(game.enPassantSquare);

game.MakeMove(new Move(52, 60, PieceType.Queen));
BoardUtils.PrintBoard(game.board);

game.UnmakeLastMove();
BoardUtils.PrintBoard(game.board);












