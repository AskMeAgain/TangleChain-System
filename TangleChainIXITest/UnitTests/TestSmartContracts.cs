using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXITest.UnitTests {
    [TestFixture]
    public class TestSmartContracts {

        [Test]
        public void TestUploadSmallCode() {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract();
            smart.Code.AddVariable("State");

            smart.Code.AddExpression(new Expression(00, "100", "_1"));
            smart.Code.AddExpression(new Expression(00, "100", "_2"));

            smart.Code.AddExpression(new Expression(01, "_1", "_2","_3"));
            smart.Code.AddExpression(new Expression(00, "_3", "_State"));

            smart.Final();

            var trytes = Core.UploadSmartContract(smart);
            var trans = TangleNetTransaction.FromTrytes(trytes[0]);

            Smartcontract result = Smartcontract.FromJSON(trans.Fragment.ToUtf8String());

            smart.Print();
            result.Print();

            Assert.AreEqual(smart, result);

        }

        [Test]
        public void TestRun() {

            Smartcontract smart = new Smartcontract();
            smart.Code.AddVariable("State");

            smart.Code.AddExpression(new Expression(00, "100", "_1"));
            smart.Code.AddExpression(new Expression(00, "100", "_2"));

            smart.Code.AddExpression(new Expression(01, "_1", "_2","_3"));
            smart.Code.AddExpression(new Expression(00, "_3", "_4"));
            smart.Code.AddExpression(new Expression(01, "100", "_4","_5"));

            Computer comp = new Computer(smart);

            comp.Run();

            Assert.AreEqual("200", comp.GetValue("_4"));
            Assert.AreEqual("300", comp.GetValue("_5"));


        }

    }
}
