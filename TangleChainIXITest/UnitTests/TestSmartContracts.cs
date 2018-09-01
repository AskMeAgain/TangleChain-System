using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;

namespace TangleChainIXITest.UnitTests {
    [TestFixture]
    public class TestSmartContracts {

        [Test]
        public void TestUploadSmallCode() {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract();

            Code code = new Code();

            code.AddVariable(new Variable("state", 1));

            Method m = new Method("test 1");
            m.AddExpression(new Expression("state = 0"));
            m.AddExpression(new Expression("if (state == 0) balance == 0"));

            code.AddMethod(m);

            smart.AddCode(code);
            smart.Final();

            var trytes = Core.UploadSmartContract(smart);
            var trans = TangleNetTransaction.FromTrytes(trytes[0]);

            Smartcontract result = Smartcontract.FromJSON(trans.Fragment.ToUtf8String());

            smart.Print();
            result.Print();

            Assert.AreEqual(smart, result);
        }

    }
}
