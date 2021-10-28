using Assimp;
using Assimp.Configs;
using Engine.Components;
using Engine.GLObjects.Textures;
using Engine.Physics;
using Engine.Rendering.Enums;
using Engine.Rendering.GameObjects;
using Engine.Structs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using Mesh = Engine.Rendering.GameObjects.Mesh;

namespace Engine.GameObjects
{
    public class Model
    {
        private static readonly Dictionary<string, Model> _models = new();
        private static readonly List<Texture> _loadedTextures = new();
        private readonly string directory;
        private readonly string _path;

        public List<BoundingRegion> BoundingRegions;
        public List<RigidBody> RigidBodies;
        public List<Mesh> Meshes = new();
        #region Constructor
        public Model(string path)
        {
            directory = path.Substring(0, path.LastIndexOf('/'));
            _path = path;
            LoadModel();
        }
        public Model(Mesh mesh)
        {
            Meshes.Add(mesh);
            _models.Add(Guid.NewGuid().ToString(),this);

        }
        #endregion

        public void Render(Shader shader, RenderFlags flags, TextureTarget target = TextureTarget.Texture2D)
        {
            Meshes.ForEach(s=>s.Render(shader,flags, target));
        }
        #region Private Methods

        #region ASSIMP METHODS
        private void ProcessNode(Assimp.Node node, Scene scene)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
                Mesh m = ProcessMesh(mesh, scene);
                Meshes.Add(m);
            }
            for (int i = 0; i < node.ChildCount; i++)
            {
                ProcessNode(node.Children[i], scene);
            }
        }
        private Mesh ProcessMesh(Assimp.Mesh mesh, Scene scene)
        {
            List<Vertex> vertices = new();
            List<int> indices = new();
            List<Texture> textures = new();
            for (int i = 0; i < mesh.VertexCount; i++)
            {

                Vertex v = new()
                {
                    Position = new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z)
                };

                if (mesh.HasTextureCoords(0))
                {
                    v.TexCoords = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
                    if (mesh.Tangents.Count > 0)
                    {
                        v.Tangent = new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z);

                    }
                    if (mesh.BiTangents.Count > 0)
                    {
                        v.Bitangent = new Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].X);
                    }
                }
                else
                    v.TexCoords = new Vector2(0.0f, 0.0f);
                if (mesh.HasNormals)
                {
                    v.Normal = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                }

                vertices.Add(v);
            }
            indices.AddRange(mesh.GetIndices());
            Material material = scene.Materials[mesh.MaterialIndex];
            List<Texture> diffuseMaps = LoadMaterialTexture(material, TextureType.Diffuse, "texture_diffuse0");
            textures.AddRange(diffuseMaps);

            List<Texture> specularMaps = LoadMaterialTexture(material, TextureType.Specular, "texture_specular0");
            textures.AddRange(specularMaps);

            List<Texture> normalMaps = LoadMaterialTexture(material, TextureType.Height, "texture_normal0");
            textures.AddRange(normalMaps);

            List<Texture> heightMaps = LoadMaterialTexture(material, TextureType.Ambient, "texture_height0");
            textures.AddRange(heightMaps);
            if (textures.Count == 0)
            {
                textures.Add(Texture.LoadFromFile(Game.TEXTURES_PATH + "/checker.jpg", "texture_diffuse0", string.Empty));
            }
            using Mesh m = new(vertices.ToArray(), indices.ToArray(), textures);
            return m;
        }
        private List<Texture> LoadMaterialTexture(Material mat, TextureType type, string typeName)
        {
            List<Texture> textures = new();
            for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
            {
                mat.GetMaterialTexture(type, i, out TextureSlot str);
                bool skip = false;
                for (int j = 0; j < _loadedTextures.Count; j++)
                {
                    if (_loadedTextures[j].Path == str.FilePath)
                    {
                        textures.Add(_loadedTextures[j]);
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                {
                    if (str.FilePath != null)
                    {
                        Texture texture = Texture.LoadFromFile(str.FilePath, typeName, directory);
                        textures.Add(texture);
                        _loadedTextures.Add(texture);
                    }
                    
                }
            }

            return textures;
        }
        private void LoadModel()
        {
            if (_models.TryGetValue(_path, out var m))
            {
                foreach (var item in m.Meshes)
                {
                    Meshes.Add(new Mesh(item.ObjectSetupper.GetVertices, item.ObjectSetupper.GetIndices, item.Textures, item.ObjectSetupper.GetVAOClass()));
                }
            }
            else
            {
                using AssimpContext importer = new();
                if (!importer.IsImportFormatSupported(Path.GetExtension(_path)))
                {
                    throw new ArgumentException("Model format " + Path.GetExtension(_path) + " is not supported!  Cannot load {1}", "filename");
                }
                importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
                LogStream.IsVerboseLoggingEnabled = true;
                var logger = new ConsoleLogStream();
                logger.Attach();
                Scene scene = importer.ImportFile(_path, 
                    PostProcessSteps.FlipUVs | 
                    PostProcessSteps.GenerateUVCoords |
                    PostProcessSteps.OptimizeMeshes | 
                    PostProcessSteps.SplitLargeMeshes | 
                    PostProcessSteps.GenerateNormals  | 
                    PostProcessSteps.RemoveRedundantMaterials | 
                    PostProcessSteps.ImproveCacheLocality |
                    PostProcessSteps.FindDegenerates |
                    PostProcessSteps.FixInFacingNormals |
                    PostProcessSteps.CalculateTangentSpace
                    );
                logger.Detach();
                ProcessNode(scene.RootNode, scene);
                _models.Add(_path, this);
            }
        }
        #endregion

        #endregion
    }
}
