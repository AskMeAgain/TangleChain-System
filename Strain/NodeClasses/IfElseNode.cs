using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IfElseNode : Node
    {
        public QuestionNode Question { get; protected set; }
        public List<Node> IfBlock { get; protected set; }
        public List<Node> ElseBlock { get; protected set; }

        public IfElseNode(QuestionNode question, List<Node> ifBlock, List<Node> elseBlock)
        {
            Question = question;
            IfBlock = ifBlock;
            ElseBlock = elseBlock;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();

            //the whole question stuff
            var questionList = Question.Compile(scope, context.NewContext());
            var questionResult = questionList.Last().Args3;

            list.AddRange(questionList);

            //we first check if questionStuff.Last() is 0
            list.Add(Factory.BranchIfOne(context + "-IfTrue", questionResult)); //goto IfTrue if equal
            list.Add(Factory.Goto(context + "-Else")); //gotoelse
            list.Add(Factory.Label(context + "-IfTrue")); //IFTrue label

            list.AddRange(IfBlock.Compile(scope, context, "IfTrue")); //we add the iftrue block

            list.Add(Factory.Goto(context + "-End")); //we jump now to end
            list.Add(Factory.Label(context + "-Else")); //Else label

            list.AddRange(ElseBlock.Compile(scope, context,"Else")); //we add stuff

            list.Add(Factory.Label(context + "-End")); //end label

            return list;
        }
    }
}
