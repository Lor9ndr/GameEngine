using GameEngine.Enums;
using GameEngine.GameObjects;

namespace GameEngine.Intefaces
{
    public interface IRenderable
    {
        public void Render(Shader shader);
        public void Update();
    }
}
