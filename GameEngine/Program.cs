using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;

namespace GameEngine
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var window = new Game(GameWindowSettings.Default);
            window.Run();
        }
    }
}
