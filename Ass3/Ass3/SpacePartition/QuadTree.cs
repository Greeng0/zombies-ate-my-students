using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Collisions;

namespace SpacePartition
{
    class QuadTree
    {
        //ID of the last Node in the tree
        static long CurrentID;

        //Number of items inserted in the tree
        private long itemCount;

        public long ItemCount
        {
            get { return itemCount; }
            set { itemCount = value; }
        }

        //Head of tree
        private QuadTreeNode head;

        internal QuadTreeNode Head
        {
            get { return head; }
            set { head = value; }
        }

        //Tree Depth
        private int depth;

        public int Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        //Ctor
        public QuadTree(Vector2 Position, float size, int Depth)
        {
            this.depth = Depth;
            CurrentID = 0;
            Head = new QuadTreeNode(Position, size, null, ref CurrentID, Depth);
        }

        //Insert a box element in the tree
        public void Insert(Box box, QuadTreeNode node = null,int DepthLevel=0)
        {
            if (node == null)
                node = Head;

            Vector3 HeadOffset = new Vector3(Head.Position.X, 0, Head.Position.Y);
            //Verifies for all 4 corners of the box including based on the rotation offset of the box
            Vector3 offsetPos = Vector3.Transform(new Vector3(box.Size.X / 2, 0, box.Size.Z / 2), box.Offset);

            Vector3 corner1 = new Vector3(box.Position.X - offsetPos.X, 0, box.Position.Z + offsetPos.Z);
            Vector3 corner2 = new Vector3(box.Position.X + offsetPos.X, 0, box.Position.Z + offsetPos.Z);
            Vector3 corner3 = new Vector3(box.Position.X - offsetPos.X, 0, box.Position.Z - offsetPos.Z);
            Vector3 corner4 = new Vector3(box.Position.X + offsetPos.X, 0, box.Position.Z - offsetPos.Z);

            //Verifies for all 4 sides of the box including based on the rotation offset of the box
            offsetPos = Vector3.Transform(new Vector3(box.Size.X / 2, 0, box.Size.Z / 2), box.Offset);

            Vector3 Side1 = new Vector3(box.Position.X - offsetPos.X, 0, box.Position.Z + offsetPos.Z);
            Vector3 Side2 = new Vector3(box.Position.X + offsetPos.X, 0, box.Position.Z + offsetPos.Z);
            Vector3 Side3 = new Vector3(box.Position.X - offsetPos.X, 0, box.Position.Z - offsetPos.Z);
            Vector3 Side4 = new Vector3(box.Position.X + offsetPos.X, 0, box.Position.Z - offsetPos.Z);

            //Make sure Box is smaller than current quadrant. If yes, go deeper, otherwise add elements here
            if (DepthLevel < this.Depth && box.Size.X <= node.Size && box.Size.Z <= node.Size)
            {
                //Process North-West Part
                if (node.Children[0] != null)
                {
                    if (corner1.X <= node.Position.X && corner1.Z <= node.Position.Y ||
                        corner2.X <= node.Position.X && corner2.Z <= node.Position.Y ||
                        corner3.X <= node.Position.X && corner3.Z <= node.Position.Y ||
                        corner4.X <= node.Position.X && corner4.Z <= node.Position.Y ||
                        Side1.X <= node.Position.X && Side1.Z <= node.Position.Y ||
                        Side2.X <= node.Position.X && Side2.Z <= node.Position.Y ||
                        Side3.X <= node.Position.X && Side3.Z <= node.Position.Y ||
                        Side4.X <= node.Position.X && Side4.Z <= node.Position.Y)
                    {

                        Insert(box, node.Children[0],DepthLevel+1);
                    }
                }

                //Process North-East Part
                if (node.Children[1] != null)
                {
                    if (corner1.X >= node.Position.X && corner1.Z <= node.Position.Y ||
                        corner2.X >= node.Position.X && corner2.Z <= node.Position.Y ||
                        corner3.X >= node.Position.X && corner3.Z <= node.Position.Y ||
                        corner4.X >= node.Position.X && corner4.Z <= node.Position.Y ||
                        Side1.X >= node.Position.X && Side1.Z <= node.Position.Y ||
                        Side2.X >= node.Position.X && Side2.Z <= node.Position.Y ||
                        Side3.X >= node.Position.X && Side3.Z <= node.Position.Y ||
                        Side4.X >= node.Position.X && Side4.Z <= node.Position.Y)
                    {

                        Insert(box, node.Children[1], DepthLevel + 1);
                    }
                }

                //Process South-West Part
                if (node.Children[2] != null)
                {
                    if (corner1.X <= node.Position.X && corner1.Z >= node.Position.Y ||
                        corner2.X <= node.Position.X && corner2.Z >= node.Position.Y ||
                        corner3.X <= node.Position.X && corner3.Z >= node.Position.Y ||
                        corner4.X <= node.Position.X && corner4.Z >= node.Position.Y ||
                        Side1.X <= node.Position.X && Side1.Z >= node.Position.Y ||
                        Side2.X <= node.Position.X && Side2.Z >= node.Position.Y ||
                        Side3.X <= node.Position.X && Side3.Z >= node.Position.Y ||
                        Side4.X <= node.Position.X && Side4.Z >= node.Position.Y)
                    {

                        Insert(box, node.Children[2], DepthLevel + 1);
                    }
                }

                //Process South-East Part
                if (node.Children[3] != null)
                {
                    if (corner1.X >= node.Position.X && corner1.Z >= node.Position.Y ||
                        corner2.X >= node.Position.X && corner2.Z >= node.Position.Y ||
                        corner3.X >= node.Position.X && corner3.Z >= node.Position.Y ||
                        corner4.X >= node.Position.X && corner4.Z >= node.Position.Y ||
                        Side1.X >= node.Position.X && Side1.Z >= node.Position.Y ||
                        Side2.X >= node.Position.X && Side2.Z >= node.Position.Y ||
                        Side3.X >= node.Position.X && Side3.Z >= node.Position.Y ||
                        Side4.X >= node.Position.X && Side4.Z >= node.Position.Y)
                    {

                        Insert(box, node.Children[3], DepthLevel + 1);
                    }
                }
            }
            else
            {
                //Add box in current layer
                if (node.Primitives == null)
                    node.Primitives = new List<Primitive>();

                if (!node.Primitives.Contains(box))
                {
                    node.Primitives.Add(box);
                    ItemCount++;
                }
            }
        }

