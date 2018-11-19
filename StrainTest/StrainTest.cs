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

        [Test]
        public void FirstCompleteTest()
        {

            IXISettings.Default(true);

            var code = "function Main {" +
                "int ShouldBe2 = 1 + 1;" +
                "}";

            var expList = new Strain("CoolApp", code).Compile();

            var smart = new Smartcontract("test", "lol");
            smart.Code.Expressions = expList;
            smart.Final();

            var triggerTrans = new Transaction("asd", -2, "lol")
                .AddFee(0)
                .AddData("Main")
                .Final();

            var comp = new Computer(smart);
            var result = comp.Run(triggerTrans);

        }
    }
}
