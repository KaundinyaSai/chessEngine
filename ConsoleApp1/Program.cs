
using System.Diagnostics;

GameState game = new GameState("rnbqkb1r/pppppppp/8/4n3/8/4Q3/PPPPPPPP/RNB1KBNR w KQkq - 0 1");

Stopwatch stopwatch = Stopwatch.StartNew();

<<<<<<< HEAD
int movesToLog = 10;
int depth = 3;     
=======
int movesToLog = 10; // or however many you want
int depth = 3;       // search depth for each move
>>>>>>> 4e3e55e0776e013523b9570401b9a2f7355e2b0c

for (int i = 0; i < movesToLog; i++)
{
    Move move = Search.FindBestMove(game, depth);

    Console.WriteLine($"Move {i + 1}: {BoardUtils.ConvertToAlg(move)}");

    game.SimplerMakeMove(move, out MoveInfo _);
}

stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");


