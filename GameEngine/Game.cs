using GameEngine.GameObjects;
using GameEngine.GameObjects.Base;
using GameEngine.GameObjects.Lights;
using GameEngine.Intefaces;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine.Factories;
using GameEngine.Bases;
using GameEngine.RenderPrepearings.FrameBuffers;
using GameEngine.DefaultMeshes;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.ImGui;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using ImGuiNET;

namespace GameEngine
{
    public class Game : GameWindow
    {
        #region Constructors

        public Game(GameWindowSettings gameWindowSettings)
            : base(gameWindowSettings, new NativeWindowSettings() { APIVersion = new Version(4, 6) })
        {
            CursorGrabbed = true;
        }
        #endregion

        #region Public Properties

        public static float FPS { get; private set; }
        public static float DeltaTime => _deltaTime;
        public static double Time => _time;
        public static int Width = 1920;
        public static int Height = 1080;
        public static Vector2i ShadowSize = new Vector2i(512, 512);
        public static Vector2i TextureSize = new Vector2i(1024, 1024);
        public KeyboardState Keyboard { get; private set; }
        public static ImGuiController Controller;

        #endregion

        #region Private Properties
        private double oldTimeSinceStart = 0;
        private static float _framesPerSecond = 0.0f;
        private static float _lastTime = 0.0f;
        private static double _time;
        private static float _deltaTime;
        private Camera _camera;
        private readonly List<IRenderable> _models = new();
        private List<Light> _lights = new();
        private WorldRenderer _worldRenderer;
        private Shader _lightBoxShader;
        private Light _sun;
        internal static int Shadows = 1;
        internal static int GammaEnable = 1;
        private bool enabled;
        private bool isGuiVisible = false;
        private GBuffer _gBuffer;
        private Mesh _fsQuad;
        private Shader _geomShader;
        private Shader _pointShader;
        private Shader _finalCombine;
        #endregion

        #region Const
        public const string RESOURCE_PATH = "../../../Resources/";
        public const string TEXTURES_PATH = RESOURCE_PATH + "Textures/";
        public const string OBJ_PATH = RESOURCE_PATH + "OBJ/";
        public const string NANOSUIT_PATH = OBJ_PATH + "NanoSuit/nanosuit.obj";
        public const string BRIDGE_PATH = OBJ_PATH + "Bridge/Jaaninoja_Bridge_decimated_SF.obj";
        public const string MAN_PATH = OBJ_PATH + "Man/man.fbx";
        public const string TERRAIN_PATH = OBJ_PATH + "Terrain/terrain1.fbx";
        public const string SKYBOX_TEXTURES_PATH = TEXTURES_PATH + "SkyBox/";

        public const string SHADERS_PATH = "../../../Shaders/";
        public const string SIMPLE_SHADER = SHADERS_PATH + "SimpleShader";
        public const string DEFERRED_RENDER_PATH = SHADERS_PATH + "DeferredShading/";
        public const string ANOTHER_SHADERS_PATH = DEFERRED_RENDER_PATH + "Another/";
        public const string SKYBOX_SHADER_PATH = SHADERS_PATH + "SkyBox/SkyBox";
        public const string SHADOW_SHADERS_PATH = SHADERS_PATH + "ShadowShaders/";
        public const string DIRECT_SHADOW_SHADERS_PATH = SHADOW_SHADERS_PATH + "DirectShadows/";
        public const string POINT_SHADOW_SHADERS_PATH = SHADOW_SHADERS_PATH + "PointShadows/";

        #endregion

