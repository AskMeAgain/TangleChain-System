using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class QuestionNode : Node
    {
        public string Question{ get; protected set; }

        public QuestionNode(string question)
        {
            Question = question;
        }

        public override List<Expression> Compile(string context) {

           return new List<Expression>() { new Expression(01,"Int_1",context + "-Question")};

        }
    }
}
