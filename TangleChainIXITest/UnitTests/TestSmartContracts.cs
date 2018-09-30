using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using TangleChainIXI.Smartcontracts;
using FluentAssertions;

namespace TangleChainIXITest.UnitTests
{
    [TestFixture]
    public class TestSmartContracts
    {

        [Test]
        public void TestUploadSmallCode()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("cool name", Utils.GenerateRandomString(81));
            smart.Code.AddVariable("State");

            smart.Code.AddExpression(new Expression(00, "100", "_1"));
            smart.Code.AddExpression(new Expression(00, "100", "_2"));

            smart.Code.AddExpression(new Expression(01, "_1", "_2", "_3"));
            smart.Code.AddExpression(new Expression(00, "_3", "_State"));

            smart.Final();

            var trytes = Core.Upload(smart);
            var trans = TangleNetTransaction.FromTrytes(trytes[0]);

            Smartcontract result = Smartcontract.FromJSON(trans.Fragment.ToUtf8String());

            smart.Print();
            result.Print();

            Assert.AreEqual(smart, result);



        }

        [Test]
        public void TestRunSimple()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("cool name", Utils.GenerateRandomString(81));

            smart.Code.AddVariable("State");

            smart.Code.AddExpression(new Expression(05, "Main"));
            smart.Code.AddExpression(new Expression(00, "__100", "R_1"));
            smart.Code.AddExpression(new Expression(00, "__100", "R_2"));

            smart.Code.AddExpression(new Expression(01, "R_1", "R_2", "R_3"));
            smart.Code.AddExpression(new Expression(00, "R_3", "R_4"));
            smart.Code.AddExpression(new Expression(01, "__100", "R_4", "R_5"));
            smart.Code.AddExpression(new Expression(01, "R_5", "__100", "R_6"));
            smart.Code.AddExpression(new Expression(03, "R_1", "__100", "R_7"));
            smart.Code.AddExpression(new Expression(06, "R_7", "S_State"));
            smart.Code.AddExpression(new Expression(00, "S_State", "R_8"));

            smart.Code.AddExpression(new Expression(05, "Exit"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_9"));

            smart.Final();

            Computer comp = new Computer(smart);

            Transaction trans = new Transaction();
            trans.AddFee(0);
            trans.Data.Add("Main");

            comp.Compile();
            comp.Run(trans);

            Assert.AreEqual("__200", comp.GetValue("R_4"));
            Assert.AreEqual("__300", comp.GetValue("R_5"));
            Assert.AreEqual("__400", comp.GetValue("R_6"));

            Assert.AreEqual("__10000", comp.GetValue("R_7"));
            Assert.AreEqual("__10000", comp.GetValue("S_State"));

            Assert.AreEqual(comp.GetValue("S_State"), comp.GetValue("R_8"));

            comp.Invoking(y => y.GetValue("R_9")).Should().Throw<Exception>().WithMessage("Register doesnt exist!");
        }

        [Test]
        public void TestDownloadMultipleSmartcontracts()
        {

            IXISettings.Default(true);

            string addr = Utils.GenerateRandomString(81);

            //first one:
            Smartcontract smart1 = new Smartcontract("test1", addr);
            Smartcontract smart2 = new Smartcontract("test1", addr);

            Expression exp = new Expression(00, Utils.GenerateRandomString(5), Utils.GenerateRandomString(5));

            smart1.Code.AddExpression(exp);
            smart2.Code.AddExpression(exp);

            smart1.Final();
            smart2.Final();

            Assert.AreEqual(smart1, smart2);

            Core.Upload(smart1);
            Core.Upload(smart2);

            List<Smartcontract> list = Core.GetAllSmartcontractsFromAddresss(smart1.SendTo);

            Assert.AreEqual(2, list.Count);

            smart1.Print();

        }

        [Test]
        public void TestConvertVars()
        {

            //not possible to convert
            string test01 = "asd";
            test01.Invoking(y => y._Int()).Should().Throw<Exception>().WithMessage("Sorry but you cant convert this to int!");

        }

        [Test]
        public void TestConstructors()
        {
            Smartcontract smart = new Smartcontract() { Name = "LOL", SendTo = "lol" };
        }

        [Test]
        public void TestReturnTransaction()
        {

            IXISettings.Default(true);

            string receiver = "__" + Utils.GenerateRandomString(81);

            Smartcontract smart = new Smartcontract();

            smart.Code.AddExpression(new Expression(05, "Main"));
            smart.Code.AddExpression(new Expression(09, receiver, "__100"));

            smart.Final();

            Computer comp = new Computer(smart);

            Transaction t = new Transaction();
            t.AddFee(0);
            t.Data.Add("Main");
            Transaction trans = comp.Run(t);

            Assert.AreEqual(trans.OutputReceiver[0], receiver.Substring(2));
            Assert.AreEqual(trans.OutputValue[0], 100);

        }

        [Test]
        public void TestJson()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("test", "lol");
            smart.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3", "__1"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart.Final();

            string json = smart.ToJSON();

            Smartcontract result = Smartcontract.FromJSON(json);

            result.Print();

            result.Should().Be(smart);
        }

