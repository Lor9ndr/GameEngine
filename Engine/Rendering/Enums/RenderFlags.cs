using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Rendering.Enums
{
    [Flags] public enum RenderFlags
    {
        None = 0,
        Mesh = 1,
        Textures = 2,
        ReverseNormals = 4,
        MeshAndTextures = Mesh | Textures,
    }
}
