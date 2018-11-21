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

            var code = "function Main {" +
                "if(1 < 3){" +
                "int a = 3;" +
                "}" +
                "}";

            var expList = new Strain("CoolApp", code).Compile();

            var comp = new Computer(expList);
            var result = comp.Run();

            comp.Register.GetFromRegister("CoolApp-0-0-function-0").GetValueAs<int>().Should().Be(3);
        }
    }
}
