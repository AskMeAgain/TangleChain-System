using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class QuestionNode : Node
    {
        public string Question{ get; protected set; }

        public QuestionNode(string question)
        {
            Question = question;

            Nodes.Add(ConstructNodeFromQuestion(question));
        }

        public override List<Expression> Compile(string context) {
           return new List<Expression>() { new Expression(01,"Int_1",context + "-Question")};
        }

        public Node ConstructNodeFromQuestion(string question) {

            //we split on && or ||
            //var array = Regex.Split(question, @"(?<=\|\||&&)").ToList();
            var array = Regex.Split(question, @"((?<=\|\||&&)|(?=\|\||&&))").ToList();
            array.RemoveAll(x => x.Equals(""));
            ;

            return null;

        }
    }
}
