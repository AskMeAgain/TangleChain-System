using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using StrainLanguage;
using StrainLanguage.Classes;
using StrainLanguage.NodeClasses;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

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

        private List<Expression> CreateExpressionList(string code)
        {
            var treeNode = new Lexer(code).Lexing();

            var parser = new Parser(treeNode);

            var result = parser.Parse();
            ;
            return result.Compile();
            ;
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
        [TestCase("1-1-1-1-1-0", "1-1-1-1-1")]
        public void ContextJumpTest(string context, string result)
        {

            var check = Utils.JumpContextUp(context);

            result.Should().Be(check);
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

        [Test]
        public void SimpleAssignTest()
        {

            var code = "Application {" +
                "entry main {" +
                "int Test2 = 3;" +
                "int Test3 = Test2;" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);
            var result = comp.Run("main");

            comp.CheckRegister("Test2").GetValueAs<int>().Should().Be(3);
            comp.CheckRegister("Test3").GetValueAs<int>().Should().Be(3);

        }

        [Test]
        public void CommentTest()
        {

            var code = "Application {" +
                "entry Main {" +
                "//Please ignore this one;" +
                "int Test3 = 3;" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);
            var result = comp.Run();

            list.Count.Should().Be(5);
            comp.CheckRegister("Test3").GetValueAs<int>().Should().Be(3);

        }

        [Test]
        [TestCase("3 + 3", 6)]
        [TestCase("4 - 3", 1)]
        [TestCase("4 * 3", 12)]
        [TestCase("4 * 3 - 10", 2)]
        [TestCase("4 * 3 + 10", 22)]
        [TestCase("10 - 2 * 5", 0)]
        [TestCase("Test * 10 - 9", 1)]
        [TestCase("Test * Test - Test", 0)]
        public void SimpleMathTest02(string exp, int equals)
        {

            var code = "Application {" +
                "entry Main {" +
                "int Test = 1;" +
                $"Test = {exp};" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);
            var result = comp.Run();

            comp.CheckRegister("Test").GetValueAs<int>().Should().Be(equals);

        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "Test5 is not in scope")]
        public void NotInScope()
        {

            var code = "Application {" +
                "entry Main {" +
                "int Test2 = 1;" +
                "if(0 == 0){" +
                "Test2 = 1111;" +
                "}else{" +
                "Test2 = 33;" +
                "int Test5 = 1 + 1;" +
                "}" +
                "int Test3 = Test2 + 1;" +
                "int Test6 = Test5 + Test5;" +
                "}" +
                "}";

            CreateExpressionList(code);
        }

        [Test]
        public void IfSimpleTest01()
        {

            var code = "Application {" +
                "entry Main {" +
                "int comparer = 33;"+
                "if(0 == 0){" +
                "int Test1 = 1;" +
                "}" +
                "if(0 != comparer){" +
                "int Test2 = 1;" +
                "}" +
                "if(33 == comparer){" +
                "int Test3 = 1;" +
                "}" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);
            var result = comp.Run();

            comp.CheckRegister("Test1").GetValueAs<int>().Should().Be(1);
            comp.CheckRegister("Test2").GetValueAs<int>().Should().Be(1);
            comp.CheckRegister("Test3").GetValueAs<int>().Should().Be(1);

        }
    }
}
