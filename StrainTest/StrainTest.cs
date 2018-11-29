using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using StrainLanguage;
using StrainLanguage.Classes;
using StrainLanguage.NodeClasses;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StrainTest
{
    [TestFixture]
    public class StrainTest
    {

        [OneTimeSetUp]
        public void Init()
        {
            IXISettings.Default(false);
        }

        [Test]
        public void FirstCompleteTest()
        {

            var code = "Test {" +
                "var int Test;" +
                "var int Test2;" +
                "entry main{" +
                "}" +
                "entry lol{" +
                "}" +
                "function test{" +
                " }" +
                " }";

            var lexedCode = new Lexer(code).Lexing();
            ;
            //var comp = new Computer(expList);
            //var result = comp.Run();

        }

        [Test]
        public void ExpressionHelperTest01()
        {

            string test = "function test(int a,int b){";

            var expHelper = new ExpressionHelper(test);

            var list = expHelper.GetParameters();

            list.Count.Should().Be(2);

        }

        [Test]
        public void ExpressionHelperTest02()
        {

            string test = "if(x == a){";

            var expHelper = new ExpressionHelper(test);

            var question = expHelper.GetQuestion();

        }

        [Test]
        [TestCase("int a = 0;", "int a = 0;")]
        public void IfElseNodeTest(string ifPara, string elsePara)
        {

            string test = "if (x == a){ " + ifPara + " }else{ " + elsePara + " } ";

            Lexer lexer = new Lexer(test);

            var treeNode = lexer.Lexing();

            var parser = new Parser(treeNode);

            var result = parser.Parse(treeNode);

            var ifelsenode = (IfElseNode)result;

            ifelsenode.IfBlock.Count.Should().Be(ifPara.Split("=").Length - 1);
            ifelsenode.ElseBlock.Count.Should().Be(elsePara.Split("=").Length - 1);
        }
    }
}
