using Engine.Components;
using Engine.GameObjects;
using OpenTK.Mathematics;

namespace Engine.Rendering.Factories
{
    public static class ModelFactory
    {
        public static Model GetNanoSuitModel() => new(Game.NANOSUIT_PATH);
        public static Model GetBridgeModel() => new(Game.BRIDGE_PATH);
        public static Model GetManModel() => new(Game.MAN_PATH);
        public static Model GetTerrainModel() => new(Game.TERRAIN_PATH);
        //public static Model GetFloorModel(Vector3 position) => new(Game.FLOOR_PATH, new Transform(position, new Vector3(0), new Vector3(0), new Vector3(0.05f), new Vector3(0)));
        public static Model GetCostumeModel() => new(Game.OBJ_PATH + "PBR/tv.obj");
        public static Model GetM4A1() => new(Game.OBJ_PATH + "m4a1/scene.gltf");
        public static Model GetLotr() => new(Game.OBJ_PATH + "lotr_troll/scene.gltf");
    }
}
