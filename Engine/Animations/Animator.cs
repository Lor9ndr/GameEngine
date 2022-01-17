using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.Animations
{
    public class Animator
    {
        private readonly List<Matrix4> _finalBoneMatrices;
        private Animation _currentAnimation;
        private float _currentTime;
        private float _deltaTime;
        public bool HasAnimation => _currentAnimation is not null;
        public Animator(Animation animation)
        {
            _currentTime = 0.0f;
            _currentAnimation = animation;
            _finalBoneMatrices = new List<Matrix4>();
            for (int i = 0; i < 100; i++)
            {
                _finalBoneMatrices.Add(Matrix4.Identity);
            }
        }
        public void UpdateAnimation(float dt)
        {
            _deltaTime = dt;
            if (_currentAnimation is not null)
            {
                _currentTime += _currentAnimation.GetTicksPerSecond() * dt;
                _currentTime = _currentTime % _currentAnimation.GetDuration();
            }
        }
        public async Task FixedUpdate()
        {
            if (_currentAnimation is not null)
            {
                await CalculateBoneTransform(_currentAnimation.GetRootNode(), Matrix4.Identity);
            }
        }
        public void PlayAnimation(Animation animation)
        {
            _currentAnimation = animation;
            _currentTime = 0.0f;
        }

        public async Task CalculateBoneTransform(AssimpNodeData node, Matrix4 parentTranform)
        {
            string nodeName = node.Name;
            if (nodeName is null)
            {
                return;
            }
            Matrix4 nodeTransform = node.Transformation;
            Bone bone = _currentAnimation.FindBone(nodeName);
            if (bone is not null)
            {
                bone.Update(_currentTime);
                nodeTransform = bone.GetLocalTransform();
            }

            Matrix4 globalTransformation = nodeTransform * parentTranform;
            var boneInfoMap = _currentAnimation.GetBoneInfoMap();
            if (boneInfoMap.TryGetValue(nodeName, out BoneInfo boneInfo))
            {
                int index = boneInfo.ID;
                Matrix4 offset = boneInfo.offset;
                _finalBoneMatrices[index] = offset * globalTransformation;
            }
            for (int i = 0; i < node.ChildrenCount; i++)
            {
                await CalculateBoneTransform(node.children[i], globalTransformation);
            }
        }

        public void Render(Shader shader)
        {
            Game.EngineGL.UseShader(shader);
            for (int i = 0; i < _finalBoneMatrices.Count; i++)
            {
                Game.EngineGL.SetShaderData($"finalBonesMatrices[{i}]", _finalBoneMatrices[i]);
            }
        }
        public List<Matrix4> GetFinalBoneMatrices() => _finalBoneMatrices;
    }
}
