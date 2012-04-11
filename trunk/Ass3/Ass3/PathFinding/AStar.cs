using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFinding
{
    //A* path finding class
    class AStar
    {
        //Starting position of path to find
        private Node _origin;

        public Node Origin
        {
            get { return _origin; }
            set 
            {
                Completed = false;
                _origin = value;
                OpenList.Clear();
                ClosedList.Clear();
                NodesOpened.Clear();
                OpenList.Add(_origin.ID.ToString(), new NodeConnection(_origin, 0, new List<Node>(), (_destination.position - _origin.position).Length()));
            }
        }

        //Destination position of path to find
        private Node _destination;

        public Node Destination
        {
            get { return _destination; }
            set { _destination = value; }
        }

        public bool Completed;//Indicate completion of the algorithm between two nodes
        public Dictionary<string, NodeConnection> ClosedList; //List of closed visited nodes
        public Dictionary<string, NodeConnection> OpenList; // List of Opened visited nodes
        public List<Node> NodesOpened; //List of all nodes that were ever opened

        public AStar()
        {
            Completed = false;
            OpenList = new Dictionary<string, NodeConnection>();
            ClosedList = new Dictionary<string,NodeConnection>();
            NodesOpened = new List<Node>();
        }

        public List<Node> GetShortestPath()
        {
            //Set first node
            KeyValuePair<string, NodeConnection> node = new KeyValuePair<string, NodeConnection>(_origin.ID.ToString(), OpenList[_origin.ID.ToString()]);
            
            //Add first path position
            node.Value.Connection.Add(node.Value.nextNode);

            //Compute shortestPath
            while (true)
            {
                //If current node is goal node, we have a path to return. It is most likely to be close to being the shortest or the shortest.
                if (node.Value.nextNode.ID == Destination.ID)
                    break;

                //For each link associated with the current node, add them to the open list and deal with duplicate nodes in the open list and closed list to overwrite those with irrelevant or longer paths.
                for (int i = 0; i < node.Value.nextNode.Links.Count; i++)
                {
                    Link currentLink = node.Value.nextNode.Links[i];

                    string ID = currentLink.node.ID.ToString();
                    NodeConnection nodetoAdd = new NodeConnection();

                    nodetoAdd.ID = currentLink.node.ID;
                    nodetoAdd.costSoFar = node.Value.costSoFar + currentLink.weight;
                    nodetoAdd.Connection = node.Value.Connection.ToList();
                    nodetoAdd.Connection.Add(currentLink.node);
                    nodetoAdd.EstimatedCostSoFar = nodetoAdd.costSoFar + (_destination.position - currentLink.node.position).Length();
                    nodetoAdd.nextNode = currentLink.node;

                    //Add Node tp list of all opened nodes ever visited if it doesn't exist
                    if (!NodesOpened.Contains(currentLink.node))
                    {
                        NodesOpened.Add(currentLink.node);
                    }

                    //Add node to open list or update open node in the open list if the current cost so far is smaller
                    if (!OpenList.ContainsKey(ID))
                    {
                        OpenList.Add(ID, nodetoAdd);
                    }
                    else
                    {
                        if (OpenList[ID].costSoFar > nodetoAdd.costSoFar)
                        {
                            OpenList[ID] = nodetoAdd;
                        }
                    }
                }

                //Add node to the closed list or update the node in the closed list if the current node has a smaller cost so far and put it back in the open list
                if (!ClosedList.ContainsKey(node.Key))
                {
                    ClosedList.Add(node.Key, node.Value);
                }
                else
                {
                    if (ClosedList[node.Key].costSoFar > node.Value.costSoFar)
                    {
                        ClosedList[node.Key] = node.Value;
                        OpenList.Add(node.Key, ClosedList[node.Key]);
                    }
                }

                //Remove processed node from the open list
                OpenList.Remove(node.Key);


                //Obtain new node with the smallest estimated cost so far
                node = new KeyValuePair<string, NodeConnection>();

                foreach (KeyValuePair<string, NodeConnection> nConn in OpenList)
                {
                    if (node.Key == null)
                    {
                        node = nConn;
                        continue;
                    }

                    if (nConn.Value.EstimatedCostSoFar < node.Value.EstimatedCostSoFar)
                    {
                        node = nConn;
                        continue;
                    }
                }
            }

            //Path finding completed, return sequence of nodes to travel
            Completed = true;

            return node.Value.Connection;

        }

    }
}
