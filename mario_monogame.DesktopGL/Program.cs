using mario_monogame.Core;
using System;

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
            using var game = new mario_monogameGame();
            game.Run();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Critical error: {ex}");
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }
}