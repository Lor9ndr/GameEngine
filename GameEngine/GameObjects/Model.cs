using Assimp;
using Assimp.Configs;
using GameEngine.Enums;
using GameEngine.GameObjects.Base;
using GameEngine.Intefaces;
using GameEngine.Structs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameObjects
{
    public class Model : AMovable, IRenderable
    {
        private static readonly Dictionary<string, Model> _models = new();
        private static readonly Dictionary<string, Mesh> _overAllMeshes = new();
        private List<Mesh> _meshes = new();
        private static readonly List<Texture> _loadedTextures = new();
        private readonly string directory;
        private readonly string _path;
        private bool _reverseNormals = false;
        private bool _enableCullFace;

        public Model(string path, Vector3 position, Vector3 direction, Vector3 rotation, Vector3 scale, float velocity,bool reverseNormals = false,bool enableCullFace=true, Matrix4 model = default)
            : base(position, direction, rotation, scale, velocity, model)
        {
            directory = path.Substring(0, path.LastIndexOf('/'));
            Position = position;
            _path = path;
            _reverseNormals = reverseNormals;
            _enableCullFace = enableCullFace;
            LoadModel();
        }

        public void Render(Shader shader)
        {

            shader.Use();
            var t2 = Matrix4.CreateTranslation(Position);
            var r1 = Matrix4.CreateRotationX(Rotation.X);
            var r2 = Matrix4.CreateRotationY(Rotation.Y);
            var r3 = Matrix4.CreateRotationZ(Rotation.Z);
            var s = Matrix4.CreateScale(Scale);
            _model = r1 * r2 * r3 * s * t2;
            shader.SetMatrix4("model", _model);
            if (_reverseNormals)
            {
                shader.SetInt("reverse_normals", 1);
            }
           
            foreach (var item in _meshes)
            {
                item.Render(shader);
            }
            shader.SetInt("reverse_normals", 0);
        }

        public void LoadModel()
        {
            if (_models.TryGetValue(_path, out var m))
            {
                foreach (var item in m._meshes)
                {
                    _meshes.Add(new Mesh(item.Vertices, item.Indices, item.Textures, item.ObjectSetupper.VAO));
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
                   PostProcessSteps.Triangulate |
                    PostProcessSteps.GenerateUVCoords |
                    PostProcessSteps.GenerateNormals |
                    PostProcessSteps.FlipUVs |
                    PostProcessSteps.FindDegenerates |
                    PostProcessSteps.FixInFacingNormals|
                    PostProcessSteps.GenerateSmoothNormals |
                    PostProcessSteps.OptimizeMeshes
                    );
                logger.Detach();
                ProcessNode(scene.RootNode, scene);
                _models.Add(_path, this);
            }
        }

        private void ProcessNode(Node node, Scene scene)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
                Mesh m = ProcessMesh(mesh, scene);
                _meshes.Add(m);
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
            List<Texture> diffuseMaps = LoadMaterialTexture(material, TextureType.Diffuse, "texture_diffuse");
            textures.AddRange(diffuseMaps);

            List<Texture> specularMaps = LoadMaterialTexture(material, TextureType.Specular, "texture_specular");
            textures.AddRange(specularMaps);

            List<Texture> normalMaps = LoadMaterialTexture(material, TextureType.Height, "texture_normal");
            textures.AddRange(normalMaps);

            List<Texture> heightMaps = LoadMaterialTexture(material, TextureType.Ambient, "texture_height");
            textures.AddRange(heightMaps);
            if (textures.Count == 0)
            {
                textures.Add(Texture.LoadFromFile("../../../Resources/Textures/checker.jpg", "texture_diffuse", string.Empty));
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
                    Texture texture = Texture.LoadFromFile(str.FilePath, typeName, directory);
                    textures.Add(texture);
                    _loadedTextures.Add(texture);
                }
            }

            return textures;
        }

        public void Dispose()
        {
            foreach (var item in _meshes)
            {
                item.Dispose();
            }
        }

       
    }
}
