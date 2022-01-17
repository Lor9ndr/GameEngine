using Engine.GameObjects.Lights;
using Engine.GLObjects.FrameBuffers;
using Engine.Rendering.GameObjects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace Engine.Components
{
    public struct ShadowData : IComponent, IDisposable
    {
        private const FramebufferAttachment _attachment = FramebufferAttachment.DepthAttachment;
        private const PixelInternalFormat _format = PixelInternalFormat.DepthComponent;
        private const PixelType _type = PixelType.Float;
        private static int id = 31;

        private FrameBuffer _shadow;
        private GameObject _attachedObject;

        public FrameBuffer Shadow { get => _shadow; set => _shadow = value; }
        public GameObject AttachedObject { get => _attachedObject; set => _attachedObject = value; }

        public ShadowData(FrameBuffer shadow)
        {
            _shadow = shadow;
            _attachedObject = null;
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

        public void Render(PointLight light)
        {
            RefreshTextureID();
            Game.EngineGL.ActiveTexture(TextureUnit.Texture0 + id)
                .BindTexture(TextureTarget.TextureCubeMap, light.ShadowData.Shadow.Texture)
                .SetShaderData($"pointLights[{light.PointLightID}].shadow", id);
            id--;
        }
        public void Render(DirectLight light)
        {
            RefreshTextureID();
            Game.EngineGL.ActiveTexture(TextureUnit.Texture0 + id)
                .BindTexture(TextureTarget.Texture2D, light.ShadowData.Shadow.Texture)
                .SetShaderData("dirLight.shadow", id);

            id--;
        }
        public void Render(SpotLight light)
        {
            RefreshTextureID();
            Game.EngineGL.ActiveTexture(TextureUnit.Texture0 + id)
              .BindTexture(TextureTarget.Texture2D, light.ShadowData.Shadow.Texture)
              .SetShaderData($"spotLights[{light.SpotLightId}].shadow", id);

            id--;
        }
        public static void RefreshTextureID()
        {
            if (id == 0)
            {
                id = 31;
            }
        }
        public static void SetTextureIdDefault() => id = 31;

        public void AttachGameObject(GameObject gameObject)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Shadow?.Dispose();
        }
    }
}
