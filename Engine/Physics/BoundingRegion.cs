using Engine.Components;
using GameEngine.Enums;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Physics
{
    public class BoundingRegion
    {
        public BoundTypes Type;

        public RigidBody Instance;
        public CollisionMesh CollisionMesh;

        // pointer for quick access to current octree node
        public Node Node;

        // sphere values
        public Vector3 Center;
        public float Radius;

        public Vector3 OgCenter;
        float ogRadius;

        // bounding box values
        public Vector3 Min;
        public Vector3 Max;

        public Vector3 OgMin;
        public Vector3 OgMax;
        public BoundingRegion(BoundTypes type)
        {
            Type = type;
        }
        public BoundingRegion(Vector3 center, float radius)
        {
            Center = center;
            OgCenter = center;
            Radius = radius;
            ogRadius = radius;
            Type = BoundTypes.Sphere;
        }
        public BoundingRegion(Vector3 min, Vector3 max)
        {
            Min = min;
            OgMin = min;
            Max = max;
            OgMax = max;
            Type = BoundTypes.AABB;
        }
        public void Transform()
        {
            if (Instance != null)
            {
                if (Type == BoundTypes.AABB)
                {
                    Min = (OgMin * Instance.Size) + Instance.Transform.Position;
                    Max = (OgMax * Instance.Size) + Instance.Transform.Position;
                }
                else
                {
                    Center = OgCenter * Instance.Size + Instance.Transform.Position;
                    float maxDim = Instance.Size.X;

                    for (int i = 0; i < 3; i++)
                    {
                        if (Instance.Size[i] > maxDim)
                        {
                            maxDim = Instance.Size[i];
                        }
                    }
                   
                    Radius = ogRadius * maxDim;
                }
            }
        }
        public Vector3 CalculateCenter() => (Type == BoundTypes.AABB) ? (Min + Max) / 2.0f : Center;
        public Vector3 CalculateDimensions() => (Type == BoundTypes.AABB) ? (Max - Min) : new Vector3(Radius * 2.0f);

        public bool ContainsRegion(BoundingRegion br)
        {
            if (br.Type == BoundTypes.AABB)
            {
                return ContainsPoint(br.Min) && ContainsPoint(br.Max);
            }
            else if (Type == BoundTypes.Sphere && br.Type == BoundTypes.Sphere)
            {
                return (Center - br.Center).Length + br.Radius < Radius;
            }
            else
            {
                if (!ContainsPoint(br.Center))
                {
                    return false;
                }
                // center inside the box
                /*
                    for each axis (x, y, z)
                    - if distance to each side is smaller than the radius, return false
                */
                for (int i = 0; i < 3; i++)
                {
                    if (MathF.Abs(Max[i] - br.Center[i]) < br.Radius || 
                        MathF.Abs(br.Center[i] - Min[i]) < br.Radius)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool IntresectsWith(BoundingRegion br)
        {
            if (Type == BoundTypes.AABB && br.Type == BoundTypes.AABB)
            {
                Vector3 rad =CalculateDimensions() / 2.0f;
                Vector3 radBr = br.CalculateDimensions() / 2.0f;

                Vector3 center = CalculateCenter();
                Vector3 centerBr = br.CalculateCenter();
                Vector3 dist = new Vector3();
                for (int i = 0; i < 3; i++)
                {
                    dist[i] = MathF.Abs(center[i] - centerBr[i]);
                }
                for (int i = 0; i < 3; i++)
                {
                    if (dist[i] > rad[i] + radBr[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (Type == BoundTypes.Sphere && br.Type == BoundTypes.Sphere)
            {
                Vector3 centerDiff = Center - br.Center;
                float distSquared = 0.0f;
                for (int i = 0; i < 3; i++)
                {
                    distSquared += centerDiff[i] * centerDiff[i];
                }
                float maxMagSquared = Radius + br.Radius;
                return distSquared <= maxMagSquared;
            }
            else if (Type == BoundTypes.Sphere)
            {
                float distSquared = 0.0f;
                for (int i = 0; i < 3; i++)
                {
                    float closestPT = MathF.Max(br.Min[i], MathF.Min(Center[i], br.Max[i]));
                    distSquared += (closestPT - Center[i]) * (closestPT - Center[i]);
                }
                return distSquared < (Radius * Radius);
            }
            else
            {
                // this is a box, br is a sphere
                // call algorithm for br (defined in preceding else if block)
                return br.IntresectsWith(this);
            }
        }
        public bool ContainsPoint(Vector3 point)
        {
            if (Type == BoundTypes.AABB)
            {
                return 
                    (point.X >= Min.X) && (point.X <= Max.Z) &&
                    (point.Y >= Min.Y) && (point.Y <= Max.Z) &&
                    (point.Z >= Min.Z) && (point.Z <= Max.Z);
            }
            else
            {
                // sphere - distance must be less than radius
                // x^2 + y^2 + z^2 <= r^2
                float distSquared = 0.0f;
                for (int i = 0; i < 3; i++)
                {
                    distSquared += (Center[i] - point[i]) * (Center[i] - point[i]);
                }
                return distSquared <= (Radius*Radius);
            }
        }
    }
}
