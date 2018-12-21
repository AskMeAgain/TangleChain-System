using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.Classes
{
    public class LexerNode
    {
        public int Order { get; set; }
        public string Line { get; set; }
        public List<LexerNode> SubLines { get; } = new List<LexerNode>();
    }

}
