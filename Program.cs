using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, Parallel World!");

        int size = 1024;

        int[,] grid, originalGrid = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                originalGrid[i, j] = 1;
            }
        }
        
        PrintGrid("Before", 0, originalGrid);

        //Sequential
        grid = (int[,])originalGrid.Clone();
        Thread thread = new Thread(() =>
        {
            FloodFill(grid, 50, 50, 5);
        }, 1024 * 1024 * 1024); // 1024 MB stack size

        thread.Start();
        thread.Join();           

        Console.ReadLine();
    }


    static void FloodFill(int[,] grid, int x, int y, int newNumber, bool parallel = false)
    {
        int oldNumber = grid[x, y];
        if (oldNumber == newNumber) return;

        Stopwatch stopwatch = Stopwatch.StartNew();

        if (parallel)
            ParallelFill(grid, x, y, oldNumber, newNumber);
        else
            Fill(grid, x, y, oldNumber, newNumber);

        stopwatch.Stop();

        if (parallel)
            PrintGrid("After Parallel", stopwatch.ElapsedMilliseconds, grid);
        else
            PrintGrid("After Serial", stopwatch.ElapsedMilliseconds, grid);
    }

    static void Fill(int[,] grid, int x, int y, int oldNumber, int newNumber)
    {
        if (x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1)) return;
        if (grid[x, y] != oldNumber) return;

        grid[x, y] = newNumber;

        Fill(grid, x - 1, y, oldNumber, newNumber); // Up
        Fill(grid, x + 1, y, oldNumber, newNumber); // Down
        Fill(grid, x, y - 1, oldNumber, newNumber); // Left
        Fill(grid, x, y + 1, oldNumber, newNumber); // Right
    }

    static void ParallelFill(int[,] grid, int x, int y, int oldNumber, int newNumber, int depth = 0, int maxDepth = 1)
    {
        if (x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1)) return;
        if (grid[x, y] != oldNumber) return;

        grid[x, y] = newNumber;


        if (depth > maxDepth)
        {
            ParallelFill(grid, x - 1, y, oldNumber, newNumber, depth + 1, maxDepth);
            ParallelFill(grid, x + 1, y, oldNumber, newNumber, depth + 1, maxDepth);
            ParallelFill(grid, x, y - 1, oldNumber, newNumber, depth + 1, maxDepth);
            ParallelFill(grid, x, y + 1, oldNumber, newNumber, depth + 1, maxDepth);
        }
        else
        {
            Parallel.Invoke(
                new ParallelOptions { MaxDegreeOfParallelism = 2 },
                () => ParallelFill(grid, x - 1, y, oldNumber, newNumber, depth + 1, maxDepth) // Up
            );
            Parallel.Invoke(
                new ParallelOptions { MaxDegreeOfParallelism = 2 },
                () => ParallelFill(grid, x + 1, y, oldNumber, newNumber, depth + 1, maxDepth) // Down
            );
            Parallel.Invoke(
                new ParallelOptions { MaxDegreeOfParallelism = 2 },
                () => ParallelFill(grid, x, y - 1, oldNumber, newNumber, depth + 1, maxDepth) // Left
            );
            Parallel.Invoke(
                new ParallelOptions { MaxDegreeOfParallelism = 2 },
                () => ParallelFill(grid, x, y + 1, oldNumber, newNumber, depth + 1, maxDepth)  // Right
            );
        }
    }

    static void PrintGrid(string firstText, long algorithmTime,  int[,] grid)
    {
        /*
        Console.WriteLine(firstText + " Fill:");
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                Console.Write(grid[i, j] + " ");
            }
            Console.WriteLine();
        }
        */
        
         Console.WriteLine($"{firstText} FloodFill took: {algorithmTime} ms\n");
    }
}