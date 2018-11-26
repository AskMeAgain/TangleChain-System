using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using StrainLanguage;
using StrainLanguage.Classes;
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
        public void ExpressionHelperTest01() {

            string test = "function test(int a,int b){";

            var expHelper = new ExpressionHelper(test);

            var list = expHelper.GetParameters();

            list.Count.Should().Be(2);

        }

        [Test]
        public void ExpressionHelperTest02() {

            string test = "if(x == a){";

            var expHelper = new ExpressionHelper(test);

            var question = expHelper.GetQuestion();

        }
    }
}
