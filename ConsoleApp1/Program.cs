
using System.Diagnostics;

Magic.AttackTablesInit();

GameState game = new GameState();

Stopwatch stopwatch = Stopwatch.StartNew();

Console.WriteLine(Search.Perft(game, 6));

stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");

