﻿using GameEngine.DefaultMeshes;
using GameEngine.GameObjects;
using GameEngine.GameObjects.Lights;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.RenderPrepearings.FrameBuffers
{
    public class DirectShadows
    {
        #region Private Properties
        private int _depthMapFBO;
        private int _depthMap;
        private Shader _depthShader;
        private Shader _lightShader;
        private Shader _debugShader;
        private Mesh _debugScreen = FullScreenQuad.GetQuad(); 
        private WorldRenderer _wr;
        private List<Light> _lights = new List<Light>();
        private TextureUnit ShadowTextureUnit = TextureUnit.Texture31;

        #endregion

        #region Public Properties
        public Vector2i ShadowSize = new Vector2i(2048, 2048);
        public float NearPlane = 1.0f;
        public float FarPlane = 10000f;
        #endregion

        #region Constructors
        public DirectShadows(int shadowWidth, int shadowHeight, WorldRenderer wr)
            :this(wr)
        {
            ShadowSize = new Vector2i(shadowWidth, shadowHeight);

        }
        public DirectShadows(WorldRenderer wr)
        {
            _wr = wr;
            foreach (var item in _wr.Lights)
            {
                if (item.GetType() == typeof(DirectLight))
                {
                    _lights.Add(item);
                }
            }
            //_lights = _wr.Lights.Where(l => l.GetType() == typeof(SpotLight) || l.GetType() == typeof(DirectLight)).ToList();
        }
        public DirectShadows(Vector2i shadowSize, WorldRenderer wr)
            :this(wr)
        {
            ShadowSize = shadowSize;
        }
        #endregion

        #region Public Methods
        public void Setup()
        {
            _depthMapFBO = GL.GenFramebuffer();

            _depthMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, ShadowSize.X, ShadowSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, _depthMap, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            else
            {
            }
            _depthShader = new Shader(Game.DIRECT_SHADOW_SHADERS_PATH + "Depth.vs", Game.DIRECT_SHADOW_SHADERS_PATH + "Depth.fr");
            _lightShader = new Shader(Game.DIRECT_SHADOW_SHADERS_PATH + "SimpleShader.vs", Game.DIRECT_SHADOW_SHADERS_PATH + "SimpleShader.fr");
            _debugShader = new Shader(Game.DIRECT_SHADOW_SHADERS_PATH + "DepthScreen.vs", Game.DIRECT_SHADOW_SHADERS_PATH + "DepthScreen.fr");
            _lightShader.SetInt("material.texture_diffuse0", 0);
            _lightShader.SetInt("shadowMap", 31);


        }

        public void RenderBuffer(Camera camera)
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, ShadowSize.X, ShadowSize.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            foreach (var item in _lights.OfType<DirectLight>())
            {
                ConfigureShaderAndMatrices(_depthShader, camera, item);
            }
            _wr.Render(camera, _depthShader);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // GL.Viewport(0, 0, Game.WIDTH, Game.HEIGHT);
            _debugShader.Use();
            _debugShader.SetFloat("near_plane", NearPlane);
            _debugShader.SetFloat("far_plane", FarPlane);
            GL.ActiveTexture(ShadowTextureUnit);
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            _debugScreen.Draw(PrimitiveType.TriangleStrip);

            /*            GL.Viewport(0, 0, Game.WIDTH, Game.HEIGHT);
                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                        foreach (var item in _lights.OfType<DirectLight>())
                        {
                            ConfigureShaderAndMatrices(_lightShader, camera, item);

                        }
                        GL.ActiveTexture(ShadowTextureUnit);
                        GL.BindTexture(TextureTarget.Texture2D, _depthMap);
                        _wr.Render(camera, _lightShader);*//*
                    */
        }
        #endregion

        #region Private Methods
        private void ConfigureShaderAndMatrices(Shader shader, Camera camera, Light light)
        {

           Matrix4 lightProjection = Matrix4.CreateOrthographicOffCenter(-FarPlane, FarPlane, -FarPlane, FarPlane, NearPlane, FarPlane);
           // Matrix4 lightProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(120), ShadowSize.X/ShadowSize.Y, NearPlane, FarPlane);
            Matrix4 lightView = Matrix4.LookAt(light.Position , light.Direction, Vector3.UnitY);
            Matrix4 lightSpaceMatrix =  lightProjection * lightView;
            shader.Use();
            shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            shader.SetFloat("far_plane", NearPlane);
            shader.SetFloat("near_plane", FarPlane);
        }
        #endregion
    }
}
