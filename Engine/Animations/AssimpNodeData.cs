using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Engine.Animations
{
    public struct AssimpNodeData
    {
        public Matrix4 Transformation;
        public string Name;
        public int ChildrenCount;
        public List<AssimpNodeData> children;
    }
}
