using mario_monogame.Core;
using System;
using System.IO;

internal class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    private static void Main(string[] args)
    {
        try
        {
            File.AppendAllText("game.log", $"[{DateTime.Now}] Game starting...\n");

            // Используем основную версию игры
            using var game = new MarioPlatformerGame();
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