
using System.Diagnostics;
using ChessEngine;

GameState game = new GameState();
Stopwatch stopwatch = Stopwatch.StartNew();

Move bestMove = Search.FindBestMove(game, 5);
Console.WriteLine($"From: {bestMove.fromIndex}, To: {bestMove.toIndex}");


stopwatch.Stop();
Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");

