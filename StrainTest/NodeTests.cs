using System;
using NUnit.Framework;
using FluentAssertions;
using Strain;
using Strain.Classes;
using System.Collections.Generic;
using TangleChainIXI.Smartcontracts;

namespace StrainTest
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void Mvp()
        {

            //we first create a block node
            //we fill it with a simple assignments

            var assignment = new IntroduceNode(new ValueNode("R_1"), new ValueNode("Str_1"));
            var assignment2 = new AddNode(new ValueNode("Str_1"), new ValueNode("Str_1"));
            var assignment3 = new IntroduceNode(new ValueNode("R_1"), new ValueNode("Str_1"));
            var assignment4 = new IntroduceNode(new ValueNode("R_1"), new ValueNode("Str_1"));

            var block = new FunctionNode("Main", assignment, assignment2, assignment3, assignment4);

            var result = new Parser(block).Parse();

        }

        [Test]
        public void FunctionCall01()
        {

            //main:
            //function call test
            //end

            //test
            //do stuff

            var left = new ValueNode("Str_1");
            var right = new ValueNode("Str_1");

            var addnode = new AddNode(left, right);

            var function = new FunctionNode("test", addnode);

            var funcCall = new FunctionCallNode("test");

            var node = new Node(funcCall, function);

            var result = new Parser(node).Parse();

            ;
        }


    }
}
