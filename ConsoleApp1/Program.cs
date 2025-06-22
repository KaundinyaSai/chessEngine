
using System.Diagnostics;

Magic.AttackTablesInit();

GameState game = new GameState("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10 ");

Stopwatch stopwatch = Stopwatch.StartNew();

Search.PerftDivide(game, 4);

stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");

