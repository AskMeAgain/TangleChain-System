using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using TangleChainIXI;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXITest.UnitTests
{
    [TestFixture]
    public class TestAssembly
    {

        [OneTimeSetUp]
        public void Init()
        {
            IXISettings.Default(true);
        }

        public (Computer comp, Transaction result) RunHelper(string label, List<Expression> expList, List<Variable> varList = null)
        {

            Smartcontract smart = new Smartcontract("cool name", Utils.GenerateRandomString(81));
            smart.AddExpression(new Expression(05, "Main"));

            expList.ForEach(x => smart.AddExpression(x));

            if (varList != null)
            {
                varList.ForEach(x => smart.AddVariable(x.Name));
            }

            smart.Final();

            Transaction trans = new Transaction("me", 2, Utils.GenerateRandomString(81));
            trans.AddFee(0)
                .AddData(label)
                .Final();

            Computer comp = new Computer(smart);
            Transaction result = comp.Run(trans);

            return (comp, result);
        }

        public List<Expression> IntroduceIntegers(List<Expression> list, string a = "Int_1", string b = "Int_2")
        {

            list.Add(new Expression(01, a, "R_1"));
            list.Add(new Expression(01, b, "R_2"));

            return list;
        }

        public List<Expression> IntroduceLongs(List<Expression> list, string a = "Lon_3", string b = "Lon_4")
        {

            list.Add(new Expression(01, a, "R_3"));
            list.Add(new Expression(01, b, "R_4"));

            return list;
        }

        public List<Expression> IntroduceStrings(List<Expression> list, string a = "Str_5", string b = "Str_6")
        {

            list.Add(new Expression(01, a, "R_5"));
            list.Add(new Expression(01, b, "R_6"));

            return list;
        }


        [Test]
        public void ThrowErrors()
        {

            var list = new List<Expression>();

            list.Add(new Expression(11, "Int_30"));

            this.Invoking(y => y.RunHelper("Main", list)).Should().Throw<Exception>().WithMessage("Wrong Index");

        }

        [Test]
        public void IntroduceVariable()
        {
            var list = new List<Expression>();
            var varList = new List<Variable>();
            varList.Add(new Variable("Counter"));

            list.Add(new Expression(10, "Counter", "R_1"));

            var bundle = RunHelper("Main", list, varList);

            bundle.comp.State.GetFromRegister("Counter").GetValueAs<int>().Should().Be(0);
            bundle.comp.Register.GetFromRegister("R_1").GetValueAs<int>().Should().Be(0);

        }

        [Test]
        public void IntroduceMetaData()
        {
            var list = new List<Expression>();

            list.Add(new Expression(11, "Int_0", "R_1"));

            var bundle = RunHelper("Main", list);

            bundle.comp.Register.GetFromRegister("R_1").GetValueAs<string>().Should().NotBeNull();

        }

        [Test]
        public void TestOutTransaction()
        {

            var receiver = Utils.GenerateRandomString(81);
            var receiver2 = Utils.GenerateRandomString(81);

            var list = new List<Expression>();

            //first receiver
            list.Add(new Expression(01, "Str_" + receiver, "R_0"));
            list.Add(new Expression(01, "Int_10", "R_1"));

            //second receiver
            list.Add(new Expression(01, "Str_" + receiver2, "R_3"));
            list.Add(new Expression(01, "Int_20", "R_4"));


            list.Add(new Expression(09, "R_0", "R_1"));
            list.Add(new Expression(09, "R_3", "R_4"));

            var bundle = RunHelper("Main", list);

            //first receiver
            bundle.result.OutputReceiver.Should().Contain(receiver);
            bundle.result.OutputValue.Should().Contain(10);

            //second receiver
            bundle.result.OutputReceiver.Should().Contain(receiver2);
            bundle.result.OutputValue.Should().Contain(20);

        }

        [Test]
        public void TestMoreMath01()
        {
            var list = new List<Expression>();

            list = IntroduceIntegers(list);
            list = IntroduceLongs(list);
            list = IntroduceStrings(list);

            //1-2 int
            //3-4 long
            //5-6 string
            list.Add(new Expression(03,"R_1","R_3","R_10"));
            list.Add(new Expression(03,"R_1","R_5","R_11"));
            list.Add(new Expression(04,"R_1","R_3","R_12"));

            list.Add(new Expression(12,"R_1","R_3","R_13"));

            var bundle = RunHelper("Main", list);

            bundle.comp.Register.GetFromRegister("R_10").GetValueAs<long>().Should().Be(4);
            bundle.comp.Register.GetFromRegister("R_10").IsOfType<SC_Long>().Should().BeTrue();

            bundle.comp.Register.GetFromRegister("R_11").GetValueAs<string>().Should().Be("15");

            bundle.comp.Register.GetFromRegister("R_12").GetValueAs<long>().Should().Be(3);
            bundle.comp.Register.GetFromRegister("R_13").GetValueAs<long>().Should().Be(-2);
        }
    }
}
