
using System.Diagnostics;

GameState game = new GameState("rnbqkb1r/pppppppp/8/4n3/8/4Q3/PPPPPPPP/RNB1KBNR w KQkq - 0 1");

Stopwatch stopwatch = Stopwatch.StartNew();

int movesToLog = 10;
int depth = 3;     

for (int i = 0; i < movesToLog; i++)
{
    Move move = Search.FindBestMove(game, depth);

    Console.WriteLine($"Move {i + 1}: {BoardUtils.ConvertToAlg(move)}");

    game.SimplerMakeMove(move, out MoveInfo _);
}

stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");