        [Test]
        public void StoreAndGetSmartcontracts()
        {

            string name = "teeest2";
            string smartName = "cool smartcontract";

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract(smartName, Utils.GenerateRandomString(81));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3", "__1"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3", "__1"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3", "__1"));

            smart.Code.AddVariable("State");
            smart.Code.AddVariable("State3");
            smart.Code.AddVariable("St3333ate");
            smart.Code.AddVariable("State5");
            smart.Code.AddVariable("Sta333333te");

            smart.Final();
            smart.Print();

            Block block = new Block(3, Utils.GenerateRandomString(81), name);
            block.Final();
            block.GenerateProofOfWork(new Difficulty(3));

            DBManager.AddBlock(block, false, false);
            DBManager.AddSmartcontract(name, smart, 3);

            Smartcontract result = DBManager.GetSmartcontract(name, smart.ReceivingAddress);

            result.Print();

            Assert.AreEqual(smart, result);

        }

        [Test]
        public void TestStringToCode()
        {

            string smartName = "cool smartcontract";

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract(smartName, Utils.GenerateRandomString(81));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3", "__1"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3", "__1"));
            smart.Code.AddExpression(new Expression(05, "Exit"));
            //smart.Code.AddVariable("State");



            string s = smart.Code.ToFlatString();

            Code c = Smartcontract.StringToCode(s);

            smart.Code.Should().Be(c);

        }

        [Test]
        public void TestEquals()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("test", "lol");
            Smartcontract smart2 = new Smartcontract("test222", "lol");

            smart.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3", "__1"));
            smart.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart.Final();

            smart2.Code.AddExpression(new Expression(00, "__1", "R_3"));
            smart2.Final();

            smart.Should().NotBe(smart2);


        }

        [Test]
        public void TestGetValues()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("test222", "lol");
            smart.Code.AddExpression(new Expression(05, "Main"));
            smart.Code.AddExpression(new Expression(00, "S_Test", "R_3"));
            smart.Code.AddExpression(new Expression(00, "D_0", "R_1"));

            smart.Final();

            Computer comp = new Computer(smart);

            Transaction t = new Transaction();
            t.AddFee(0);
            t.Data.Add("Main");

            comp.Invoking(y => y.Run(t)).Should().Throw<Exception>().WithMessage("State doesnt exist!");

            comp.Invoking(y => y.GetValue("_Start")).Should().Throw<Exception>().WithMessage("sorry but your input is wrong formated");

            Transaction dataTrans = new Transaction();
            dataTrans.AddFee(100);
        }

        [Test]
        public void TestGetValues2()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("test222", "lol");
            smart.Code.AddExpression(new Expression(05, "Main"));
            smart.Code.AddExpression(new Expression(00, "D_0", "R_1"));
            smart.Code.AddExpression(new Expression(00, "T_0", "R_2"));
            smart.Code.AddExpression(new Expression(00, "T_1", "R_3"));
            smart.Code.AddExpression(new Expression(00, "T_2", "R_4"));
            smart.Code.AddExpression(new Expression(00, "T_3", "R_5"));

            smart.Final();

            Computer comp = new Computer(smart);

            Transaction dataTrans = new Transaction("Me", 1, "pooladdr");
            dataTrans.Final();

            dataTrans.AddFee(100);
            dataTrans.Data.Add("Main");

            comp.Run(dataTrans);

            comp.GetValue("D_0").Should().Be("__100");

            comp.GetValue("R_5").Should().Be("__Me");
            comp.GetValue("R_4").Should().Be("__" + dataTrans.Time);
            comp.GetValue("R_3").Should().Be("__pooladdr");
            comp.GetValue("R_2").Should().Be("__" + dataTrans.Hash);
            comp.Invoking(y => y.GetValue("L_2")).Should().Throw<Exception>().WithMessage("sorry but your pre flag doesnt exist!");

        }

        [Test]
        public void TestChangeRegister()
        {


            Computer comp = new Computer(new Smartcontract());

            comp.ChangeRegister("R_1", "__3");
            comp.ChangeRegister("R_1", "__5");

            comp.Register["R_1"].Should().Be("__5");

        }

        [Test]
        public void TestCompile()
        {

            Computer comp = new Computer(new Smartcontract());
            comp.Invoking(y => y.Compile()).Should().Throw<Exception>().WithMessage("Your code doesnt have any entry points!");


        }

    }
}
