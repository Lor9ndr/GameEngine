using GameEngine.Bases;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.RenderPrepearings
{
    public class ShadowData : IComponent
    {
        public Matrix4 Projection;
        public float Bias;
        public bool FlipFaces;

        public ShadowData(Matrix4 projection, float bias, bool flipFaces)
        {
            Projection = projection;
            Bias = bias;
            FlipFaces = flipFaces;
        }
    }
}
