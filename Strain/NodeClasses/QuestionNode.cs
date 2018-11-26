using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class QuestionNode : Node
    {
        private string _question;

        public QuestionNode(string question)
        {
            _question = question;
        }
    }
}
