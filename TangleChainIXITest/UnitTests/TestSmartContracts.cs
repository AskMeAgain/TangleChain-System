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

            Smartcontract smart = new Smartcontract("cool name", Utils.GenerateRandomString(81))
                .AddVariable("State")
                .AddExpression(00, "100", "_1")
                .AddExpression(00, "100", "_2")
                .AddExpression(01, "_1", "_2", "_3")
                .AddExpression(00, "_3", "_State")
                .Final()
                .Upload();


            Smartcontract result = Core.GetSpecificFromAddress<Smartcontract>(smart.SendTo, smart.Hash);

            smart.Print();
            result.Print();

            smart.Should().Be(result);

        }

        [Test]
        public void TestRunSimple()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("cool name", Utils.GenerateRandomString(81))
                .AddVariable("State")
                .AddExpression(05, "Main")
                .AddExpression(00, "__100", "R_1")
                .AddExpression(00, "__100", "R_2")
                .AddExpression(01, "R_1", "R_2", "R_3")
                .AddExpression(00, "R_3", "R_4")
                .AddExpression(01, "__100", "R_4", "R_5")
                .AddExpression(01, "R_5", "__100", "R_6")
                .AddExpression(03, "R_1", "__100", "R_7")
                .AddExpression(06, "R_7", "S_State")
                .AddExpression(00, "S_State", "R_8")
                .AddExpression(05, "Exit")
                .AddExpression(00, "__1", "R_9")
                .Final();

            Computer comp = new Computer(smart);

            Transaction trans = new Transaction()
                .AddFee(0)
                .AddData("Main");

            comp.Compile();
            comp.Run(trans);

            "__200".Should().Be(comp.GetValue("R_4"));
            "__300".Should().Be(comp.GetValue("R_5"));
            "__400".Should().Be(comp.GetValue("R_6"));
            "__10000".Should().Be(comp.GetValue("R_7"));
            "__10000".Should().Be(comp.GetValue("S_State"));
            comp.GetValue("S_State").Should().Be(comp.GetValue("R_8"));
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

            smart1.AddExpression(exp).Final().Upload();
            smart2.AddExpression(exp).Final().Upload();

            smart1.Should().Be(smart2);

            List<Smartcontract> list = Core.GetAllFromAddress<Smartcontract>(smart1.SendTo);

            list.Count.Should().Be(2);

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

            smart.AddExpression(05, "Main")
                .AddExpression(09, receiver, "__100")
                .Final();


            Transaction triggerTrans = new Transaction()
                .AddFee(0)
                .AddData("Main");

            Computer comp = new Computer(smart);
            Transaction trans = comp.Run(triggerTrans);

            Assert.AreEqual(trans.OutputReceiver[0], receiver.Substring(2));
            Assert.AreEqual(trans.OutputValue[0], 100);

        }

        [Test]
        public void TestJson()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("test", "lol");
            smart.AddExpression(00, "__1", "R_3")
                .AddExpression(00, "__1", "R_3", "__1")
                .AddExpression(00, "__1", "R_3")
                .Final();

            Utils.FromJSON<Smartcontract>(smart.ToJSON()).Should().Be(smart);
        }

        [Test]
        public void StoreAndGetSmartcontracts()
        {

            string name = "teeest2";
            string smartName = "cool smartcontract";

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract(smartName, Utils.GenerateRandomString(81));

            smart.AddExpression(00, "__1", "R_3")
                .AddExpression(00, "__1", "R_3", "__1")
                .AddExpression(00, "__1", "R_3")
                .AddExpression(00, "__1", "R_3", "__1")
                .AddExpression(00, "__1", "R_3")
                .AddExpression(00, "__1", "R_3", "__1")
                .AddVariable("State", "__0")
                .AddVariable("State3")
                .AddVariable("St3333ate")
                .AddVariable("State5")
                .AddVariable("Sta333333te")
                .Final();

            smart.Print();

            Block block = new Block(3, Utils.GenerateRandomString(81), name);

            block.Final().GenerateProofOfWork(3);

            DBManager.AddBlock(block);
            DBManager.AddSmartcontract(name, smart, 3);

            DBManager.GetSmartcontract(name, smart.ReceivingAddress).Should().Be(smart);

        }

        [Test]
        public void TestStringToCode()
        {

            string smartName = "cool smartcontract";

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract(smartName, Utils.GenerateRandomString(81));
            smart.AddExpression(00, "__1", "R_3")
                .AddExpression(00, "__1", "R_3", "__1")
                .AddExpression(00, "__1", "R_3")
                .AddExpression(00, "__1", "R_3", "__1")
                .AddExpression(05, "Exit");


            string s = smart.Code.ToFlatString();

            Code c = SmartcontractUtils.StringToCode(s);

            smart.Code.Should().Be(c);

        }

        [Test]
        public void TestEquals()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("test", "lol");
            Smartcontract smart2 = new Smartcontract("test222", "lol");

            smart.AddExpression(00, "__1", "R_3")
                .AddExpression(00, "__1", "R_3", "__1")
                .AddExpression(00, "__1", "R_3")
                .Final();

            smart2.AddExpression(00, "__1", "R_3").Final();

            smart.Should().NotBe(smart2);

        }

        [Test]
        public void TestGetValues()
        {

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("test222", "lol");
            smart.AddExpression(05, "Main")
                .AddExpression(00, "S_Test", "R_3")
                .AddExpression(00, "D_0", "R_1")
                .Final();

            Computer comp = new Computer(smart);

            Transaction t = new Transaction()
                .AddFee(0)
                .AddData("Main");

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
            smart.AddExpression(05, "Main")
                .AddExpression(00, "D_0", "R_1")
                .AddExpression(00, "T_0", "R_2")
                .AddExpression(00, "T_1", "R_3")
                .AddExpression(00, "T_2", "R_4")
                .AddExpression(00, "T_3", "R_5")
                .Final();

            Computer comp = new Computer(smart);

            Transaction dataTrans = new Transaction("Me", 1, "pooladdr")
                .AddFee(100)
                .AddData("Main")
                .Final();

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