        //Insert a Pathfinding node in the tree
        public void Insert(PathFinding.Node pfNode, QuadTreeNode node = null, int DepthLevel = 0)
        {
            if (node == null)
                node = Head;

            //Make sure Box is smaller than current quadrant. If yes, go deeper, otherwise add elements here
            if (DepthLevel < this.Depth)
            {
                //Process North-West Part
                if (node.Children[0] != null)
                {
                    if (pfNode.position.X <= node.Position.X && pfNode.position.Z <= node.Position.Y)
                    {
                        Insert(pfNode, node.Children[0], DepthLevel + 1);
                    }
                }

                //Process North-East Part
                if (node.Children[1] != null)
                {
                    if (pfNode.position.X >= node.Position.X && pfNode.position.Z <= node.Position.Y)
                    {

                        Insert(pfNode, node.Children[1], DepthLevel + 1);
                    }
                }

                //Process South-West Part
                if (node.Children[2] != null)
                {
                    if (pfNode.position.X <= node.Position.X && pfNode.position.Z >= node.Position.Y)
                    {

                        Insert(pfNode, node.Children[2], DepthLevel + 1);
                    }
                }

                //Process South-East Part
                if (node.Children[3] != null)
                {
                    if (pfNode.position.X >= node.Position.X && pfNode.position.Z >= node.Position.Y)
                    {

                        Insert(pfNode, node.Children[3], DepthLevel + 1);
                    }
                }
            }
            else
            {
                //Add box in current layer
                if (node.PathFindingNodes == null)
                    node.PathFindingNodes = new List<PathFinding.Node>();

                if (!node.PathFindingNodes.Contains(pfNode))
                {
                    node.PathFindingNodes.Add(pfNode);
                    ItemCount++;
                }
            }
        }

