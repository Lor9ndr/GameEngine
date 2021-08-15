using GameEngine.DefaultMeshes;
using GameEngine.Extensions;
using GameEngine.GameObjects;
using GameEngine.GameObjects.Lights;
using GameEngine.Intefaces;
using GameEngine.RenderPrepearings.FrameBuffers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class Game : GameWindow
    {
        #region Constructors

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) 
            : base(gameWindowSettings, nativeWindowSettings)
        { }
        #endregion

        #region Public Properties

        public static float FPS { get; private set; }
        public static float DeltaTime => _deltaTime;
        public static double Time => _time;

        public KeyboardState Keyboard { get; private set; }
        #endregion

        #region Private Properties
        private double oldTimeSinceStart = 0;
        private static float _framesPerSecond = 0.0f;
        private static float _lastTime = 0.0f;
        private static double _time;
        private static float _deltaTime;
        private Camera _camera;
        private List<IRenderable> _models = new List<IRenderable>();
        private List<Light> _lights = new List<Light>();
        private WorldRenderer _worldRenderer;

        private Shader SimpleShader;
        private Shader LightBoxShader;
        private DeferredShading _ds;


        private static Texture _lightTexture = Texture.LoadFromFile("../../../Resources/Textures/blub.png", "texture_diffuse", string.Empty);

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

        public const string SHADER_PATH = "../../../Shaders/";
        public const string SIMPLE_SHADER = SHADER_PATH +"SimpleShader";
        public const string DEFERRED_RENDER_PATH = SHADER_PATH + "DeferredShading/";
        public const string ANOTHER_SHADER_PATH = DEFERRED_RENDER_PATH + "Another/";
        public const string SKYBOX_SHADER_PATH = SHADER_PATH + "SkyBox/SkyBox";

        public const int WIDTH = 1920;
        public const int HEIGHT = 1080;
        #endregion

        #region Overrides
        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _camera = new Camera(new Vector3(0, 0, 3), Size.X / (float)Size.Y);
            SimpleShader = new Shader(SIMPLE_SHADER + ".vs", SIMPLE_SHADER + ".fr");
            LightBoxShader = new Shader(DEFERRED_RENDER_PATH + "LightBoxV.glsl", DEFERRED_RENDER_PATH + "LightBoxfr.glsl");
            Random rd = new();

            #region Models
            _models.Add(new Model
               (MAN_PATH
               ,
               new Vector3(0),
               new Vector3(rd.Next(-100, 100)),
               new Vector3(rd.Next(-10, 10)),
               new Vector3(0.5f), 0));
            for (int i = 0; i < 10; i++)
            {
                var pos = new Vector3(rd.Next(-100, 10), rd.Next(-70, 10) * (float)_time, rd.Next(-50, 70));
                _models.Add(new Model
                (NANOSUIT_PATH
                ,
                pos,
                new Vector3(rd.Next(-100, 100)),
                new Vector3(rd.Next(-10, 10)),
                new Vector3(0.5f), 0));

            }

            #endregion

            #region Lights
            DirectLight directLight = new DirectLight(Cube.GetMesh()
                , position: new Vector3(0, 100, 0)
                , ambient: new Vector3(0.6f)
                , diffuse: new Vector3(1f)
                , lightColor: new Vector3(1)
                , direction: new Vector3(0)
                , scale: new Vector3(1));
            SpotLight sl = new SpotLight(_camera.Position
                            , ambient: new Vector3(0.0f, 0.0f, 0.0f)
                            , diffuse: new Vector3(1.0f, 1.0f, 1.0f)
                            , lightColor: new Vector3(1)
                            , scale: new Vector3(1));
            _lights.Add(sl);
            _lights.Add(directLight);

            for (int i = 0; i < 32; i++)
            {
                var position = new Vector3(rd.Next(-30,15), rd.Next(-20, 10), rd.Next(-30, -30));
                var resultX = rd.NextFloat(0, 1);
                var resultY = rd.NextFloat(0, 1);
                var resultZ = rd.NextFloat(0, 1);
                _lights.Add
                    (new PointLight
                        (
                            Cube.GetMesh()
                            , position
                            , ambient: new Vector3(0.05f)
                            , diffuse: new Vector3(0.8f)
                            , lightColor: new Vector3(resultX, resultY, resultZ)
                            , scale: new Vector3(1)
                        )
                    ); ;
            }
            var ter = new Model(TERRAIN_PATH, new Vector3(0), new Vector3(0), new Vector3(0), new Vector3(0.05f),0);
            #endregion
            _models.Add(ter);
            _worldRenderer = new WorldRenderer(_models, _lights);
            CursorGrabbed = true;
            base.OnLoad();
            _camera.Enable(this);
            _ds = new DeferredShading(_worldRenderer);
            _ds.Setup();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.CullFace);

            _worldRenderer.Render(_camera, SimpleShader);
            _worldRenderer.RenderLights(_camera, LightBoxShader, true);
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            CalculateFPS();
            Title =
                $"FPS: {FPS},"+
            $" Objects: {_models.Count}," +
            $" Lights: {_lights.Count}, " +
            $"CAMERAPOS: {_camera.Position}";
            _worldRenderer.Update();
            foreach (var item in _lights)
            {
                item.Position.Z += MathF.Sin(DeltaTime) * 0.5f;
            }
            Random rd = new Random();
          
            _time += args.Time;
            _deltaTime = (float)_time - (float)oldTimeSinceStart;
            HandleKeyBoard();
            base.OnUpdateFrame(args);
            oldTimeSinceStart = _time;
        }

        #endregion

        #region Private Methods

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
                foreach (var item in _worldRenderer.Lights)
                {
                    item.SetAmbient(new Vector3(0.01f));
                }
            }
            if (Keyboard.IsKeyDown(Keys.Down))
            {
                foreach (var item in _worldRenderer.Lights)
                {
                    item.SetAmbient(new Vector3(-0.01f));
                }
            }
        }


        #endregion
    }
}
/*            _gBuffer.BindForGeometryPass();
            GL.Enable(EnableCap.DepthTest);

            GL.CullFace(CullFaceMode.Back);

            GL.ClearColor(0, 0, 0, 1);
            GL.ClearDepth(1.0f);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GeomShader.Use();
            GeomShader.SetMatrix4("view", _camera.GetViewMatrix());
            GeomShader.SetVector3("AmbientColor", new Vector3(Color4.White.R, Color4.White.G, Color4.White.B));
            GeomShader.SetFloat("AmbientPower", ((float)MathF.Sin(1 * 6) + 1) / 2f);
            GeomShader.SetVector3("AmbientDirection", Vector3.One);
            _worldRenderer.RenderObjects(_camera, GeomShader);
            GL.Disable(EnableCap.DepthTest);

            //Light blub icon rendering



            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            GL.CullFace(CullFaceMode.Front);
            _gBuffer.BindForLightPass();
            PointLightShader.Use();
            PointLightShader.SetVector2("ScreenSize", new Vector2(WIDTH, HEIGHT));
            PointLightShader.SetInt("PositionBuffer", _gBuffer.PositionTexture);
            PointLightShader.SetInt("NormalBuffer", _gBuffer.NormalTexture);
            PointLightShader.SetFloat("LightRadius", 60);
            PointLightShader.SetVector3("LightCenter", new Vector3(0));
            PointLightShader.SetFloat("LightIntensity", 2);

            _worldRenderer.RenderLights(_camera, PointLightShader);
            GL.Disable(EnableCap.Blend);
            _gBuffer.Restore();

            if (Keyboard.IsKeyDown(Keys.F1))
            {
                _gBuffer.DumpToScreen(WIDTH, HEIGHT);
            }
            else
            {
                GL.ClearColor(Color4.CornflowerBlue);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.CullFace(CullFaceMode.Back);
                FinalCombineShader.Use();
                FinalCombineShader.SetInt("ColorBuffer", _gBuffer.DiffuseTexture);
                FinalCombineShader.SetInt("LightBuffer", _gBuffer.LightTexture);
                _fsQuad.Draw(PrimitiveType.Triangles);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.CullFace(CullFaceMode.Front);

                FlatShader.Use();
                _worldRenderer.RenderLights(_camera, FlatShader);

                GL.Disable(EnableCap.Blend);

                //_gBuffer.BlitResult(WIDTH, HEIGHT);
            }*/