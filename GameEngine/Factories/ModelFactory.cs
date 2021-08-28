using GameEngine.Bases;
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
    }
}
