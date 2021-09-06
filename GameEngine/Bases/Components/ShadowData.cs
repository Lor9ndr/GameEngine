using GameEngine.Bases;
using GameEngine.GameObjects;
using GameEngine.GameObjects.Lights;
using GameEngine.RenderPrepearings.FrameBuffers.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Bases.Components
{
    public class ShadowData : IComponent
    {
        private FrameBuffer _shadow;

        public FrameBuffer Shadow { get => _shadow; set => _shadow = value; }

        public ShadowData()
        {
        }

        /// <summary>
        /// May be it will be usefull somehow
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="light"></param>
        /// <param name="textureIdx"></param>
        public void Render(Shader shader, Light light, int textureIdx)
        {
            var type = light.GetType();
            if (type == typeof(PointLight))
            {
                var tmp = (PointLight)light;
                GL.ActiveTexture(TextureUnit.Texture0 + textureIdx);
                Shadow.CubeMap.Bind();
                shader.SetInt($"pointLightBuffers[{tmp.PointLightID}]", textureIdx);
            }
            else if (type == typeof(DirectLight))
            {
                GL.ActiveTexture(TextureUnit.Texture0 + textureIdx);
                Shadow.Texture.Bind(TextureTarget.Texture2D);
                shader.SetInt("dirLight.shadow", textureIdx);

            }
            else if (type == typeof(SpotLight))
            {

            }
        }

    }
}
