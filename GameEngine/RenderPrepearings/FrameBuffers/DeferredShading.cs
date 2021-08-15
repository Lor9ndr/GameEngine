using GameEngine.GameObjects;
using GameEngine.Structs;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.RenderPrepearings.FrameBuffers
{
    public class DeferredShading
    {
        private const string shaderPath = "../../../Shaders/DeferredShading/";
        Shader shaderGeometryPass;
        Shader shaderLightingPass;
        Shader shaderLightBox;
        private WorldRenderer _world;
        private int _gBuffer;
        private int _gPosition;
        private int _gNormal;
        private int _gAlbedoSpec;
        private int _rboDepth;
        private int[] _textures;

        static int quadVAO = 0;
        private int _finalTexture;

        public DeferredShading(WorldRenderer wr)
        {
            _world = wr;
            shaderGeometryPass = new Shader(shaderPath + "GBufferV.glsl", shaderPath + "GBufferFR.glsl");
            shaderLightingPass = new Shader(shaderPath + "DefferedShadingV.glsl", shaderPath + "DefferedShadingFR.glsl");
            shaderLightBox = new Shader(shaderPath + "LightBoxV.glsl", shaderPath + "LightBoxFR.glsl");
        }
        public void RenderQuad()
        {
            int quadVBO;
            if (quadVAO == 0)
            {
                float[] quadVertices = {
            // positions        // texture Coords
            -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
             1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
             1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
            };
                // setup plane VAO
                quadVAO = GL.GenVertexArray();
                quadVBO = GL.GenBuffer();
                GL.BindVertexArray(quadVAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length, quadVertices, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (3 * sizeof(float)));
            }
            GL.BindVertexArray(quadVAO);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.BindVertexArray(0);
        }
        public void Setup()
        {
            _gBuffer = GL.GenBuffer();
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _gBuffer);
            _textures = new int[3];
            GL.GenTextures(_textures.Length, _textures);
            _rboDepth = GL.GenTexture();
            for (int i = 0; i < _textures.Length; i++)
            {
                GL.BindTexture(TextureTarget.Texture2D, _textures[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.Rgb, PixelType.Float, (IntPtr)null);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, _textures[i], 0);
            }
            GL.BindTexture(TextureTarget.Texture2D, _rboDepth);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth32fStencil8, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, _rboDepth, 0);

            GL.BindTexture(TextureTarget.Texture2D, _finalTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.Rgb, PixelType.Float, (IntPtr)null);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, TextureTarget.Texture2D, _finalTexture, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("FRAMEBUFFER NOT COMPLETE!!!");
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            shaderLightingPass.SetInt("gColorMap", _textures[0]);

            /*
                       _gPosition = GL.GenTexture();
                       GL.BindTexture(TextureTarget.Texture2D, _gPosition);
                       GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, (IntPtr)null);
                       GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                       GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                       GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _gPosition, 0);

                       _gNormal = GL.GenTexture();
                       GL.BindTexture(TextureTarget.Texture2D, _gNormal);
                       GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, (IntPtr)null);
                       GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                       GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                       GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, _gNormal, 0);

                       _gAlbedoSpec = GL.GenTexture();
                       GL.BindTexture(TextureTarget.Texture2D, _gAlbedoSpec);
                       GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, (IntPtr)null);
                       GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                       GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                       GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, _gAlbedoSpec, 0);



                       _rboDepth = GL.GenRenderbuffer();
                       GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rboDepth);
                       GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Game.WIDTH, Game.HEIGHT);
                       GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _rboDepth);
                       if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                       {
                           throw new Exception("FRAMEBUFFER NOT COMPLETE!!!");
                       }
                       GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                       shaderLightingPass.Use();
                       GL.Enable(EnableCap.Texture2D);*/
        }
        public void RenderBuffers(Camera camera)
        {
            shaderGeometryPass.Use();
            // startFrame
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _gBuffer);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment4);
            GL.Clear(ClearBufferMask.ColorBufferBit);


            // Bind For geom pass
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _gBuffer);
            DrawBuffersEnum[] attachments = {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2,
            };
            GL.DrawBuffers(attachments.Length, attachments);


            GL.DepthMask(true);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            _world.RenderObjects(camera, shaderGeometryPass);
            GL.DepthMask(false);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.StencilTest);
            // STENCIL PASS
            GL.DrawBuffer(DrawBufferMode.None);
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Clear(ClearBufferMask.StencilBufferBit);

            GL.StencilFunc(StencilFunction.Always, 0, 0);
            GL.StencilOpSeparate(StencilFace.Back, StencilOp.Keep, StencilOp.IncrWrap, StencilOp.Keep);
            GL.StencilOpSeparate(StencilFace.Front, StencilOp.Keep, StencilOp.DecrWrap, StencilOp.Keep);
            _world.RenderLights(camera, shaderLightBox);

            GL.Disable(EnableCap.StencilTest);
            // LIGHT PASS
            GL.DrawBuffer(DrawBufferMode.ColorAttachment4);
            for (int i = 0; i < _textures.Length; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GL.BindTexture(TextureTarget.Texture2D, _textures[i]);
            }

            GL.StencilFunc(StencilFunction.Notequal, 0, 0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            for (int i = 0; i < _world.Lights.Count; i++)
            {
                var light = _world.Lights[i];
                const float constant = 1.0f; // note that we don't send this to the shader, we assume it is always 1.0 (in our case)
                const float linear = 0.7f;
                const float quadratic = 1.8f;
                shaderLightingPass.SetVector3($"lights[{i}].position", light.Position);
                shaderLightingPass.SetVector3($"lights[{i}].color", light.LightColor);

                shaderLightingPass.SetFloat($"lights[{i}].linear", linear);
                shaderLightingPass.SetFloat($"lights[{i}].quadratic", quadratic);
                float maxBrightness = MathF.Max(MathF.Max(light.LightColor.X, light.LightColor.Y), light.LightColor.Z);
                float radius = (-linear + MathF.Sqrt(linear * linear - 4 * quadratic * (constant - (256.0f / 5.0f) * maxBrightness))) / (2.0f * quadratic);
                shaderLightingPass.SetFloat($"lights[{i}].radius", radius);
            }
            shaderLightingPass.SetVector2("gScreenSize", new Vector2(Game.WIDTH, Game.HEIGHT));
            shaderLightingPass.SetVector3("viewPos", camera.Position);
            shaderLightingPass.SetInt("gPositionMap", _textures[0]);
            shaderLightingPass.SetInt("gColorMap", _textures[1]);
            shaderLightingPass.SetInt("gNormalMap", _textures[2]);
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Blend);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _gBuffer);
            int halfWidth = Game.WIDTH / 2;
            int halfHeight = Game.HEIGHT / 2;
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, 0, halfWidth, halfHeight, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

            GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, halfHeight, halfWidth, Game.HEIGHT, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment2);
            GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, halfWidth, halfHeight, Game.WIDTH, Game.HEIGHT, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment3);
            GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, halfWidth, 0, Game.WIDTH, halfHeight, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);


        }








        /*int halfWidth = Game.WIDTH / 2;
        int halfHeight = Game.HEIGHT / 2;
        GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
        GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, 0, halfWidth, halfHeight, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

        GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
        GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, halfHeight, halfWidth, Game.HEIGHT, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
        GL.ReadBuffer(ReadBufferMode.ColorAttachment2);
        GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, halfWidth, halfHeight, Game.WIDTH, Game.HEIGHT, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
        GL.ReadBuffer(ReadBufferMode.ColorAttachment3);
        GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, halfWidth, 0, Game.WIDTH, halfHeight, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
*/





        /* GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, _gBuffer);

         shaderGeometryPass.Use();
         shaderGeometryPass.SetMatrix4("projection", camera.GetProjectionMatrix());
         shaderGeometryPass.SetMatrix4("view", camera.GetViewMatrix());
         _world.RenderObjects(camera, shaderGeometryPass);
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

         shaderLightingPass.Use();
         GL.ActiveTexture(TextureUnit.Texture0);
         GL.BindTexture(TextureTarget.Texture2D, _gPosition);
         GL.ActiveTexture(TextureUnit.Texture1);
         GL.BindTexture(TextureTarget.Texture2D, _gNormal);
         GL.ActiveTexture(TextureUnit.Texture2);
         GL.BindTexture(TextureTarget.Texture2D, _gAlbedoSpec);


         for (int i = 0; i < _world.Lights.Count; i++)
         {
             var light = _world.Lights[i];
             const float constant = 1.0f; // note that we don't send this to the shader, we assume it is always 1.0 (in our case)
             const float linear = 0.7f;
             const float quadratic = 1.8f;
             shaderLightingPass.SetVector3($"lights[{i}].position", light.Position);
             shaderLightingPass.SetVector3($"lights[{i}].color", light.LightColor);

             shaderLightingPass.SetFloat($"lights[{i}].linear", linear);
             shaderLightingPass.SetFloat($"lights[{i}].quadratic", quadratic);
             float maxBrightness = MathF.Max(MathF.Max(light.LightColor.X, light.LightColor.Y), light.LightColor.Z);
             float radius = (-linear + MathF.Sqrt(linear * linear - 4 * quadratic * (constant - (256.0f / 5.0f) * maxBrightness))) / (2.0f * quadratic);
             shaderLightingPass.SetFloat($"lights[{i}].radius", radius);
         }
         shaderLightingPass.SetVector3("viewPos", camera.Position);
         shaderLightingPass.SetInt("gPosition", _gPosition);
         shaderLightingPass.SetInt("gNormal", _gNormal);
         shaderLightingPass.SetInt("gAlbedoSpec", _gAlbedoSpec);
         RenderQuad();

         GL.BindFramebuffer( FramebufferTarget.ReadFramebuffer, _gBuffer);
         GL.BindFramebuffer( FramebufferTarget.DrawFramebuffer, 0); // буфер глубины по-умолчанию
         GL.BlitFramebuffer(
           0, 0, Game.WIDTH, Game.HEIGHT, 0, 0, Game.WIDTH, Game.HEIGHT,  ClearBufferMask.StencilBufferBit,  BlitFramebufferFilter.Nearest
         );
         GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0);
         shaderLightBox.Use();
         shaderLightBox.SetMatrix4("projection", camera.GetProjectionMatrix());
         shaderLightBox.SetMatrix4("view", camera.GetViewMatrix());
         _world.RenderLights(camera, shaderLightBox);*/


        /*            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _gBuffer);

                    GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                    GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, 0, Game.WIDTH / 2, Game.HEIGHT / 2, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                    GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
                    GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, Game.WIDTH / 2, 0, Game.WIDTH, Game.HEIGHT / 2, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                    GL.ReadBuffer(ReadBufferMode.ColorAttachment2);
                    GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, Game.HEIGHT / 2, Game.WIDTH / 2, Game.HEIGHT, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                    GL.ReadBuffer(ReadBufferMode.ColorAttachment3);
                    GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, Game.WIDTH / 2, Game.WIDTH / 2, Game.WIDTH, Game.HEIGHT, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);*/



    }
    /*public class DeferredShading
    {
        private const string shaderPath = "../../../Shaders/DeferredShading/";
        Shader shaderGeometryPass;
        Shader shaderLightingPass;
        Shader shaderLightBox;
        Shader finalShader;
        private WorldRenderer _world;
        private int _gBuffer;
        private int _gPosition;
        private int _gNormal;
        private int _gAlbedoSpec;
        private int _rboDepth;
        private int _depth;
        private static Vertex[] quadVertices = {
            // positions        // texture Coords
            new Vertex(new Vector3(-1.0f,  1.0f, 0.0f),new Vector2( 0.0f, 1.0f)),
            new Vertex(new Vector3(-1.0f, -1.0f, 0.0f),new Vector2( 0.0f, 0.0f)),
            new Vertex(new Vector3( 1.0f,  1.0f, 0.0f), new Vector2(1.0f, 1.0f)),
            new Vertex(new Vector3(1.0f, -1.0f, 0.0f),new Vector2( 1.0f, 0.0f)),
            };
        private static int[] quadIndices =
        {
              0, 1, 3,
              1, 2, 3,
        };
        private static Mesh quad = new Mesh(quadVertices, quadIndices);

        static int quadVAO = 0;
        private int[] _textures = new int[4];
        PixelInternalFormat[] _formats = new PixelInternalFormat[]{
            PixelInternalFormat.Rgb32f, //Position -> we need float values
            PixelInternalFormat.Rgba, //Diffuse -> we need color values [0-1]
            PixelInternalFormat.Rgb32f, //Normal -> Same as position
            PixelInternalFormat.Rgba, //Light -> Same as diffuse

        };
        public DeferredShading(WorldRenderer wr)
        {
            _world = wr;
            shaderGeometryPass = new Shader(shaderPath + "GBuffer.vs", shaderPath + "GBuffer.fr");
            shaderLightingPass = new Shader(shaderPath + "DefferedShading.vs", shaderPath + "DefferedShading.fr");
            shaderLightBox = new Shader(shaderPath + "LightBox.vs", shaderPath + "LightBox.fr");
            finalShader = new Shader(shaderPath + "Final.vs", shaderPath + "Final.fr");
        }
        public void RenderQuad()
        {
            int quadVBO;
            if (quadVAO == 0)
            {

                // setup plane VAO
                quadVAO = GL.GenVertexArray();
                quadVBO = GL.GenBuffer();
                GL.BindVertexArray(quadVAO);
                GL.BindBuffer( BufferTarget.ArrayBuffer, quadVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length, quadVertices,  BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3,  VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (3 * sizeof(float)));
            }
            GL.BindVertexArray(quadVAO);
            GL.DrawElements(PrimitiveType.Triangles, quadIndices.Length, DrawElementsType.UnsignedInt, 0); ;
            GL.BindVertexArray(0);
        }
        public void Setup()
        {
            _gBuffer = GL.GenBuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _gBuffer);

*//*            for (int i = 0; i < _textures.Length; i++)
            {
                _textures[i] = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _textures[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, _formats[i], Game.WIDTH, Game.HEIGHT, 0, PixelFormat.Rgb, PixelType.Float, (IntPtr)null);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, _textures[i], 0);
            }*/
    /*
                // depth

                GL.BindTexture(TextureTarget.Texture2D, _depth);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
                GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, _depth, 0);
                DrawBuffersEnum[] drawBuffers =
                    {
                    DrawBuffersEnum.ColorAttachment0,
                    DrawBuffersEnum.ColorAttachment1,
                    DrawBuffersEnum.ColorAttachment2,
                    DrawBuffersEnum.ColorAttachment3,
                    };
                GL.DrawBuffers(drawBuffers.Length, drawBuffers);*//*


                _gPosition = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _gPosition);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, (IntPtr)null);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _gPosition, 0);

                _gNormal = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _gNormal);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, (IntPtr)null);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, _gNormal, 0);

                _gAlbedoSpec = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _gAlbedoSpec);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Game.WIDTH, Game.HEIGHT, 0, PixelFormat.Rgba, PixelType.Float, (IntPtr)null);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, _gAlbedoSpec, 0);


                DrawBuffersEnum[] attachments = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 };
                GL.DrawBuffers(3, attachments);
                _rboDepth = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rboDepth);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Game.WIDTH, Game.HEIGHT);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _rboDepth);
                if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                {
                    throw new Exception("FRAMEBUFFER NOT COMPLETE!!!");
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                shaderLightingPass.Use();
                shaderLightingPass.SetInt("gPosition", 0);
                shaderLightingPass.SetInt("gNormal", 1);
                shaderLightingPass.SetInt("gAlbedoSpec", 2);
            }
            public void RenderBuffers(Camera camera, Game window)
            {
                GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


                GL.BindFramebuffer(FramebufferTarget.Framebuffer, _gBuffer);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                _world.RenderObjects(camera, shaderGeometryPass);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                shaderLightingPass.Use();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _gPosition);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, _gNormal);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, _gAlbedoSpec);

                for (int i = 0; i < _world.Lights.Count; i++)
                {
                    var light = _world.Lights[i];
                    const float constant = 1.0f; // note that we don't send this to the shader, we assume it is always 1.0 (in our case)
                    const float linear = 0.7f;
                    const float quadratic = 1.8f;
                    shaderLightingPass.SetVector3($"lights[{i}].position", light.Position);
                    shaderLightingPass.SetVector3($"lights[{i}].color", light.LightColor);

                    shaderLightingPass.SetFloat($"lights[{i}].linear", linear);
                    shaderLightingPass.SetFloat($"lights[{i}].quadratic", quadratic);
                    float maxBrightness = MathF.Max(MathF.Max(light.LightColor.X, light.LightColor.Y), light.LightColor.Z);
                    float radius = (-linear + MathF.Sqrt(linear * linear - 4 * quadratic * (constant - (256.0f / 5.0f) * maxBrightness))) / (2.0f * quadratic);
                    shaderLightingPass.SetFloat($"lights[{i}].radius", radius);
                }
                shaderLightingPass.SetVector3("viewPos", camera.Position);
                quad.Draw(OpenTK.Graphics.OpenGL4.PrimitiveType.TriangleStrip);

                // Blit result to sceen
                if (window.Keyboard.IsKeyDown(Keys.F1))
                {
                    BlitToScreen();
                }
                else
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, _gBuffer);

                    GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, 0, Game.WIDTH, Game.HEIGHT, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    shaderLightBox.Use();
                    _world.RenderLights(camera, shaderLightBox);

                }
            }
            public void BlitToScreen()
            {
                GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, 0, Game.WIDTH, Game.HEIGHT, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _gBuffer);

                GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, 0, Game.WIDTH / 2, Game.HEIGHT / 2, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
                GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, Game.WIDTH / 2, 0, Game.WIDTH, Game.HEIGHT / 2, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                GL.ReadBuffer(ReadBufferMode.ColorAttachment2);
                GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, 0, Game.HEIGHT / 2, Game.WIDTH / 2, Game.HEIGHT, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                GL.ReadBuffer(ReadBufferMode.ColorAttachment3);
                GL.BlitFramebuffer(0, 0, Game.WIDTH, Game.HEIGHT, Game.WIDTH / 2, Game.WIDTH / 2, Game.WIDTH, Game.HEIGHT, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            }

        }*/
}