        //Retrieve Objects nearby PathFinding Nodes given Position in the Quad Tree and Superior layers of the quadrant if UpperLayerDepth > 0
        public void RetrieveNearbyObjects(Vector3 Position, ref List<PathFinding.Node> pfNodesNearby, int UpperLayerDepth, QuadTreeNode node, int DepthCounter = 0)
        {
            if (node == null)
                node = Head;

            //Process North-West Part
            if (node.Children[0] != null)
            {
                //If Sphere can be found inside the North West quadrant of the current quadrant or if this depth level should be covered, go inside
                if ((Position.X <= node.Position.X && Position.Z <= node.Position.Y) || (this.depth - DepthCounter) <= UpperLayerDepth)
                {
                    RetrieveNearbyObjects(Position, ref pfNodesNearby, UpperLayerDepth, node.Children[0], DepthCounter + 1);
                }
            }

            //Process North-East Part
            if (node.Children[1] != null)
            {
                //If Sphere can be found inside the North East quadrant of the current quadrant or if this depth level should be covered, go inside
                if ((Position.X >= node.Position.X &&Position.Z <= node.Position.Y) || (this.depth - DepthCounter) <= UpperLayerDepth)
                {
                    RetrieveNearbyObjects(Position, ref pfNodesNearby, UpperLayerDepth, node.Children[1], DepthCounter + 1);
                }
            }

            //Process South-West Part
            if (node.Children[2] != null)
            {
                //If Sphere can be found inside the South West quadrant of the current quadrant or if this depth level should be covered, go inside
                if ((Position.X <= node.Position.X && Position.Z >= node.Position.Y) || (this.depth - DepthCounter) <= UpperLayerDepth)
                {
                    RetrieveNearbyObjects(Position, ref pfNodesNearby, UpperLayerDepth, node.Children[2], DepthCounter + 1);
                }
            }

            //Process South-East Part
            if (node.Children[3] != null)
            {
                //If Sphere can be found inside the South West quadrant of the current quadrant or if this depth level should be covered, go inside
                if ((Position.X >= node.Position.X && Position.Z >= node.Position.Y) || (this.depth - DepthCounter) <= UpperLayerDepth)
                {
                    RetrieveNearbyObjects(Position, ref pfNodesNearby, UpperLayerDepth, node.Children[3], DepthCounter + 1);
                }
            }

            //Return all primitives found in the node 
            if (node.PathFindingNodes != null)
            {
                //Add all primitives found in the quadrant that aren't already in the list
                foreach (PathFinding.Node pfNode in node.PathFindingNodes)
                {
                    if (!pfNodesNearby.Contains(pfNode))
                    {
                        pfNodesNearby.Add(pfNode);
                    }
                }
            }
        }

        //Return boxes that generate Quad Tree Grid. It is possible to filter lower levels and display only higher ones by setting UpperLayerDepth to a value > 0
        public List<Box> RetrieveBoundariesFromPosition(Sphere sphere, ref List<Box> boxes, int UpperLayerDepth = 0, QuadTreeNode node = null)
        {
            if (node == null)
                node = Head;

            if (boxes == null)
                boxes = new List<Box>();

            float nodeSize = (float)Math.Pow(2, UpperLayerDepth);

            //Make sure sphere fits in the current node and go deeper
            if (sphere.Radius * 2 < node.Size / nodeSize)
            {
                //Process North-West Part
                if (node.Children[0] != null)
                {
                    if (sphere.Position.X - sphere.Radius <= node.Position.X && sphere.Position.Z + sphere.Radius <= node.Position.Y)
                    {
                        RetrieveBoundariesFromPosition(sphere, ref boxes, UpperLayerDepth, node.Children[0]);
                    }
                }

                //Process North-East Part
                if (node.Children[1] != null)
                {
                    if (sphere.Position.X + sphere.Radius >= node.Position.X && sphere.Position.Z + sphere.Radius <= node.Position.Y)
                    {

                        RetrieveBoundariesFromPosition(sphere, ref boxes, UpperLayerDepth, node.Children[1]);
                    }
                }

                //Process South-West Part
                if (node.Children[2] != null)
                {
                    if (sphere.Position.X - sphere.Radius <= node.Position.X && sphere.Position.Z - sphere.Radius >= node.Position.Y)
                    {

                        RetrieveBoundariesFromPosition(sphere, ref boxes, UpperLayerDepth, node.Children[2]);
                    }
                }

                //Process South-East Part
                if (node.Children[3] != null)
                {
                    if (sphere.Position.X + sphere.Radius >= node.Position.X && sphere.Position.Z - sphere.Radius >= node.Position.Y)
                    {

                        RetrieveBoundariesFromPosition(sphere, ref boxes, UpperLayerDepth, node.Children[3]);
                    }
                }
            }

            //return box sized based on depth level
            boxes.Add(new Box(new Vector3(node.Position.X, 1.5f, node.Position.Y), new Vector3(0), new Vector3(node.Size * 2, 3, node.Size * 2)));

            return boxes;
        }