        #region Overrides
        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _camera = new Camera(new Vector3(0, 0, 3), Size.X / (float)Size.Y);
            //SimpleShader = new Shader(SIMPLE_SHADER + ".vs", SIMPLE_SHADER + ".fr");
            _lightBoxShader = new Shader(SHADERS_PATH + "LightBox.vert", SHADERS_PATH + "LightBox.frag");
            _geomShader = new Shader(DEFERRED_RENDER_PATH + "DrawGeom.vert", DEFERRED_RENDER_PATH + "DrawGeom.frag");
            _pointShader = new Shader(DEFERRED_RENDER_PATH + "PointLight.vert", DEFERRED_RENDER_PATH + "PointLight.frag");
            _finalCombine = new Shader(DEFERRED_RENDER_PATH + "Final.vert", DEFERRED_RENDER_PATH + "Final.frag");
            Random rd = new();
            _fsQuad = FullScreenQuad.GetQuad();

            #region Models
            _models.Add(ModelFactory.GetManModel(new Vector3(0)));
            for (int i = 0; i < 15; i++)
            {
                var pos = new Vector3(rd.Next(-20, 15), rd.Next(0, 30), rd.Next(-15, 16));
                _models.Add(ModelFactory.GetNanoSuitModel(pos));

            }
            _models.Add(ModelFactory.GetTerrainModel(new Vector3(0)));

            #endregion

            #region Lights
            _sun = LightFactory.GetDirectLight(new Vector3(0.5555345f, 10.0f, 0.412412f), new Vector3(0.0f,0.0f,0.0f));
            //Light sl = LightFactory.GetSpotLight(_camera.Position, _camera.Front);
            //_lights.Add(sl);
            _lights.Add(_sun);

            for (int i = 0; i < 10; i++)
            {
                var position = new Vector3(rd.Next(-5, 6), rd.Next(0, 10), rd.Next(-5, 5));
                var direction = new Vector3(rd.Next(-100,100), rd.Next(-100,100), rd.Next(-100, 100));
                _lights.Add(LightFactory.GetSpotLight(position, direction));
            }

           for (int i = 0; i < 5; i++)
           {
               var position = new Vector3(rd.Next(-5, 6), rd.Next(0, 10), rd.Next(-5, 5));
               _lights.Add(LightFactory.GetPointLight(position));
           }
            #endregion

            _worldRenderer = new WorldRenderer(_models, _lights, this);
            _camera.Enable(this);

            Controller = new ImGuiController(this);
            this.Size = new Vector2i(Width, Height);
            GL.Enable(EnableCap.FramebufferSrgb);
            base.OnLoad();
            //_gBuffer = new GBuffer();
            //_gBuffer.Init(Width, Height);

