
// File for debugging

using System;
using System.Diagnostics;

using Chess;

GameState game = new GameState("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq");
game.LoadBoard();

Stopwatch stopwatch = Stopwatch.StartNew();

long totalNodes = Search.PerftDivide(game, 2); 

stopwatch.Stop();

Console.WriteLine($"Time taken: {stopwatch.Elapsed.TotalSeconds} seconds");





