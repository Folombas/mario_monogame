using mario_monogame.Core;
using System;
using System.IO;

internal class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// This creates an instance of your game and calls it's Run() method
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    private static void Main(string[] args)
    {
        try
        {
            // Логирование запуска
            File.AppendAllText("game.log", $"[{DateTime.Now}] Game starting...\n");
            
            using var game = new mario_monogameGame();
            File.AppendAllText("game.log", $"[{DateTime.Now}] Game created, calling Run()\n");
            
            game.Run();
            
            File.AppendAllText("game.log", $"[{DateTime.Now}] Game exited normally\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText("game.log", $"[{DateTime.Now}] CRITICAL ERROR: {ex}\n");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }
}