

GameState game = new GameState("rnbqkbnr/pp2pppp/2p5/1B1p4/4P3/8/PPPP1PPP/RNBQK1NR w KQkq - 0 1");

BoardUtils.PrintBoard(game.board);

game.MakeMove(new Move(42, 34));

Console.WriteLine(game.IsBlackKingInCheck);














