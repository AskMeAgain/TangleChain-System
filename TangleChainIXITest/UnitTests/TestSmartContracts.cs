using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using TangleChainIXI.Smartcontracts;
using FluentAssertions;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXITest.UnitTests
{
    [TestFixture]
    public class TestSmartContracts
    {
        public Smartcontract CreateSimpleSmartcontract()
        {

            Smartcontract smart = new Smartcontract("Cool", "You");

            smart.SetFee(3)
                .AddVariable("Test", new SC_Int(0))
                .AddExpression(05, "Main")
                .AddExpression(new Expression(01, "Str_Hallo", "Test"))
                .AddExpression(new Expression(00, "Test", "Test1"))
                .AddExpression(new Expression(01, "Int_3", "R_1"))
                .AddExpression(new Expression(01, "Int_3", "R_2"))
                .AddExpression(new Expression(04, "R_1", "R_2", "R_3"))
                //we add now metadata
                .AddExpression(new Expression(11, "Int_0", "R_8"))
                .AddExpression(new Expression(11, "Int_1", "R_7"))
                .AddExpression(new Expression(11, "Int_2", "R_6"))
                .AddExpression(new Expression(11, "Int_3", "R_5"));

            return smart;

        }

        [Test]
        public void TestDownloadSpecific() {

            string json = "{\"Name\":\"cool contract\",\"SendTo\":\"YL9HYVZCUHHGXTQ9RGKTJPKXAIPKCM9DDUKGANCLHFT9NDQREJRTFXFFHRIVJPLANCQXYXR9KW9TIUOQK\",\"Hash\":\"OUVVHZHMWOHNXMAACYXN\",\"Balance\":0,\"Code\":{\"Variables\":{\"Counter\":{}},\"Expressions\":[{\"ByteCode\":5,\"Args1\":\"PayIn\",\"Args2\":\"\",\"Args3\":\"\"},{\"ByteCode\":15,\"Args1\":\"Int_2\",\"Args2\":\"R_0\",\"Args3\":\"\"},{\"ByteCode\":10,\"Args1\":\"Counter\",\"Args2\":\"R_1\",\"Args3\":\"\"},{\"ByteCode\":1,\"Args1\":\"Int_1\",\"Args2\":\"R_3\",\"Args3\":\"\"},{\"ByteCode\":3,\"Args1\":\"R_1\",\"Args2\":\"R_3\",\"Args3\":\"R_2\"},{\"ByteCode\":6,\"Args1\":\"R_2\",\"Args2\":\"Counter\",\"Args3\":\"\"},{\"ByteCode\":9,\"Args1\":\"R_0\",\"Args2\":\"R_3\",\"Args3\":\"\"},{\"ByteCode\":5,\"Args1\":\"Exit\",\"Args2\":\"\",\"Args3\":\"\"}]},\"TransactionFee\":1,\"Signature\":\"0xe1e4ddb60a60d24612daa947f22d259234bdbe4edd8bf4259754f53d4109c4052090444aa57ccc1451bba6c4ba1e364b513a3546cd2afb5a353d126b4609f97f1c\",\"From\":\"0xFe84b71404D9217522a619658E829CaABa397A20\",\"ReceivingAddress\":\"0xE5e745c5D2B5265D13b9D4768F0f18d6E78D89A4\"}";

            Smartcontract smart = Utils.FromJSON<Smartcontract>(json);

            smart.Should().NotBeNull();
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
        public void TestCompile()
        {
            Computer comp = new Computer(new Smartcontract());
            comp.Invoking(y => y.Compile()).Should().Throw<Exception>().WithMessage("Your code doesnt have any entry points!");

            var smart = CreateSimpleSmartcontract();
            Computer comp2 = new Computer(smart);

            comp2.Invoking(y => y.Compile()).Should().NotThrow<Exception>();

        }

        [Test]
        public void TestSCStuff()
        {

            SC_String s = new SC_String();

            s.value.Should().Be("");

            s.GetValueAsStringWithPrefix().Should().Be("Str_");

            SC_Int innt = new SC_Int("1");

            this.Invoking(x => new SC_Int("lol")).Should().Throw<Exception>().WithMessage("CANT CONVERT TO INT!");

            innt.GetValueAs<int>().Should().Be(1);

        }

        [Test]
        public void TestExtensions()
        {

            //this is not correct
            var s = "String_Test";

            s.GetSCType().Should().BeNull();

            var ss = "Test";
            ss.Invoking(x => x.RemovePreFix<SC_String>()).Should().Throw<Exception>();

            //test is of type
            SC_String obj = new SC_String("lol");

            obj.IsOfType<SC_String>().Should().BeTrue();

            obj.IsOfType<SC_Int, SC_String>().Should().BeTrue();

        }

        [Test]
        public void TestSmartcontract()
        {

            IXISettings.Default(true);

            var smart = CreateSimpleSmartcontract();

            var comp = new Computer(smart);

            var triggerTrans = new Transaction("me", 2, "pool addr");

            triggerTrans.AddFee(0)
                .AddData("Main")
                .Final();

            var result = comp.Run(triggerTrans);

            comp.Register.Invoking(x => x.GetFromRegister("lol")).Should().Throw<Exception>();

            result.Should().BeNull();

        }


        [Test]
        public void TestCompareCode()
        {

            var smart1 = CreateSimpleSmartcontract();
            var smart2 = CreateSimpleSmartcontract();

            smart1.Code.Should().Be(smart2.Code);
            smart1.Should().Be(smart2);

            //we now test all not be things
            smart1.Code = new Code();

            smart1.Should().NotBe(smart2);
            smart1.Code.Should().NotBe(smart2.Code);

            smart1 = CreateSimpleSmartcontract();
            smart2 = CreateSimpleSmartcontract();

            smart1.SetFee(3000);
            smart1.GetFee().Should().Be(3000);

            smart1.Should().NotBe(smart2);
        }

        [Test]
        public void TestComputerMath()
        {

            IXISettings.Default(true);

            var smart = CreateSimpleSmartcontract();

            Computer comp = new Computer(smart);

            var triggerTrans = new Transaction("me", 2, "pool addr");

            triggerTrans.AddFee(0)
                .AddData("Main")
                .Final();

            comp.Run(triggerTrans);

            comp.Register.GetFromRegister("R_3").GetValueAs<long>().Should().Be(9);
            comp.Register.GetFromRegister("R_5").GetValueAs<string>().Should().Be("me");

        }

    }
}
