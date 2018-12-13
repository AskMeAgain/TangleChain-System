using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class SmallerEqualNode:Node
    {
        public Node Left { get; set; }
        public Node Right { get; set; }

        public SmallerEqualNode(Node left, Node right)
        {
            Left = left;
            Right = right;
        }
    }
}
