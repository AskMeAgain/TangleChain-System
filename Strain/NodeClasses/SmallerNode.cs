using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class SmallerNode:Node
    {
        public Node Left { get; set; }
        public Node Right { get; set; }

        public SmallerNode(Node left, Node right)
        {
            Left = left;
            Right = right;
        }
    }
}
