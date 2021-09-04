using GameEngine.Bases;
using GameEngine.Bases.Components;
using GameEngine.DefaultMeshes;
using GameEngine.Extensions;
using GameEngine.GameObjects;
using GameEngine.GameObjects.Lights;
using GameEngine.RenderPrepearings;
using GameEngine.RenderPrepearings.FrameBuffers;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Factories
{
    public static class LightFactory
    {
        private static Random _rd = new Random();

        #region Default Data

        private static Matrix4 _orthogonalProjection => Matrix4.CreateOrthographicOffCenter(-DirectLight.FarPlane, DirectLight.FarPlane, -DirectLight.FarPlane, DirectLight.FarPlane, DirectLight.NearPlane, DirectLight.FarPlane);
        private static Matrix4 _perspectiveProjection => Matrix4.CreatePerspectiveOffCenter(-SpotLight.FarPlane, SpotLight.FarPlane, -SpotLight.FarPlane, SpotLight.FarPlane, SpotLight.NearPlane, SpotLight.FarPlane);
        private static Matrix4 _perspectiveFOVProjection => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), Game.ShadowSize.X / Game.ShadowSize.Y, PointLight.NearPlane, PointLight.FarPlane);
        private static Texture _lightTexture = Texture.LoadFromFile("../../../Resources/Textures/blub.png", "diffuse", string.Empty);
        private static Mesh _cube => Cube.GetTexturedMesh(_lightTexture);
        private static LightData _directData => new LightData(ambient: new Vector3(0.6f), diffuse: new Vector3(1f), color: new Vector3(1f), specular: new Vector3(0.7f));
        private static LightData _pointData => new LightData(ambient: new Vector3(0.25f), diffuse: new Vector3(1f), color: new Vector3(1), specular: new Vector3(0.3f));
        private static LightData _pointRandomColorData => new LightData(ambient: new Vector3(0.25f), diffuse: new Vector3(1f), color: new Vector3(_rd.NextFloat(0f, 1f), _rd.NextFloat(0f, 1f), _rd.NextFloat(0f, 1f)), specular: new Vector3(0.3f));

        private static ShadowData _directShadowData => new ShadowData();
        private static ShadowData _spotShadowData => new ShadowData();
        private static ShadowData _pointShadowData => new ShadowData();

        private static readonly LightData _spotData = new LightData(diffuse: new Vector3(1), ambient: new Vector3(0), color: new Vector3(1f), specular: new Vector3(0.3f));
        #endregion

        #region Getters
        public static Light GetPointLight(Vector3 position) => new PointLight(_cube, _pointData,  new Transform(position));
        public static Light GetRandomColorPointLight(Vector3 position) => new PointLight(_cube, _pointRandomColorData,  new Transform(position));

        public static Light GetDirectLight(Vector3 position, Vector3 direction) => new DirectLight(_cube, _directData, new Transform(position, direction));

        public static Light GetSpotLight(Vector3 position, Vector3 direction) => new SpotLight(_spotData,new Transform(position, direction),cutOff: 0.9f,outerCutOff: 0.95f);
        #endregion
    }
}
