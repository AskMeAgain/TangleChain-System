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
        public void TestUploadDownloadSmallCode() {

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
        public void TestRunSimple() {

            Smartcontract smart = new Smartcontract();
            smart.Code.AddVariable("State");

            smart.Code.AddExpression(new Expression(05, "Main"));
            smart.Code.AddExpression(new Expression(00, "__100", "R_1"));
            smart.Code.AddExpression(new Expression(00, "__100", "R_2"));

            smart.Code.AddExpression(new Expression(01, "R_1", "R_2","R_3"));
            smart.Code.AddExpression(new Expression(00, "R_3", "R_4"));
            smart.Code.AddExpression(new Expression(01, "__100", "R_4","R_5"));
            smart.Code.AddExpression(new Expression(01, "R_5", "__100","R_6"));
            smart.Code.AddExpression(new Expression(03, "R_1", "__100","R_7"));
            smart.Code.AddExpression(new Expression(06, "R_7", "S_State"));
            smart.Code.AddExpression(new Expression(00, "S_State", "R_8"));

            smart.Code.AddExpression(new Expression(05, "Exit"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_9"));

            Computer comp = new Computer(smart);

            comp.Compile();
            comp.Run("Main" /*,DATA FIELD OF TRANSACTION*/);

            Assert.AreEqual("__200", comp.GetValue("R_4"));
            Assert.AreEqual("__300", comp.GetValue("R_5"));
            Assert.AreEqual("__400", comp.GetValue("R_6"));

            Assert.AreEqual("__10000", comp.GetValue("R_7"));
            Assert.AreEqual("__10000", comp.GetValue("S_State"));

            Assert.AreEqual(comp.GetValue("S_State"), comp.GetValue("R_8"));

            //throws error "doesnt exist!"
            Assert.AreNotEqual(comp.GetValue("__1"), comp.GetValue("R_9"));
        }

        [Test]
        public void TestDownloadMultipleSmartcontracts() {

            IXISettings.Default(true);

            //first one:
            Smartcontract smart1 = new Smartcontract();
            Smartcontract smart2 = new Smartcontract();

            Expression exp = new Expression(00, Utils.GenerateRandomString(5), Utils.GenerateRandomString(5));

            smart1.Code.AddExpression(exp);
            smart2.Code.AddExpression(exp);
            
            smart1.Final();
            smart2.Final();
            
            Assert.AreEqual(smart1, smart2);

            Core.UploadSmartContract(smart1);
            Core.UploadSmartContract(smart2);

            List<Smartcontract> list = Core.GetAllSmartcontractsFromAddresss(smart1.SendTo);

            Assert.AreEqual(2, list.Count);

            smart1.Print();

        }

        [Test]
        public void TestGetValue() {

            //needs to get done
        }

        [Test]
        public void TestReturnTransaction() {

            //needs to get done
        }


    }
}
