using Assimp;
using Assimp.Configs;
using Engine.Animations;
using Engine.Extensions;
using Engine.GLObjects.Textures;
using Engine.Rendering.Enums;
using Engine.Structs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mesh = Engine.Rendering.GameObjects.Mesh;

namespace Engine.GameObjects
{
    /// <summary>
    /// Класс модели состоит из множества мешей 
    /// </summary>
    public class Model : IDisposable
    {
        /// <summary>
        /// Словарь уже существующих текстур, где ключ - путь, значение - модель
        /// </summary>
        private static readonly Dictionary<string, Model> _models = new();

        /// <summary>
        /// Список загруженных текстур
        /// </summary>
        private static readonly List<Texture> _loadedTextures = new();

        private readonly string directory;
        private readonly string _path;

        /// <summary>
        /// Список мешей
        /// </summary>
        public List<Mesh> Meshes = new();

        /// <summary>
        /// Словарь костей, для анимаций
        /// </summary>
        private readonly Dictionary<string, BoneInfo> _boneInfoMap = new Dictionary<string, BoneInfo>();

        /// <summary>
        /// Кол-во костей
        /// </summary>
        private int _boneCounter = 0;

        /// <summary>
        /// Максимальный вес костей
        /// </summary>
        public const int MAX_BONE_WEIGHTS = 100;

        /// <summary>
        /// Геттер получения словаря костей
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, BoneInfo> GetBoneInfoMap() => _boneInfoMap;

        /// <summary>
        /// Геттер получения кол-во костей
        /// </summary>
        /// <returns></returns>
        public int GetBoneCount() => _boneCounter;

        /// <summary>
        /// Класс отвечающий за воспроизведение анимаций
        /// </summary>
        public Animator Animator;

        /// <summary>
        /// Список анимаций, 
        /// TODO: стоит возможно сделать статичным и загружать туда все анимации и воспроизводить из <see cref="Animator"/>
        /// </summary>
        public Dictionary<string, Animations.Animation> Animations;

        #region Constructor
        /// <summary>
        /// Конструктор модели по пути
        /// </summary>
        /// <param name="path">Путь, где находится модель</param>
        public Model(string path)
        {
            directory = path.Substring(0, path.LastIndexOf('/'));
            _path = path;
            // Если модель не существует в словаре моделей, то загружаем её
            if (!CheckModels(_path))
            {
                LoadModel();
                //GetOrSetThread();
            }
        }

        /// <summary>
        /// Создание модели из 1 меша
        /// </summary>
        /// <param name="mesh">Сам меш</param>
        /// <param name="name">Название модели, которое пойдет как путь модели</param>
        public Model(Mesh mesh, string name)
        {
            if (!CheckModels(name))
            {
                Meshes.Add(mesh);

                _path = name;
            }
        }

        /// <summary>
        /// Конструктор модели из 1 меша и списка текстур по имени
        /// </summary>
        /// <param name="mesh">Сам меш</param>
        /// <param name="textures">Список текстур</param>
        /// <param name="name">Наименование модели, пойдет как путь модели</param>
        public Model(Mesh mesh, List<Texture> textures, string name)
            : this(mesh, name)
        {
            if (!CheckModels(name))
            {
                Meshes.Add(mesh);

                _path = name;
                mesh.Textures.AddRange(textures);
            }
        }

        private void GetOrSetThread()
        {
            /*            if (ImportingTask != null)
                        {
                            ImportingQueue.Enqueue(LoadModel);
                        }
                        else
                        {
                            ImportingQueue.Enqueue(LoadModel);
                            ImportingTask = new Task(() =>
                            {
                                while (!_requestTermination.WaitOne(0))
                                {
                                    if (ImportingQueue.Count > 0)
                                    {
                                        var action = ImportingQueue.Dequeue();
                                        action.Invoke();
                                    }
                                }
                            });
                            ImportingTask.Start();
                        }*/
        }

