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

            smart.Code.AddExpression(new Expression(00, 2, 3));
            smart.Code.AddExpression(new Expression(00, 2, 3));
            smart.Code.AddExpression(new Expression(00, 2, 3));
            smart.Code.AddExpression(new Expression(00, 2, 3));

            smart.Final();

            var trytes = Core.UploadSmartContract(smart);
            var trans = TangleNetTransaction.FromTrytes(trytes[0]);

            Smartcontract result = Smartcontract.FromJSON(trans.Fragment.ToUtf8String());

            smart.Print();
            result.Print();

            Assert.AreEqual(smart, result);

        }

        [Test]
        public void TestPrint() {

        }

    }
}
