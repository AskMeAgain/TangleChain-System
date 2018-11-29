using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class EntryNode : Node
    {

        public string EntryName{ get; protected set; }

        public EntryNode(string entryName, List<Node> list)
        {
            EntryName = entryName;
            Nodes = list;
        }

    }
}
