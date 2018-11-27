using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class EntryNode : Node
    {

        private string _name;

        public EntryNode(string name, List<Node> list)
        {
            _name = name;
            Nodes = list;
        }

    }
}
