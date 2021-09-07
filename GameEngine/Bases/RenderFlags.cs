using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Bases
{
    [Flags] public enum RenderFlags
    {
        None = 0,
        Mesh = 1,
        Textures = 2,
        ReverseNormals = 4
    }
}
