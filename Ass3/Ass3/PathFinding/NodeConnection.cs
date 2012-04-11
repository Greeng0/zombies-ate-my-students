using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFinding
{
    //A* helping class for computing node connections
    class NodeConnection
    {
        public int ID; // ID of node
        public Node nextNode; //next node to process
        public float costSoFar;//cost so far for the path
        public List<Node> Connection; //Path travelled so far
        public float EstimatedCostSoFar;// Estimated 

        public NodeConnection(Node next, float cost, List<Node> connection, float estimatedCost)
        {
            ID = next.ID;
            nextNode = next;
            costSoFar = cost;
            Connection = connection;
            EstimatedCostSoFar = estimatedCost;
        }

        public NodeConnection()
        {
            ID = -1;
            nextNode = new Node();
            costSoFar = 0;
            Connection = new List<Node>();
            EstimatedCostSoFar = 0;
        }
    }
}
