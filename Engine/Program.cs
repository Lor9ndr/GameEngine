using OpenTK.Windowing.Desktop;

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
