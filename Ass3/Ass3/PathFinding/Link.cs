using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PathFinding
{
    //Link class used to join nodes together
    public class Link
    {
        public Node node; //Linked node
        public float weight;//Weight value for this link

        public Link(Node node)
        {
            this.node = node;
        }
    }
}
