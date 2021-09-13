﻿using GameEngine.Bases;
using GameEngine.Bases.Components;
using GameEngine.RenderPrepearings;
using GameEngine.RenderPrepearings.FrameBuffers.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameObjects.Lights
{
    public class DirectLight : Light
    {

        public DirectLight(Mesh mesh, LightData lightData, Transform transform) : base(mesh, lightData, transform)
        {
            ShadowData.Shadow = new FrameBuffer(Game.ShadowSize, ClearBufferMask.DepthBufferBit);
            ShadowData.Shadow.Setup();
            ShadowData.Shadow.Bind();
            ShadowData.Shadow.DisableColorBuffer();
            ShadowData.Shadow.AttachTexture2DMap(FramebufferAttachment.DepthAttachment, PixelInternalFormat.DepthComponent, PixelType.Float);
            FarPlane = 100.0f;
            NearPlane = 1.0f;
            UpdateMatrices();

        }

        public override void Render(Shader shader,RenderFlags flags, int textureIdx)
        {
            Logger.ClearError();
            string name = "dirLight.";
            Transform.Render(shader, name);
            LightData.Render(shader, name);
            shader.SetVector3(name + "lightColor", LightData.Color);
            shader.SetMatrix4(name + "lightSpaceMatrix", LightSpaceMatrix);
            shader.SetFloat(name + "farPlane", FarPlane);
            GL.ActiveTexture(TextureUnit.Texture0 + textureIdx);
            ShadowData.Shadow.Texture.Bind(TextureTarget.Texture2D);
            shader.SetInt(name + "shadow", textureIdx);
            shader.SetFloat("far_plane", FarPlane);
            SetupModel(shader);
            if (flags.HasFlag(RenderFlags.Mesh))
            {
                DrawMesh(shader);
            }
            UpdateMatrices();
        }
        public override Matrix4 GetProjection => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(179.0f),1.0f, NearPlane, FarPlane);
        public override void UpdateMatrices()
        {

            // TODO: Strange Bag here  Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Direction, Vector3.UnitY);
            // Have to fix it!!
            Matrix4 view = Matrix4.LookAt(Transform.Position, Transform.Direction, Vector3.UnitY);
            LightSpaceMatrix =  view * GetProjection;
        }
    }
}
