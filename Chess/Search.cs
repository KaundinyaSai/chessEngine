namespace Chess;
public static class Search
{
    public static long PerftDivide(GameState game, int depth)
    {
        var moves = game.GetAllMoves();
        long total = 0;

        foreach (var move in moves)
        {
            game.MakeMove(move);
            long count = Perft(game, depth - 1);
            game.UnmakeMove(move);

            Console.WriteLine($"{move.ToAlgebraic()} : {count}");
            total += count;
        }

        Console.WriteLine($"Total nodes: {total}");
        return total;
    }


    public static long Perft(GameState game, int depth)
    {
        if (depth == 0)
        {
            return 1;
        }

        long nodes = 0;
        var moves = game.GetAllMoves();

        foreach (var move in moves)
        {
            if (!game.IsStrictlyLegalMove(move))
            {
                continue; // Skip illegal moves
            }

            game.MakeMove(move);
            nodes += Perft(game, depth - 1);
            game.UnmakeMove(move);
        }

        return nodes;
    }
}