        //Retrieve Objects nearby given Sphere in the Quad Tree and Superior layers of the quadrant if UpperLayerDepth > 0
        public void RetrieveNearbyObjects(Sphere sphere, ref List<Primitive> primitivesNearby, int UpperLayerDepth = 0)
        {
            RetrieveNearbyObjects(sphere, ref primitivesNearby, UpperLayerDepth, null);
        }

        //Retrieve Objects nearby given Sphere in the Quad Tree and Superior layers of the quadrant if UpperLayerDepth > 0
        public void RetrieveNearbyObjects(Sphere sphere, ref List<Primitive> primitivesNearby, int UpperLayerDepth, QuadTreeNode node, int DepthCounter = 0)
        {
            if (node == null)
                node = Head;

            //Process North-West Part
            if (node.Children[0] != null)
            {
                //If Sphere can be found inside the North West quadrant of the current quadrant or if this depth level should be covered, go inside
                if ((sphere.Position.X - sphere.Radius <= node.Position.X && sphere.Position.Z + sphere.Radius <= node.Position.Y) || (this.depth - DepthCounter) <= UpperLayerDepth)
                {
                    RetrieveNearbyObjects(sphere, ref primitivesNearby, UpperLayerDepth, node.Children[0], DepthCounter + 1);
                }
            }

            //Process North-East Part
            if (node.Children[1] != null)
            {
                //If Sphere can be found inside the North East quadrant of the current quadrant or if this depth level should be covered, go inside
                if ((sphere.Position.X + sphere.Radius >= node.Position.X && sphere.Position.Z + sphere.Radius <= node.Position.Y) || (this.depth - DepthCounter) <= UpperLayerDepth)
                {
                    RetrieveNearbyObjects(sphere, ref primitivesNearby, UpperLayerDepth, node.Children[1], DepthCounter + 1);
                }
            }

            //Process South-West Part
            if (node.Children[2] != null)
            {
                //If Sphere can be found inside the South West quadrant of the current quadrant or if this depth level should be covered, go inside
                if ((sphere.Position.X - sphere.Radius <= node.Position.X && sphere.Position.Z - sphere.Radius >= node.Position.Y) || (this.depth - DepthCounter) <= UpperLayerDepth)
                {
                    RetrieveNearbyObjects(sphere, ref primitivesNearby, UpperLayerDepth, node.Children[2], DepthCounter + 1);
                }
            }

            //Process South-East Part
            if (node.Children[3] != null)
            {
                //If Sphere can be found inside the South West quadrant of the current quadrant or if this depth level should be covered, go inside
                if ((sphere.Position.X + sphere.Radius >= node.Position.X && sphere.Position.Z - sphere.Radius >= node.Position.Y) || (this.depth - DepthCounter) <= UpperLayerDepth)
                {
                    RetrieveNearbyObjects(sphere, ref primitivesNearby, UpperLayerDepth, node.Children[3], DepthCounter + 1);
                }
            }

            //Return all primitives found in the node 
            if (node.Primitives != null)
            {
                //Add all primitives found in the quadrant that aren't already in the list
                foreach (Primitive prim in node.Primitives)
                {
                    if (!primitivesNearby.Contains(prim))
                    {
                        primitivesNearby.Add(prim);
                    }
                }
            }
        }

