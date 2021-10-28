using OpenTK.Windowing.Desktop;
using System;

namespace Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(GameWindowSettings.Default);
            game.Run();
        }
    }
}
