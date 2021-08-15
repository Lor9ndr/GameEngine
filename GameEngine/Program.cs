using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;

namespace GameEngine
{
    class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1920, 1080),
                Title = "OpenTK",
            };

            var window = new Game(GameWindowSettings.Default, nativeWindowSettings);
            window.Run();
        }
    }
}