            //ToggleFullscreen();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _worldRenderer.Render(_camera,RenderFlags.None, RenderFlags.MeshAndTextures);
            /*Logger.ClearError();

            _worldRenderer.DefaultFBO.Bind();
            GeomPass();
            LightPass();
            _worldRenderer.DefaultFBO.Activate();

            if (Keyboard.IsKeyDown(Keys.F1))
            {
                _gBuffer.DumpToScreen(Width, Height);
            }
            else
            {
                GL.ClearColor(Color4.CornflowerBlue);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.CullFace(CullFaceMode.Back);
                _finalCombine.Use();
                _finalCombine.SetInt("ColorBuffer", _gBuffer.DiffuseTexture);
                _finalCombine.SetInt("LightBuffer", _gBuffer.LightTexture);
                _fsQuad.Draw();

                //Light blub icon rendering
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                GL.CullFace(CullFaceMode.Front);
                _lightBoxShader.Use();
                _worldRenderer.SetupCamera(_camera, _lightBoxShader);
                foreach (var item in _worldRenderer.Lights)
                {
                    item.SetupModel(_lightBoxShader);
                    if (item.Mesh != null)
                    {
                        _lightBoxShader.SetInt("diffuse", 0);
                        item.Mesh.Textures[0].Use(TextureUnit.Texture0);
                        _lightBoxShader.SetVector3("lightColor", item.LightData.Color);
                        item.DrawMesh(_lightBoxShader);
                    }
                }

                GL.Disable(EnableCap.Blend);
            }

            Logger.CheckLastError();*/




#if DEBUG
            _lightBoxShader.Use();
            _worldRenderer.SetupCamera(_camera, _lightBoxShader);
            foreach (var item in _worldRenderer.Lights)
            {
                item.SetupModel(_lightBoxShader);
                if (item.Mesh != null)
                {
                    _lightBoxShader.SetInt("diffuse", 0);
                    item.Mesh.Textures[0].Use(TextureUnit.Texture0);
                    _lightBoxShader.SetVector3("lightColor", item.LightData.Color);
                    item.SetupModel(_lightBoxShader);
                    item.DrawMesh(_lightBoxShader);
                }
            }
#endif
            onDrawGUI();
            if (isGuiVisible)
            {
                Controller.Render();
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            CalculateFPS();
            var spotlight = _lights.OfType<SpotLight>().FirstOrDefault();
            Title =
                $"FPS: {FPS}," +
            $" Objects: {_models.Count}," +
            $" Lights: {_lights.Count}, " +
            $" CAMERAPOS: {_camera.Position}" +
            $" Shadows: {Shadows}" +
            $" Gamma : {GammaEnable}";
            _worldRenderer.Update();
            if ((bool)(KeyboardState.IsKeyDown(Keys.O)))
            {
                _lights.OfType<SpotLight>().FirstOrDefault().Transform.Position = _camera.Position;
                _lights.OfType<SpotLight>().FirstOrDefault().Transform.Direction = _camera.Front;
            }

            _worldRenderer.LightShader.Use();
            _worldRenderer.LightShader.SetInt("shadows", Shadows);
            _worldRenderer.LightShader.SetInt("gammaEnable", GammaEnable);
            _worldRenderer.LightShader.SetFloat("time", (float)Time);

            Controller.Update(this, _deltaTime);
            _time += args.Time;
            _deltaTime = (float)_time - (float)oldTimeSinceStart;
            HandleKeyBoard();
            base.OnUpdateFrame(args);
            oldTimeSinceStart = _time;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            // Update the opengl viewport
            GL.Viewport(0, 0, e.Width, e.Height);
            Width = Size.X;
            Height = Size.Y;

            // Tell ImGui of the new size
            Controller.WindowResized(e.Width, e.Height);
            base.OnResize(e);
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            Controller.MouseScroll(e.Offset);
        }
        #endregion

        #region Private Methods