        //Degrees to Radian angle conversion
        public double DegreeToRad(float angle)
        {
            return (Math.PI * angle) / 180f;
        }


        //*UNUSED* *UNCOMMENTED*
        //Return boxes that generate Quad Tree Grid. It is possible to filter lower levels and display only higher ones by setting UpperLayerDepth to a value > 0
        public List<Box> RetrieveBoundariesFromPosition(Box box, ref List<Box> boxes, int UpperLayerDepth = 0, QuadTreeNode node = null)
        {
            if (node == null)
                node = Head;

            if (boxes == null)
                boxes = new List<Box>();

            //Verifies for all 4 sides of the box based on the rotation offset of the box
            Vector3 offsetPos = Vector3.Transform(new Vector3(box.Size.X / 2, 0, box.Size.Z / 2), box.Offset);

            Vector3 corner1 = new Vector3(box.Position.X - offsetPos.Z, 0, box.Position.Z + offsetPos.X);
            Vector3 corner2 = new Vector3(box.Position.X + offsetPos.X, 0, box.Position.Z + offsetPos.Z);
            Vector3 corner3 = new Vector3(box.Position.X - offsetPos.X, 0, box.Position.Z - offsetPos.Z);
            Vector3 corner4 = new Vector3(box.Position.X + offsetPos.Z, 0, box.Position.Z - offsetPos.X);

            //Verifies for all 4 sides of the box based on the rotation offset of the box
            offsetPos = Vector3.Transform(new Vector3(box.Size.X / 2, 0, 0), box.Offset);

            Vector3 Side1 = new Vector3(box.Position.X - offsetPos.Z, 0, box.Position.Z + offsetPos.X);
            Vector3 Side2 = new Vector3(box.Position.X + offsetPos.X, 0, box.Position.Z + offsetPos.Z);
            Vector3 Side3 = new Vector3(box.Position.X - offsetPos.X, 0, box.Position.Z - offsetPos.Z);
            Vector3 Side4 = new Vector3(box.Position.X + offsetPos.Z, 0, box.Position.Z - offsetPos.X);

            //Boxes are drawn twice as big as the node's size
            if (box.Size.X <= node.Size && box.Size.Z <= node.Size)
            {
                if (node.Children[0] != null)
                {
                    if (corner1.X <= node.Position.X && corner1.Z >= node.Position.Y ||
                        corner2.X <= node.Position.X && corner2.Z >= node.Position.Y ||
                        corner3.X <= node.Position.X && corner3.Z >= node.Position.Y ||
                        corner4.X <= node.Position.X && corner4.Z >= node.Position.Y ||
                        Side1.X <= node.Position.X && Side1.Z >= node.Position.Y ||
                        Side2.X <= node.Position.X && Side2.Z >= node.Position.Y ||
                        Side3.X <= node.Position.X && Side3.Z >= node.Position.Y ||
                        Side4.X <= node.Position.X && Side4.Z >= node.Position.Y)
                    {

                        RetrieveBoundariesFromPosition(box, ref boxes, UpperLayerDepth, node.Children[0]);
                    }
                }

                if (node.Children[1] != null)
                {
                    if (corner1.X >= node.Position.X && corner1.Z >= node.Position.Y ||
                        corner2.X >= node.Position.X && corner2.Z >= node.Position.Y ||
                        corner3.X >= node.Position.X && corner3.Z >= node.Position.Y ||
                        corner4.X >= node.Position.X && corner4.Z >= node.Position.Y ||
                        Side1.X >= node.Position.X && Side1.Z >= node.Position.Y ||
                        Side2.X >= node.Position.X && Side2.Z >= node.Position.Y ||
                        Side3.X >= node.Position.X && Side3.Z >= node.Position.Y ||
                        Side4.X >= node.Position.X && Side4.Z >= node.Position.Y)
                    {

                        RetrieveBoundariesFromPosition(box, ref boxes, UpperLayerDepth, node.Children[1]);
                    }
                }

                if (node.Children[2] != null)
                {
                    if (corner1.X <= node.Position.X && corner1.Z <= node.Position.Y ||
                        corner2.X <= node.Position.X && corner2.Z <= node.Position.Y ||
                        corner3.X <= node.Position.X && corner3.Z <= node.Position.Y ||
                        corner4.X <= node.Position.X && corner4.Z <= node.Position.Y ||
                        Side1.X <= node.Position.X && Side1.Z <= node.Position.Y ||
                        Side2.X <= node.Position.X && Side2.Z <= node.Position.Y ||
                        Side3.X <= node.Position.X && Side3.Z <= node.Position.Y ||
                        Side4.X <= node.Position.X && Side4.Z <= node.Position.Y)
                    {

                        RetrieveBoundariesFromPosition(box, ref boxes, UpperLayerDepth, node.Children[2]);
                    }
                }

                if (node.Children[3] != null)
                {
                    if (corner1.X >= node.Position.X && corner1.Z <= node.Position.Y ||
                        corner2.X >= node.Position.X && corner2.Z <= node.Position.Y ||
                        corner3.X >= node.Position.X && corner3.Z <= node.Position.Y ||
                        corner4.X >= node.Position.X && corner4.Z <= node.Position.Y ||
                        Side1.X >= node.Position.X && Side1.Z <= node.Position.Y ||
                        Side2.X >= node.Position.X && Side2.Z <= node.Position.Y ||
                        Side3.X >= node.Position.X && Side3.Z <= node.Position.Y ||
                        Side4.X >= node.Position.X && Side4.Z <= node.Position.Y)
                    {

                        RetrieveBoundariesFromPosition(box, ref boxes, UpperLayerDepth, node.Children[3]);
                    }
                }
            }

            boxes.Add(new Box(new Vector3(node.Position.X, 1.5f, node.Position.Y), new Vector3(0), new Vector3(node.Size * 2, 3, node.Size * 2)));

            return boxes;
        }
    }

