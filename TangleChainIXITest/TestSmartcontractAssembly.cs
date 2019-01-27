using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using TangleChainIXI;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXITest
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class TestSmartcontractAssembly
    {
        private IXISettings _settings;

        [OneTimeSetUp]
        public void Init()
        {
            _settings = new IXISettings().Default(true);
        }

        private (Computer comp, Maybe<Transaction> result) RunHelper(string label, List<Expression> expList, Dictionary<string, ISCType> varList = null)
        {

            Smartcontract smart = new Smartcontract("cool name", Utils.GenerateRandomString(81));
            smart.AddExpression(new Expression(05, "Main"));

            expList.ForEach(x => smart.AddExpression(x));

            if (varList != null)
            {
                smart.Code.Variables = varList;
            }

            smart.Final(_settings);

            Transaction trans = new Transaction("me", 2, Utils.GenerateRandomString(81));
            trans.AddFee(0)
                .AddData(label)
                .Final(_settings);

            Computer comp = new Computer(smart);
            var result = comp.Run(trans);

            return (comp, result);
        }

        private List<Expression> IntroduceIntegers(List<Expression> list, string a = "Int_1", string b = "Int_2")
        {

            list.Add(new Expression(01, a, "R_1"));
            list.Add(new Expression(01, b, "R_2"));

            return list;
        }

        private List<Expression> IntroduceLongs(List<Expression> list, string a = "Lon_3", string b = "Lon_4")
        {

            list.Add(new Expression(01, a, "R_3"));
            list.Add(new Expression(01, b, "R_4"));

            return list;
        }

        private List<Expression> IntroduceStrings(List<Expression> list, string a = "Str_5", string b = "Str_6")
        {

            list.Add(new Expression(01, a, "R_5"));
            list.Add(new Expression(01, b, "R_6"));

            return list;
        }


        [Test]
        [ExpectedException(typeof(ArgumentException), "Wrong Index")]
        public void WrongIndex()
        {

            var list = new List<Expression>();

            list.Add(new Expression(11, "Int_30"));

            RunHelper("Main", list);

        }

        [Test]
        public void IntroduceVariable()
        {
            var list = new List<Expression>();
            var varList = new Dictionary<string, ISCType>();
            varList.Add("Counter", new SC_Int(0));

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
            bundle.result.HasValue.Should().BeTrue();
            bundle.result.Value.OutputReceiver.Should().Contain(receiver);
            bundle.result.Value.OutputValue.Should().Contain(10);

            //second receiver
            bundle.result.Value.OutputReceiver.Should().Contain(receiver2);
            bundle.result.Value.OutputValue.Should().Contain(20);

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
            list.Add(new Expression(01, "Str_100", "R_7"));
            list.Add(new Expression(03, "R_1", "R_3", "R_10"));
            list.Add(new Expression(03, "R_1", "R_5", "R_11"));
            list.Add(new Expression(04, "R_1", "R_3", "R_12"));

            list.Add(new Expression(12, "R_1", "R_3", "R_13"));

            list.Add(new Expression(03, "R_5", "R_1", "R_14"));

            list.Add(new Expression(04, "R_5", "R_2", "R_15"));

            list.Add(new Expression(04, "R_5", "R_2", "R_15"));

            list.Add(new Expression(03, "R_3", "R_2", "R_16"));
            list.Add(new Expression(03, "R_3", "R_5", "R_17"));

            list.Add(new Expression(04, "R_3", "R_2", "R_18"));

            list.Add(new Expression(12, "R_3", "R_2", "R_19"));

            var bundle = RunHelper("Main", list);

            bundle.comp.Register.GetFromRegister("R_10").GetValueAs<long>().Should().Be(4);
            bundle.comp.Register.GetFromRegister("R_10").IsOfType<SC_Long>().Should().BeTrue();

            bundle.comp.Register.GetFromRegister("R_11").GetValueAs<string>().Should().Be("15");

            bundle.comp.Register.GetFromRegister("R_12").GetValueAs<long>().Should().Be(3);
            bundle.comp.Register.GetFromRegister("R_13").GetValueAs<long>().Should().Be(-2);

            bundle.comp.Register.GetFromRegister("R_14").GetValueAs<string>().Should().Be("51");

            bundle.comp.Register.GetFromRegister("R_15").GetValueAs<string>().Should().Be("55");

            bundle.comp.Register.GetFromRegister("R_16").GetValueAs<long>().Should().Be(5);
            bundle.comp.Register.GetFromRegister("R_17").GetValueAs<string>().Should().Be("35");
            bundle.comp.Register.GetFromRegister("R_18").GetValueAs<long>().Should().Be(6);

            bundle.comp.Register.GetFromRegister("R_19").GetValueAs<long>().Should().Be(1);

            bundle.comp.Register.GetFromRegister("R_3").GetValueAsStringWithPrefix().Should().Be("Str_3");

            bundle.comp.Register.GetFromRegister("R_7").Invoking(x => x.GetValueAs<bool>()).Should().Throw<Exception>()
                .WithMessage("100 is not a valid value for Boolean.");

        }

        [Test]
        public void TestEquals()
        {
            var list = new List<Expression>();

            list = IntroduceIntegers(list);
            list = IntroduceLongs(list);
            list = IntroduceStrings(list);

            var bundle = RunHelper("Main", list);

            var obj1 = bundle.comp.Register.GetFromRegister("R_1");
            var obj3 = bundle.comp.Register.GetFromRegister("R_3");
            var obj5 = bundle.comp.Register.GetFromRegister("R_5");

            obj1.IsEqual(obj3).Should().BeFalse();
            obj1.IsEqual(obj5).Should().BeFalse();
            obj3.IsEqual(obj5).Should().BeFalse();
            obj5.IsEqual(obj1).Should().BeFalse();

            obj5.IsEqual(obj5).Should().BeTrue();

        }

        [Test]
        public void TestBranching()
        {

            var list = new List<Expression>();

            list = IntroduceIntegers(list);

            //main:
            //branch to JumpHere if R_1 == R_1
            //R_2 = 333
            //JumpHere
            //R_2 should be empty here

            list.Add(new Expression(14, "JumpHere", "R_1", "R_1"));
            list.Add(new Expression(01, "Int_333", "R_2"));
            list.Add(new Expression(28, "JumpHere"));

            var bundle = RunHelper("Main", list);

            bundle.comp.Register.GetFromRegister("R_2").GetValueAs<int>().Should().Be(2);

        }

        [Test]
        public void TestBranching2()
        {

            var list = new List<Expression>();

            list = IntroduceIntegers(list);

            //main:
            //branch to JumpHere if R_1 != R_2
            //R_2 = 333
            //JumpHere
            //R_2 should be empty here

            list.Add(new Expression(16, "JumpHere", "R_1", "R_2"));
            list.Add(new Expression(01, "Int_333", "R_2"));
            list.Add(new Expression(28, "JumpHere"));

            var bundle = RunHelper("Main", list);

            bundle.comp.Register.GetFromRegister("R_2").GetValueAs<int>().Should().Be(2);

        }

        [Test]
        public void TestIntroduceData()
        {

            var list = new List<Expression>();

            list = IntroduceIntegers(list);

            list.Add(new Expression(00, "R_1", "R_2"));
            list.Add(new Expression(07, "Int_1", "R_1"));


            var bundle = RunHelper("Main", list);

            bundle.comp.Register.GetFromRegister("R_1").GetValueAs<string>().Should().Be("Main");
            bundle.comp.Register.GetFromRegister("R_2").GetValueAs<string>().Should().Be("1");

        }

        [Test]
        public void TestSwitch()
        {

            var list = new List<Expression>();

            list = IntroduceIntegers(list);

            list.Add(new Expression(18, "R_1", "R_2"));

            var bundle = RunHelper("Main", list);

            bundle.comp.Register.GetFromRegister("R_2").GetValueAs<int>().Should().Be(1);
            bundle.comp.Register.GetFromRegister("R_1").GetValueAs<int>().Should().Be(2);

        }

        [Test]
        public void TestJumpAndLink()
        {

            var list = new List<Expression>();
            list.Add(new Expression(19, "Side"));
            list.Add(new Expression(28, "Exit"));
            list.Add(new Expression(28, "Side"));
            list.Add(new Expression(01, "Int_1", "R_1"));
            list.Add(new Expression(20));

            var bundle = RunHelper("Main", list);

            bundle.comp.Register.GetFromRegister("R_1").GetValueAs<int>().Should().Be(1);
        }
    }
}
