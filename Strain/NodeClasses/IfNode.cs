using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IfNode : Node
    {
        private string _question;

        public IfNode(string question, FunctionNode node)
        {
            Nodes = new List<Node>() { node };
            _question = question;
        }

        public override List<Expression> Compile(string context)
        {
            //we first need to compile the question into a result
            //_question parse

            var list = CompileQuestion(_question, context);

            //args3 of the last exp will contain the result (1 or 0).
            list.Add(new Expression(21, Nodes.First().GetArgument<string>(0), list.Last().Args3)); //goto function

            list.Add(new Expression(13, context + "-0-endoffunction")); //gotoendofFunction

            list.AddRange(Nodes.Last().Compile(context + "-0-function"));

            list.Add(new Expression(05, context + "-0-endoffunction"));//end of function

            return list;
        }

        private List<Expression> CompileQuestion(string question, string context)
        {

            //will always be of pattern if(X < 5){
            var brokenDown = question.Substring(question.LastIndexOf("(") + 1, question.LastIndexOf(")") - question.LastIndexOf("(") - 1);
            var helper = new ExpressionHelper(brokenDown);
            var list = new List<Expression>();

            //we introduce the vars
            list.Add(new Expression(01, helper[0], context + "-0-QLeft"));
            list.Add(new Expression(01, helper[2], context + "-0-QRight"));

            //left is smaller then right
            if (helper[1].Equals("<"))
            {
                //we compare now
                list.Add(new Expression(22, context + "-0-QLeft", context + "-0-QRight", context + "-0-QResult"));
            }

            //left is bigger then right
            if (helper[1].Equals(">"))
            {
                //we compare now
                list.Add(new Expression(23, context + "-0-QLeft", context + "-0-QRight", context + "-0-QResult"));
            }

            //left is equal right
            if (helper[1].Equals("="))
            {
                //we compare now
                list.Add(new Expression(24, context + "-0-QLeft", context + "-0-QRight", context + "-0-QResult"));
            }

            return list;
        }

    }
}
