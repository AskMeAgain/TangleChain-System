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
    [Parallelizable(ParallelScope.All)]
    public class TestSmartContracts
    {

        [OneTimeSetUp]
        public void Init()
        {
            IXISettings.Default(true);
        }

        private Smartcontract CreateSimpleSmartcontract()
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
        public void TestJsonConvertion()
        {

            var preSmart = new Smartcontract();

            preSmart.Code.Expressions.Add(new Expression(00, "asd"));
            preSmart.Code.Variables.Add("lol", new SC_Int(1));
            preSmart.Code.Variables.Add("lol2", new SC_String("1"));
            preSmart.Code.Variables.Add("lol3", new SC_Long(1));

            string json = preSmart.ToJSON();

            Smartcontract smart = Utils.FromJSON<Smartcontract>(json);

            smart.Should().NotBeNull();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "Your code doesnt have any entry points!")]
        public void TestCompile()
        {

            new Computer(new Smartcontract()).Compile();

        }

        [Test]
        [ExpectedNoException]
        public void TestCompile02()
        {

            var smart = CreateSimpleSmartcontract();
            new Computer(smart).Compile();
        }

        [Test]
        public void TestSCStuff()
        {

            SC_String s = new SC_String();

            s.value.Should().Be("");

            s.GetValueAsStringWithPrefix().Should().Be("Str_");

            s.ToString().Should().Be("Str_");

            SC_Int innt = new SC_Int("1");

            innt.GetValueAs<int>().Should().Be(1);
            innt.ToString().Should().Be("Int_1");

            SC_Long lon = new SC_Long(1);
            lon.ToString().Should().Be("Lon_1");

        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "CANT CONVERT TO INT!")]
        public void TestSCStuffThrow()
        {
            new SC_Int("lol");
        }

        [Test]
        public void TestExtensions()
        {

            //this is not correct
            var s = "String_Test";

            s.GetSCType().Should().BeNull();

            //test is of type
            SC_String obj = new SC_String("lol");

            obj.IsOfType<SC_String>().Should().BeTrue();
            obj.IsOfType<SC_Int, SC_String>().Should().BeTrue();

        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "ERROR CANT REMOVE PREFIX OF System.String[]")]
        public void TestExtensionThrow()
        {
            "Test".RemovePreFix<SC_String>();
        }

        [Test]
        public void TestSmartcontract()
        {


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

        [Test]
        public void TestCodeConvertion()
        {

            var smart = CreateSimpleSmartcontract();

            var flat = smart.Code.ToFlatString();

            var code = SmartcontractUtils.StringToCode(flat);

            code.Should().Be(smart.Code);

        }
    }
}