        private void GeomPass()
        {
            _gBuffer.BindForGeometryPass();
            GL.Enable(EnableCap.DepthTest);
            {
                GL.CullFace(CullFaceMode.Back);

                GL.ClearColor(0,0,0,0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                _geomShader.Use();

                _worldRenderer.SetupCamera(_camera, _geomShader);
                _geomShader.SetVector3("AmbientColor", new Vector3(255, 255, 255));
                _geomShader.SetVector3("AmbientDirection", new Vector3(1));
                _geomShader.SetFloat("AmbientPower" ,(float)Math.Sin(2 * 6) + 1 / 2f);


                foreach (var b in _worldRenderer.GameObjects)
                {
                    b.Render(_geomShader, RenderFlags.MeshAndTextures);
                }
            }
            GL.Disable(EnableCap.DepthTest);
        }
        private void LightPass()
        {
            _gBuffer.BindForLightPass();
            //Light rendering
            GL.ClearColor(0,0,0,0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
            GL.DrawBuffers(1,_gBuffer.DrawBuffers);

            GL.CullFace(CullFaceMode.Front);

            _pointShader.Use();
            _worldRenderer.SetupCamera(_camera, _pointShader);
            _pointShader.SetVector2("ScreenSize", new Vector2(Width, Height));
            _pointShader.SetInt("PositionBuffer", _gBuffer.PositionTexture);
            _pointShader.SetInt("NormalBuffer", _gBuffer.NormalTexture);

            foreach (var pl in _worldRenderer.Lights)
            {
                _pointShader.SetVector3("LightCenter",new Vector3( 0, 2.5f, 0));
                _pointShader.SetFloat("LightRadius", 10);
                _pointShader.SetVector3("LightColor", new Vector3(1, 1, 1));
                _pointShader.SetFloat("LightIntensity", 1);
                _pointShader.SetMatrix4("model", pl.Transform.Model);
            }

            GL.Disable(EnableCap.Blend);
            _gBuffer.Restore();
        }

        private static void CalculateFPS()
        {
            float currentTime = (float)(_time);

            _framesPerSecond++;
            if ((currentTime - _lastTime) > 1.0f)
            {
                _lastTime = currentTime;

                FPS = _framesPerSecond;

                _framesPerSecond = 0;
            }
        }

        private void HandleKeyBoard()
        {
            Keyboard = KeyboardState;
            if (Keyboard.IsKeyDown(Keys.Escape))
            {
                this.Close();
            }
            if (Keyboard.IsKeyDown(Keys.M))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
            }
            if (Keyboard.IsKeyDown(Keys.Comma))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            if (Keyboard.IsKeyDown(Keys.Period))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
            if (Keyboard.IsKeyDown(Keys.Up))
            {
                foreach (var item in _lights.OfType<SpotLight>())
                {
                    item.LightData.Intensity += 1.0f;
                }
            }
            if (Keyboard.IsKeyDown(Keys.Down))
            {
                foreach (var item in _lights.OfType<SpotLight>())
                {
                    item.LightData.Intensity -= 1.0f;
                }

            }
            if (Keyboard.IsKeyDown(Keys.X))
            {
                ToggleFullscreen();
            }
            Shadows = Keyboard.IsKeyDown(Keys.B) ? 0 : 1;
            GammaEnable = Keyboard.IsKeyDown(Keys.G) ? 0 : 1;
           
            if (Keyboard.IsKeyDown(Keys.Left))
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Transform.Position += new Vector3(1, 0, 0);
                }

            }
            if (Keyboard.IsKeyDown(Keys.Right))
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Transform.Position -= new Vector3(1, 0, 0);
                }
            }

            if (Keyboard.IsKeyDown(Keys.LeftControl))
            {
                CursorGrabbed = false;
                _camera.CanMove = CursorGrabbed;
                CursorVisible = true;
            }
            if (Keyboard.IsKeyDown(Keys.LeftAlt))
            {
                CursorGrabbed = true;
                _camera.CanMove = CursorGrabbed;
                CursorVisible = false;
                MousePosition = new Vector2(Game.Width /2, Game.Height/2);
            }
            isGuiVisible = CursorVisible;
            
           

        }

        private void ToggleFullscreen()
        {
            if (IsFullscreen)
            {
                WindowBorder = WindowBorder.Resizable;
                WindowState = WindowState.Normal;
                _camera.AspectRatio = Size.X / (float)Size.Y;
            }
            else
            {
                WindowBorder = WindowBorder.Hidden;
                WindowState = WindowState.Fullscreen;
                _camera.AspectRatio = Size.X / (float)Size.Y;
            }
        }
        private Matrix4 CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition,
          Vector3 cameraUpVector, Vector3? cameraForwardVector)
        {
            Matrix4 result = Matrix4.Identity;

            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            vector.X = objectPosition.X - cameraPosition.X;
            vector.Y = objectPosition.Y - cameraPosition.Y;
            vector.Z = objectPosition.Z - cameraPosition.Z;
            float num = vector.LengthSquared;
            if (num < 0.0001f)
            {
                vector = cameraForwardVector.HasValue ? -cameraForwardVector.Value : new Vector3(0, 0, -1);
            }
            else
            {
                Vector3.Multiply(vector, (float)(1f / ((float)Math.Sqrt((double)num))), out vector);
            }
            Vector3.Cross(cameraUpVector,vector, out vector3);
            vector3.Normalize();
            Vector3.Cross(vector, vector3, out vector2);
            result.M11 = vector3.X;
            result.M12 = vector3.Y;
            result.M13 = vector3.Z;
            result.M14 = 0;
            result.M21 = vector2.X;
            result.M22 = vector2.Y;
            result.M23 = vector2.Z;
            result.M24 = 0;
            result.M31 = vector.X;
            result.M32 = vector.Y;
            result.M33 = vector.Z;
            result.M34 = 0;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1;

            return result;
        }
        void onDrawGUI()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open", "CTRL+O"))
                    {
                    }

                    ImGui.Separator();
                    if (ImGui.MenuItem("Save", "CTRL+S"))
                    {
                    }

                    if (ImGui.MenuItem("Save As", "CTRL+Shift+S"))
                    {
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Settings"))
                {
                    ImGui.Checkbox("Enabled", ref enabled);

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
            if (ImGui.Begin("GameObjects"))
            {
                if (ImGui.TreeNode("Objects"))
                {
                    foreach (var obj in _worldRenderer.GameObjects)
                    {
                        var item = obj as AMovable;
                        ImGui.PushID(obj.GetType().ToString() + _worldRenderer.GameObjects.IndexOf(obj));
                        if (ImGui.TreeNode(item.GetType().ToString()))
                        {
                            ImGui.Text("Transform");

                            Vector3 pos = item.Transform.Position;
                            var p = new System.Numerics.Vector3(pos.X, pos.Y, pos.Z);
                            ImGui.DragFloat3("Position", ref p, 0.1f);
                            item.Transform.Position = new Vector3(p.X, p.Y, p.Z);

                            Vector3 rotation = item.Transform.Rotation;
                            var r = new System.Numerics.Vector3(rotation.X, rotation.Y, rotation.Z);
                            ImGui.DragFloat3("Rotation", ref r, 0.1f);
                            item.Transform.Rotation = new Vector3(r.X, r.Y, r.Z);

                            Vector3 scale = item.Transform.Scale;
                            var s = new System.Numerics.Vector3(scale.X, scale.Y, scale.Z);
                            ImGui.DragFloat3("Scale", ref s, 0.1f);
                            item.Transform.Scale = new Vector3(s.X, s.Y, s.Z);
                        }

                        ImGui.PopID();
                    }
                    if (ImGui.TreeNode("Lights"))
                    {
                        foreach (var obj in _worldRenderer.Lights)
                        {
                            var item = obj; 
                            ImGui.PushID(obj.GetType().ToString() + _worldRenderer.Lights.IndexOf(obj));
                            if (ImGui.TreeNode(item.GetType().ToString()))
                            {
                                ImGui.Text("Transform");

                                Vector3 pos = item.Transform.Position;
                                var p = new System.Numerics.Vector3(pos.X, pos.Y, pos.Z);
                                ImGui.DragFloat3("Position", ref p, 0.1f);
                                item.Transform.Position = new Vector3(p.X, p.Y, p.Z);

                                Vector3 rotation = item.Transform.Rotation;
                                var r = new System.Numerics.Vector3(rotation.X, rotation.Y, rotation.Z);
                                ImGui.DragFloat3("Rotation", ref r, 0.1f);
                                item.Transform.Rotation = new Vector3(r.X, r.Y, r.Z);

                                Vector3 scale = item.Transform.Scale;
                                var s = new System.Numerics.Vector3(scale.X, scale.Y, scale.Z);
                                ImGui.DragFloat3("Scale", ref s, 0.1f);
                                item.Transform.Scale = new Vector3(s.X, s.Y, s.Z);

                                ImGui.Text("LightData");
                                float intens = item.LightData.Intensity;
                                ImGui.DragFloat("Intensity", ref intens);
                                item.LightData.Intensity = intens;

                                var near = item.NearPlane;
                                var far = item.FarPlane;

                                ImGui.DragFloat("Near", ref near, 1.0f, 1.0f, 100.0f);
                                ImGui.DragFloat("Far", ref far, 1.0f, 1.0f, 9999.0f);
                                item.NearPlane = near;
                                item.FarPlane = far;
                            }

                            ImGui.PopID();
                        }

                        ImGui.TreePop();
                    }
                }
            }
            ImGui.End();

            #endregion

          
        }
    }
}