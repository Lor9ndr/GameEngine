using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Physics
{
    public class CollisionMesh
    {
        public CollisionModel Model;
        public BoundingRegion BR;
        public List<Vector3> Points;
        public List<Face> faces;
        public CollisionMesh(int noPoints,float[] coordinates, int noFaces, int indices)
        {
            Vector3 min = Vector3.PositiveInfinity;
            Vector3 max = Vector3.NegativeInfinity;
            Points = new List<Vector3>();
            for (int i = 0; i < noPoints; i++)
            {
                Points.Add(new Vector3(coordinates[i*3 + 0], coordinates[i * 3 + 1], coordinates[i*3 + 2]));
                // Find new minimum
                if (Points[i].X < min.X)
                {
                    min.X = Points[i].X;
                }
                if (Points[i].Y < min.Y)
                {
                    min.Y = Points[i].Y;
                }
                if (Points[i].Z < min.Z)
                {
                    min.Z = Points[i].Z;
                }
                // Find new maximum
                if (Points[i].X > max.X)
                {
                    max.X = Points[i].X;
                }
                if (Points[i].Y > max.Y)
                {
                    max.Y = Points[i].Y;
                }
                if (Points[i].Z > max.Z)
                {
                    max.Z = Points[i].Z;
                }
            }
            Vector3 center = (min + max) / 2.0f;
            float maxRadiusSquared = 0.0f;
            for (int i = 0; i < noPoints; i++)
            {
                float radiusSquared = 0.0f;
                radiusSquared += (Points[i].X - center.X) * (Points[i].X - center.X);
                radiusSquared += (Points[i].Y - center.Y) * (Points[i].Y - center.Y);
                radiusSquared += (Points[i].Z - center.Z) * (Points[i].Z - center.Z);
                if (radiusSquared > maxRadiusSquared)
                {
                    maxRadiusSquared = radiusSquared;
                }
            }
            BR = new BoundingRegion(center, MathF.Sqrt(maxRadiusSquared));
        }



    }
}
