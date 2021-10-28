using Engine.GameObjects.Lights;
using Engine.GLObjects.FrameBuffers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Components
{
    public struct ShadowData : IComponent
    {
        private const FramebufferAttachment _attachment = FramebufferAttachment.DepthAttachment;
        private const PixelInternalFormat _format = PixelInternalFormat.DepthComponent;
        private const PixelType _type = PixelType.Float;
        private static int id = 31;

        private FrameBuffer _shadow;

        public FrameBuffer Shadow { get => _shadow; set => _shadow = value; }
        public ShadowData(FrameBuffer shadow)
        {
            _shadow = shadow;
            //Game.ChangeShadowsSize += Game_ChangeShadowsSize;
        }

        /// <summary>
        /// Doesn't work as needed
        /// </summary>
        /// <param name="shadows"></param>
        private void Game_ChangeShadowsSize(Vector2i shadows)
        {
            Shadow.DeleteBuffer();
            Shadow = GetDefaultFrameBuffer();
            Shadow.Setup();
            Shadow.Bind();
            Shadow.DisableColorBuffer();
            if (Shadow.Texture != null)
            {
                AttachTexture2DMap();
            }
            else
            {
                AttachCubeMap();
            }
        }
        public void AttachTexture2DMap() => Shadow.AttachTexture2DMap(_attachment, _format, _type);
        public void AttachCubeMap() => Shadow.AttachCubeMap(_attachment, _format, _type);

        public static FrameBuffer GetDefaultFrameBuffer() => new FrameBuffer(Game.ShadowSize, ClearBufferMask.DepthBufferBit);

        /// <summary>
        /// May be it will be usefull somehow
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="light"></param>
        /// <param name="textureIdx"></param>
        public void Render(Shader shader, Light light)
        {
            var type = light.GetType();
            RefreshTextureID();
            if (type == typeof(PointLight))
            {
                var tmp = (PointLight)light;
                GL.ActiveTexture(TextureUnit.Texture0 + id);
                Shadow.CubeMap.Bind();
                shader.SetInt($"pointLights[{tmp.PointLightID}].shadow", id);
            }
            else if (type == typeof(DirectLight))
            {
                GL.ActiveTexture(TextureUnit.Texture0 + id);
                Shadow.Texture.Bind(TextureTarget.Texture2D);
                shader.SetInt("dirLight.shadow", id);

            }
            else
            {
                var tmp = (SpotLight)light;
                GL.ActiveTexture(TextureUnit.Texture0 + id);
                Shadow.Texture.Bind(TextureTarget.Texture2D);
                shader.SetInt($"spotLights[{tmp.ID}].shadow", id);
            }
            id--;
        }
        private void RefreshTextureID()
        {
            if (id == -1)
            {
                id = 31;
            }
        }

    }
}
