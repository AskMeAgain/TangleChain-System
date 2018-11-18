using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.Classes
{
    public class TreeNode
    {
        public int Order { get; set; }
        public string Line { get; set; }
        public List<TreeNode> SubLines { get; set; } = new List<TreeNode>();
    }

}
