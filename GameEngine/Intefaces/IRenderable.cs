using GameEngine.Bases;
using GameEngine.Enums;
using GameEngine.GameObjects;

namespace GameEngine.Intefaces
{
    public interface IRenderable
    {
        public void Render(Shader shader, RenderFlags flags);
        public void Update();
    }
}
