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

        [Test]
        public void FirstCompleteTest()
        {

            var code = "Main {" +
                "var int Test;" +
                "var int Test2;" +
                "entry main{" +
                "int test = 3;" +
                "}" +
                "entry lol{" +
                "int test = 3;" +
                "}" +
                "function test{" +
                " }" +
                " }";

            var treeNode = new Lexer(code).Lexing();

            var parser = new Parser(treeNode);

            var result = parser.Parse();

            var comp = new Computer(result.Compile());

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

        [Test]
        public void AssignTest()
        {

            string test = "test{ int a = 3; a = variableName; }";

            Lexer lexer = new Lexer(test);

            var treeNode = lexer.Lexing();

            var parser = new Parser(treeNode);

            var result = parser.Parse();

            result.Nodes[0].Should().BeOfType<AssignNode>();

            ((AssignNode)result.Nodes[0]).Name.Should().Be("a");
            ((AssignNode)result.Nodes[0]).Type.Should().Be("int");
            ((AssignNode)result.Nodes[0]).Nodes.Count.Should().Be(1);
            ((AssignNode)result.Nodes[0]).Nodes[0].Should().BeOfType<ExpressionNode>();

            ((AssignNode)result.Nodes[0]).Nodes[0].Nodes[0].Should().BeOfType<ValueNode>();

            var variableNode = (VariableNode)result.Nodes[1];

            variableNode.Nodes[0].Nodes[0].Should().BeOfType<VariableNode>();

        }
    }
}