    //Node of a Quad Tree
    class QuadTreeNode
    {
        //List of all primitives in the node
        private List<Primitive> primitives;

        internal List<Primitive> Primitives
        {
            get { return primitives; }
            set { primitives = value; }
        }

        private List<PathFinding.Node> pathFindingNodes;

        internal List<PathFinding.Node> PathFindingNodes
        {
            get { return pathFindingNodes; }
            set { pathFindingNodes = value; }
        }


        //Size of the quadrant for that node
        float size;

        public float Size
        {
            get { return size; }
            set { size = value; }
        }

        //Unique Node ID
        private long uID;
        public long UID
        {
            get { return uID; }
            set { uID = value; }
        }

        //Position of Center of Node Quadrant
        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        //Link to Parent Node
        QuadTreeNode parent;
        internal QuadTreeNode Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        //The 4 sub Quadrants of the node
        QuadTreeNode[] children = new QuadTreeNode[4];
        internal QuadTreeNode[] Children
        {
            get { return children; }
            set { children = value; }
        }

        //Ctor
        public QuadTreeNode(Vector2 position, float size, QuadTreeNode parent, ref long uID, int depth)
        {
            //Assign Unique ID and increase ID counter
            this.UID = uID;
            uID++;

            this.position = position;
            this.parent = parent;
            this.size = size;

            //Divide size for new cells
            size /= 2;

            if (depth > 0)
            {
                Vector2 NWposition = new Vector2(position.X - size, position.Y - size);
                Vector2 NEposition = new Vector2(position.X + size, position.Y - size);
                Vector2 SWposition = new Vector2(position.X - size, position.Y + size);
                Vector2 SEposition = new Vector2(position.X + size, position.Y + size);

                children[0] = new QuadTreeNode(NWposition, size, this, ref uID, depth - 1);
                children[1] = new QuadTreeNode(NEposition, size, this, ref uID, depth - 1);
                children[2] = new QuadTreeNode(SWposition, size, this, ref uID, depth - 1);
                children[3] = new QuadTreeNode(SEposition, size, this, ref uID, depth - 1);
            }
        }
    }
}
