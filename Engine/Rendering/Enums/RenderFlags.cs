using System;

namespace Engine.Rendering.Enums
{
    [Flags]
    public enum RenderFlags
    {
        None = 0,
        Mesh = 1,
        Textures = 2,
        ReverseNormals = 4,
        LightData = 8,
        ProcessShadow = 16,
        Animation = 32,
        MeshAndAnimations = Mesh | Animation,
        MeshAndTextures = Mesh | Textures,
        MeshTexturesAnimation = Mesh | Textures | Animation,
    }
}
