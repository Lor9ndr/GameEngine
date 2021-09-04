using GameEngine.Bases;
using GameEngine.Bases.Components;
using GameEngine.GameObjects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Factories
{
    public static class ModelFactory
    {
        public static Model GetNanoSuitModel(Vector3 position) => new(Game.NANOSUIT_PATH, new Transform(position, new Vector3(0), new Vector3(0), new Vector3(0.5f), 0),false);
        public static Model GetBridgeModel(Vector3 position) => new(Game.BRIDGE_PATH, new Transform(position, new Vector3(0), new Vector3(0), new Vector3(1f), 0),false);
        public static Model GetManModel(Vector3 position) => new(Game.MAN_PATH, new Transform(position, new Vector3(0), new Vector3(0), new Vector3(0.5f), 0),false);
        public static Model GetTerrainModel(Vector3 position) => new(Game.TERRAIN_PATH, new Transform(position, new Vector3(0), new Vector3(0), new Vector3(0.05f), 0),false);
        public static Model GetCostumeModel(Vector3 position) => new(Game.OBJ_PATH + "PBR/tv.obj", new Transform(position, new Vector3(0), new Vector3(0), new Vector3(2), 0), false);
        public static Model GetM4A1(Vector3 position) => new(Game.OBJ_PATH + "m4a1/scene.gltf", new Transform(position, new Vector3(0), new Vector3(0), new Vector3(2), 0), false);
        public static Model GetLotr(Vector3 position) => new(Game.OBJ_PATH + "lotr_troll/scene.gltf", new Transform(position, new Vector3(0), new Vector3(0), new Vector3(2), 0), false);
    }
}
