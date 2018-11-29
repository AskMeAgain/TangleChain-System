using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class QuestionNode : Node
    {
        public string Question{ get; protected set; }

        public QuestionNode(string question)
        {
            Question = question;
        }
    }
}
