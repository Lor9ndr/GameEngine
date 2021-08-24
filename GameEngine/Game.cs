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
        private readonly List<IRenderable> _models = new();
        private List<Light> _lights = new();
        private WorldRenderer _worldRenderer;

        private Shader SimpleShader;
        private Shader LightBoxShader;
        private DeferredShading _ds;

        private ShadowFrameBuffer _shadowFB;

        private static Texture _lightTexture = Texture.LoadFromFile("../../../Resources/Textures/blub.png", "texture_diffuse", string.Empty);
        private DirectLight _sun;
        internal static int Shadows = 1;

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
        public const string SIMPLE_SHADER = SHADERS_PATH +"SimpleShader";
        public const string DEFERRED_RENDER_PATH = SHADERS_PATH + "DeferredShading/";
        public const string ANOTHER_SHADERS_PATH = DEFERRED_RENDER_PATH + "Another/";
        public const string SKYBOX_SHADER_PATH = SHADERS_PATH + "SkyBox/SkyBox";
        public const string SHADOW_SHADERS_PATH = SHADERS_PATH + "ShadowShaders/";
        public const string DIRECT_SHADOW_SHADERS_PATH = SHADOW_SHADERS_PATH + "DirectShadows/";
        public const string POINT_SHADOW_SHADERS_PATH = SHADOW_SHADERS_PATH + "PointShadows/";

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
               new Vector3(1f), 0));
            for (int i = 0; i < 10; i++)
            {
                var pos = new Vector3(rd.Next(-8, 8), rd.Next(0), rd.Next(-15,16));
                _models.Add(new Model
                (NANOSUIT_PATH,
                pos,
                new Vector3(rd.Next(-100, 100)),
                new Vector3(rd.Next(-100, 100)),
                new Vector3(0.5f), 0));

            }

            #endregion

            #region Lights
            _sun = new DirectLight(Cube.GetMesh()
                , position: new Vector3(-1, 7, 1.3f)
                , ambient: new Vector3(0.6f)
                , diffuse: new Vector3(1f)
                , lightColor: new Vector3(0.6f)
                , direction: new Vector3(0)
                , specular: new Vector3(1.0f)
                , scale: new Vector3(1));
            SpotLight sl = new SpotLight(_camera.Position
                            , ambient: new Vector3(0.0f, 0.0f, 0.0f)
                            , diffuse: new Vector3(1.0f, 1.0f, 1.0f)
                            , lightColor: new Vector3(1)
                            , specular: new Vector3(0.3f)
                            , outerCutOff: 0.90f
                            , cutOff: 0.90f
                            , scale: new Vector3(1));
            _lights.Add(sl);
            _lights.Add(_sun);

            for (int i = 0; i < 1; i++)
            {
                var position = new Vector3(rd.Next(-5,4), rd.Next(3, 10), rd.Next(-5, 4));
                var resultX = rd.NextFloat(0, 1);
                var resultY = rd.NextFloat(0, 1);
                var resultZ = rd.NextFloat(0, 1);
                _lights.Add
                    (new PointLight
                        (
                            Cube.GetMesh()
                            , position
                            , ambient: new Vector3(1f)
                            , diffuse: new Vector3(1f)
                            , lightColor: new Vector3(resultX, resultY, resultZ)
                            , scale: new Vector3(1)
                            , specular: new Vector3(0.3f)
                        )
                    );
            }
            var ter = new Model(TERRAIN_PATH, new Vector3(0), new Vector3(0), new Vector3(0), new Vector3(0.05f),0, reverseNormals:false);
            #endregion
            _models.Add(ter);
            _worldRenderer = new WorldRenderer(_models, _lights);
            CursorGrabbed = true;
            base.OnLoad();
            _camera.Enable(this);
            _shadowFB = new ShadowFrameBuffer(_worldRenderer);
            _shadowFB.Setup();
            /*_directShadows = new DirectShadows(_worldRenderer);
            _directShadows.Setup();*/
            /* _pointShadows = new PointShadows(_worldRenderer);
             _pointShadows.Setup();*/
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            /*            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                        GL.Enable(EnableCap.CullFace);*/
            //_worldRenderer.Render(_camera, SimpleShader);
            //_directShadows.RenderBuffer(_camera);
            _shadowFB.RenderBuffer(_camera);
#if DEBUG
            _worldRenderer.RenderLights(_camera, LightBoxShader, true);
#endif
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            CalculateFPS();
            Title =
                $"FPS: {FPS},"+
            $" Objects: {_models.Count}," +
            $" Lights: {_lights.Count}, " +
            $"CAMERAPOS: {_camera.Position}" +
            $"Shadows: {Shadows}";
            _worldRenderer.Update();
            foreach (var item in _lights.OfType<DirectLight>())
            {
                item.Position.X = (float)(10 * Math.Sin(Time));
                item.Position.Z = (float)(10 * Math.Cos(Time));
            }
            _shadowFB.LightShader.SetInt("shadows", Shadows);


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
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Position += new Vector3(0, 0.1f, 0);
                }

            }
            Shadows = Keyboard.IsKeyDown(Keys.B)? 0 : 1;
            if (Keyboard.IsKeyDown(Keys.Down))
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Position -= new Vector3(0, 0.1f, 0);
                }


            }
            if (Keyboard.IsKeyDown(Keys.Left))
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Position += new Vector3(1, 0, 0);
                }

            }
            if (Keyboard.IsKeyDown(Keys.Right))
            {
                foreach (var item in _lights.OfType<PointLight>())
                {
                    item.Position -= new Vector3(1,0,0);
                }

            }

        }


        #endregion
    }
}