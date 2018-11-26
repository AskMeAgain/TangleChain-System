using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class IfNode : Node
    {

        public IfNode(QuestionNode question, params Node[] nodes)
        {
            Nodes = nodes.ToList(); ;
        }
    }
}
