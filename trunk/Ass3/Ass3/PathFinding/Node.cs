using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections;

namespace PathFinding
{
    //Vertex of a graph class
    public class Node
    {
        public static int CurrentID;
        public Vector3 position; //Position of the vertex/Node
        public List<Link> Links; //Nodes which this vertex is attached to through a directed graph
        public int ID; // Unique ID for identification purposes

        public Node()
        {
            position = new Vector3(1.5f,0,1.5f);
            Links = new List<Link>();
            ID = -1;
        }

        public Node(Vector3 pos)
        {
            position = pos;
            Links = new List<Link>();
            ID = CurrentID;
            CurrentID++;
        }
    }
}
