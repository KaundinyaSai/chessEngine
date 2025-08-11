
using System.Diagnostics;
using ChessEngine;

GameState game = new GameState();

Stopwatch stopwatch = Stopwatch.StartNew();

Console.WriteLine(Search.Perft(game, 5));

stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");


