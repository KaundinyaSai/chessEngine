
using System.Diagnostics;

GameState game = new GameState();

Stopwatch stopwatch = Stopwatch.StartNew();

Move move = Search.FindBestMove(game, 5);
Console.WriteLine($"{move.fromIndex} -> {move.toIndex}");

stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");

