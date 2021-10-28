using Engine.Components;
using Engine.GameObjects;
using GameEngine.Enums;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Engine.Physics
{
    public class Node
    {
        public const int NUMBER_CHILDREN = 8;
        public const float MIN_BOUNDS = 0.5F;
        public Node Parent;
        public Node[] Childrens = new Node[NUMBER_CHILDREN];
        public char ActiveOctants;
        public bool HasChildren = false;
        public bool treeReady = false;
        public bool treeBuilt = false;
        public short MaxLifeSpan = 8;
        public short CurrentLifeSpan = -1;

        public List<BoundingRegion> Objects;
        public Queue<BoundingRegion> Queue;
        public BoundingRegion Region;
        public Node()
        {

        }
        public Node(BoundingRegion bounds)
        {

        }
        public Node(BoundingRegion bounds, List<BoundingRegion> objectList)
        {

        }

        public BoundingRegion CalculateBounds(out BoundingRegion brOut, Octant octant, BoundingRegion parentRegion)
        {
            Vector3 center = parentRegion.CalculateCenter();
            switch (octant)
            {
                case Octant.O1:
                    brOut = new BoundingRegion(center, parentRegion.Max);
                    break;
                case Octant.O2:
                    brOut = new BoundingRegion(new Vector3(parentRegion.Min.X, center.Y, center.Z), new Vector3(center.X, parentRegion.Max.Y, parentRegion.Max.Z));
                    break;
                case Octant.O3:
                    brOut = new BoundingRegion( new Vector3(parentRegion.Min.X, parentRegion.Min.Y, center.Z), new Vector3(center.X, center.Y, parentRegion.Max.Z));
                    break;
                case Octant.O4:
                    brOut = new BoundingRegion( new Vector3(center.X, parentRegion.Min.Y, center.Z), new Vector3(parentRegion.Max.X, center.Y, parentRegion.Max.Z));
                    break;
                case Octant.O5:
                    brOut = new BoundingRegion( new Vector3(center.X, center.Y, parentRegion.Min.Z), new Vector3(parentRegion.Max.X, parentRegion.Max.Y, center.Z));
                    break;
                case Octant.O6:
                    brOut = new BoundingRegion( new Vector3(parentRegion.Min.X, center.Y, parentRegion.Min.Z), new Vector3(center.X, parentRegion.Max.Y, center.Z));
                    break;
                case Octant.O7:
                    brOut = new BoundingRegion(parentRegion.Min, center);
                    break;
                case Octant.O8:
                    brOut = new BoundingRegion( new Vector3(center.X, parentRegion.Min.Y, parentRegion.Min.Z), new Vector3(parentRegion.Max.X, center.Y, center.Z));
                    break;
                default:
                    brOut = null;
                    break;
            }
            return brOut;

        }
        public void AddToPending(RigidBody instance, Model model)
        {
            foreach (var br in model.BoundingRegions)
            {
                br.Instance = instance;
                br.Transform();
                Queue.Enqueue(br);
            }
        }

        // build tree (called during initialization)
        public void Build()
        {
            BoundingRegion[] octants = new BoundingRegion[NUMBER_CHILDREN];
            Vector3 dimensions  = Region.CalculateDimensions();
            List<BoundingRegion> octLists = new List<BoundingRegion>();
            /*
                termination conditions (don't subdivide further)
                - 1 or less objects (ie an empty leaf node or node with 1 object)
                - dimesnions are too small
            */


            if (Objects.Count <= 1) 
            {
                SetPointerToCurrentCell();
            }
            for (int i = 0; i < 3; i++)
            {
                if (dimensions[i] < MIN_BOUNDS)
                {
                    SetPointerToCurrentCell();
                }
            }
            for (int i = 0; i < NUMBER_CHILDREN; i++)
            {
                CalculateBounds(out octants[i], (Octant)(1 << i), Region);
            }
            // determine which octants to place objects in
            for (int i = 0, len = Objects.Count; i < len; i++)
            {
                BoundingRegion br = Objects[i];
                for (int j = 0; j < NUMBER_CHILDREN; j++)
                {
                    if (octants[j].ContainsRegion(br))
                    {
                        // octant contains region
                        octLists.Add(br);
                        Objects.RemoveAt(i); 

                        // offset because removed object from list
                        i--;
                        len--;
                        break;
                    }
                }
            }
            // populate octants
            for (int i = 0; i < NUMBER_CHILDREN; i++)
            {
                if (octLists.Count != 0)
                {
                    Childrens[i] = new Node(octants[i], octLists);
                    Childrens[i].Parent = this;
                    Childrens[i].Build();
                    HasChildren = true;

                }
            }

        }

        private void SetPointerToCurrentCell()
        {
            treeBuilt = true;
            treeReady = true;

            // set pointer to current cell of each object
            for (int i = 0; i < Objects.Count; i++)
            {
                Objects[i].Node = this;
            }
        }

        // update objects in tree (called during each iteration of main loop)
        public void Update(Box box)
        {
           /* if (treeBuilt && treeReady)
            {
                box.
            }*/

        }

        // process pending queue
        public void ProcessPending()
        {

        }

        // dynamically insert object into node
        public bool Insert(BoundingRegion obj)
        {
            return true;
        }

        // check collisions with all objects in node
        public void CheckCollisionsSelf(BoundingRegion obj)
        {

        }

        // check collisions with all objects in child nodes
        public void checkCollisionsChildren(BoundingRegion obj)
        {

        }

        // check collisions with a ray
        public BoundingRegion CheckCollisionsRay(Ray r, float tmin)
        {
            return new BoundingRegion( BoundTypes.AABB);
        }

        // destroy object (free memory)
        public void Destroy()
        {

        }
    }
}
