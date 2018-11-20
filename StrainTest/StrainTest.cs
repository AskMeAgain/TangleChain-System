using System;
using System.Collections.Generic;
using System.Text;
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
                "int ShouldBe2 = 1 + 1;" +
                "}";

            var expList = new Strain("CoolApp", code).Compile();

            var comp = new Computer(expList);
            var result = comp.Run();

        }
    }
}