        /// <summary>
        /// Возращает true, если модель уже была загружена
        /// </summary>
        /// <param name="path">путь, по которому ищем в словаре моделей </param>
        /// <returns></returns>
        private bool CheckModels(string path)
        {
            if (path == null)
            {
                return false;
            }
            if (_models.TryGetValue(path, out var m))
            {
                Meshes.AddRange(m.Meshes);
                return true;
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Функция отрисовки модели
        /// </summary>
        /// <param name="shader">Шейдер, куда отправляем модель</param>
        /// <param name="flags">Флаги отрисовки модели</param>
        /// <param name="target">Куда накладываем текстуры</param>
        /// <param name="type">Тип отрсиовки модели</param>
        public void Render(Shader shader, RenderFlags flags, TextureTarget target = TextureTarget.Texture2D, OpenTK.Graphics.OpenGL4.PrimitiveType type = OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles)
        {
            if (flags.HasFlag(RenderFlags.Animation))
            {
                if (Animator is not null && Animator.HasAnimation)
                {
                    Animator.Render(shader);
                }
            }
            if (flags.HasFlag(RenderFlags.Mesh))
            {
                foreach (var item in Meshes)
                {
                    item.Render(shader, flags, target, type);
                }
            }

        }

        #region Private Methods

        /// <summary>
        /// Устанавливаем на каждую вершину данные по костям по умолчанию
        /// </summary>
        /// <param name="v">Вершину, которую будем изменять</param>
        /// <returns></returns>
        private static Vertex SetVertexBoneDataToDefault(Vertex v)
        {
            for (int i = 0; i < Vertex.MAX_BONE_INFLUENCE; i++)
            {
                v.BoneIDs[i] = -1;
                v.Weights[i] = 0.0f;
            }
            return v;
        }

        /// <summary>
        /// Устанавливаем веса для костей для определенной вершины
        /// </summary>
        /// <param name="v">Вершина</param>
        /// <param name="boneID">Индекс кости</param>
        /// <param name="weight">Вес кости</param>
        /// <returns>Изменненная вершина</returns>
        private static Vertex SetVertexBoneData(Vertex v, int boneID, float weight)
        {
            for (int i = 0; i < Vertex.MAX_BONE_INFLUENCE; i++)
            {
                if (v.BoneIDs[i] < 0)
                {
                    v.Weights[i] = weight;
                    v.BoneIDs[i] = boneID;
                    return v;
                }
            }
            return v;
        }

        /// <summary>
        /// Извлекаем веса вершины и устанавливаем в соответствии с словарем костей
        /// </summary>
        /// <param name="vertices">список вершин</param>
        /// <param name="mesh">меш из загружаемой модели</param>
        private void ExtractBoneWeightForVertices(List<Vertex> vertices, Assimp.Mesh mesh)
        {
            for (int boneIndex = 0; boneIndex < mesh.BoneCount; boneIndex++)
            {
                string boneName = mesh.Bones[boneIndex].Name;
                int boneID;
                if (!_boneInfoMap.ContainsKey(boneName))
                {
                    BoneInfo newBoneInfo;

                    newBoneInfo.ID = _boneCounter;
                    newBoneInfo.offset = mesh.Bones[boneIndex].OffsetMatrix.GetMatrix4FromAssimpMatrix();
                    boneID = _boneCounter;
                    _boneCounter++;
                    _boneInfoMap.Add(boneName, newBoneInfo);
                }
                else
                {
                    boneID = _boneInfoMap[boneName].ID;
                }
                var weights = mesh.Bones[boneIndex].VertexWeights;
                int numWeights = mesh.Bones[boneIndex].VertexWeightCount;
                for (int weightIndex = 0; weightIndex < numWeights; ++weightIndex)
                {
                    int vertexId = weights[weightIndex].VertexID;
                    float weight = weights[weightIndex].Weight;
                    vertices[vertexId] = SetVertexBoneData(vertices[vertexId], boneID, weight);
                }
            }
        }


        #region ASSIMP METHODS
        private void ProcessNode(Node node, Scene scene)
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

        public void Dispose()
        {
            Console.WriteLine($"Disposing: {_path}");
            foreach (var item in Meshes)
            {
                item.Dispose();
            }
        }

        private Mesh ProcessMesh(Assimp.Mesh mesh, Scene scene)
        {
            List<Vertex> vertices = new();
            List<uint> indices = new();
            List<Texture> textures = new();
            for (int i = 0; i < mesh.VertexCount; i++)
            {

                Vertex v = new(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));
                v = SetVertexBoneDataToDefault(v);
                if (mesh.HasTextureCoords(0))
                    v.TexCoords = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);

                if (mesh.HasTangentBasis)
                {
                    v.Tangent = new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z);
                    v.Bitangent = new Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z);
                }

                if (mesh.HasNormals)
                    v.Normal = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);

                vertices.Add(v);
            }

            indices.AddRange(mesh.GetIndices().Select(s => Convert.ToUInt32(s)));
            Material material = scene.Materials[mesh.MaterialIndex];
            List<Texture> diffuseMaps = LoadMaterialTexture(material, Assimp.TextureType.Diffuse, Rendering.Enums.TextureType.Diffuse);
            textures.AddRange(diffuseMaps);

            List<Texture> specularMaps = LoadMaterialTexture(material, Assimp.TextureType.Specular, Rendering.Enums.TextureType.Specular);
            textures.AddRange(specularMaps);

            List<Texture> normalMaps = LoadMaterialTexture(material, Assimp.TextureType.Normals, Rendering.Enums.TextureType.Normal);
            textures.AddRange(normalMaps);
            List<Texture> heightMaps = LoadMaterialTexture(material, Assimp.TextureType.Height, Rendering.Enums.TextureType.Roughness);
            textures.AddRange(normalMaps);


            if (textures.Count == 0)
            {
                textures.AddRange(Texture.GetDefaultTextures);
            }
            ExtractBoneWeightForVertices(vertices, mesh);

            return new Mesh(vertices.ToArray(), indices.ToArray(), textures);

        }
        private List<Texture> LoadMaterialTexture(Material mat, Assimp.TextureType type, Rendering.Enums.TextureType typeName)
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
                    Meshes.Add(new Mesh(item.ObjectSetupper.GetVertices(), item.ObjectSetupper.GetIndices(), item.Textures, item.ObjectSetupper.GetVAOClass()));
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
                Scene scene = importer.ImportFile(_path, PostProcessPreset.TargetRealTimeMaximumQuality
                    | PostProcessSteps.Debone
                    | PostProcessSteps.FixInFacingNormals
                    | PostProcessSteps.FlipUVs
                    | PostProcessSteps.GenerateSmoothNormals
                    | PostProcessSteps.GenerateUVCoords
                    | PostProcessSteps.CalculateTangentSpace
                    | PostProcessSteps.Triangulate);
                Animations = new Dictionary<string, Animations.Animation>();

                ProcessNode(scene.RootNode, scene);
                _models.Add(_path, this);
                for (int i = 0; i < scene.AnimationCount; i++)
                {
                    Animations.Add(scene.Animations[i].Name.ToString(), new Animations.Animation(_path, this, i));
                }
                Animator = new Animator(Animations.FirstOrDefault().Value);
                logger.Detach();
            }
        }

        #endregion

        #endregion
    }
}
