﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public override List<Expression> Compile(string context)
        {
            var list = new List<Expression>();

            //the whole question stuff
            var questionList = Question.Compile(context + "-0");

            list.AddRange(questionList);

            //we first check if questionStuff.Last() is 0
            list.Add(new Expression(01, "Int_1", context + "-Compare"));
            list.Add(new Expression(14, context + "-IfTrue", context + "-Compare", questionList.Last().Args2)); //goto IfTrue if equal
            list.Add(new Expression(13, context + "-Else")); //gotoelse
            list.Add(new Expression(05, context + "-IfTrue")); //IFTrue label

            int i = 0;
            list.AddRange(IfBlock.SelectMany(x => x.Compile(context + "-IfTrue-" + i++))); //we add the iftrue block
            list.Add(new Expression(13, context + "-End")); //we jump now to end
            list.Add(new Expression(05, context + "-Else")); //Else label

            int ii = 0;
            list.AddRange(ElseBlock.SelectMany(x => x.Compile(context + "-Else-" + ii++))); //we add stuff
            list.Add(new Expression(05, context + "-End")); //end label

            return list;
        }
    }
}
