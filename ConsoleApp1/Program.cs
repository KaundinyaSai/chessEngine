
using System.Diagnostics;

Magic.AttackTablesInit();

GameState game = new GameState();

Stopwatch stopwatch = Stopwatch.StartNew();

Search.PerftDivide(game, 4);

stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");

