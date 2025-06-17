using System;
using System.Diagnostics;

GameState game = new GameState();

Stopwatch stopwatch = Stopwatch.StartNew();

Search.PerftDivide(game, 1);

stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");


















