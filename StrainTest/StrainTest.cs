using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using StrainLanguage;
using StrainLanguage.Classes;
using StrainLanguage.NodeClasses;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace StrainTest
{
    [TestFixture]
    [NonParallelizable]
    public class StrainTest
    {

        [OneTimeSetUp]
        public void Init()
        {
            IXISettings.Default(true);
        }

        private List<Expression> CreateExpressionList(string code)
        {
            var strain = new Strain(code);
            return strain.Compile();
        }

        [Test]
        [TestCase("int a,int b", 2)]
        [TestCase("int b", 1)]
        [TestCase("", 0)]
        public void ExpressionHelperTest01(string exp, int result)
        {

            string test = "function test(" + exp + "){";

            var expHelper = new ExpressionHelper(test);

            var list = expHelper.GetParameterNodeFromString();

            list.Count.Should().Be(result);

        }

        [Test]
        public void ExpressionHelperTest02()
        {

            string test = "if(x == a){";

            var expHelper = new ExpressionHelper(test);

            var question = expHelper.GetStringInBrackets();

            question.Should().Be("x == a");

        }

        [Test]
        [TestCase("1-1-1-1-1-0", "1-1-1-1-1")]
        public void ContextJumpTest(string context, string result)
        {

            var check = new ParserContext(context).OneContextUp();

            result.Should().Be(check.ToString());
        }

        [Test]
        public void SimpleAssignTest()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro Test2 = 3;" +
                "intro Test3 = Test2;" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);
            var result = comp.Run();

            comp.CheckRegister("Test2").GetValueAs<int>().Should().Be(3);
            comp.CheckRegister("Test3").GetValueAs<int>().Should().Be(3);

        }

        [Test]
        public void CommentTest()
        {

            var code = "Application {" +
                "entry Main() {" +
                "//Please ignore this one;" +
                "intro Test3 = 3; //also ignore this!;" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);
            var result = comp.Run();

            list.Count.Should().Be(5);
            comp.CheckRegister("Test3").GetValueAs<int>().Should().Be(3);

        }

        [Test]
        [TestCase("3 + 3", 6)]
        [TestCase("4 - 3", 1)]
        [TestCase("4 * 3", 12)]
        [TestCase("4 * 3 - 10", 2)]
        [TestCase("4 * 3 + 10", 22)]
        [TestCase("10 - 2 * 5", 0)]
        [TestCase("Test * 10 - 9", 1)]
        [TestCase("Test * Test - Test", 0)]
        public void SimpleMathTest02(string exp, int equals)
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro Test = 1;" +
                $"Test = {exp};" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);
            var result = comp.Run();

            comp.CheckRegister("Test").GetValueAs<int>().Should().Be(equals);

        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "Test5 is not in scope")]
        public void NotInScope()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro Test2 = 1;" +
                "if(0 == 0){" +
                "Test2 = 1111;" +
                "}else{" +
                "Test2 = 33;" +
                "intro Test5 = 1 + 1;" +
                "}" +
                "intro Test3 = Test2 + 1;" +
                "intro Test6 = Test5 + Test5;" +
                "}" +
                "}";

            CreateExpressionList(code);
        }

        [Test]
        public void IfSimpleTest01()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro comparer = 33;" +
                "if(0 == 0){" +
                "intro Test1 = 1;" +
                "}" +
                "if(0 != comparer){" +
                "intro Test2 = 1;" +
                "}" +
                "if(33 == comparer){" +
                "intro Test3 = 1;" +
                "}" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);
            var result = comp.Run();

            comp.CheckRegister("Test1").GetValueAs<int>().Should().Be(1);
            comp.CheckRegister("Test2").GetValueAs<int>().Should().Be(1);
            comp.CheckRegister("Test3").GetValueAs<int>().Should().Be(1);

        }

        [Test]
        public void IfNotTest()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro comparer = 33;" +
                "if(0 != 0){" +
                "intro Test1 = 1;" +
                "}else{" +
                "intro Test2 = 1;" +
                "}" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);
            var result = comp.Run();

            comp.CheckRegister("Test2").GetValueAs<int>().Should().Be(1);

        }

        [Test]
        public void StateTest()
        {

            var code = "Application {" +
                "var state;" +
                "entry Main() {" +
                "state = state + 1;" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });
            comp.Run();

            var state = comp.GetCompleteState();

            //we run again
            var comp2 = new Computer(list, state);
            comp2.Run();

            comp2.State.GetFromRegister("state").GetValueAs<int>().Should().Be(2);

        }

        [Test]
        public void FunctionTestSimple01()
        {

            var code = "Application {" +
                "function test(){" +
                "intro test1 = 3 + 3;" +
                "return 0;" +
                "}" +
                "entry Main() {" +
                "test();" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });
            comp.Run();

            comp.CheckRegister("test1").GetValueAs<int>().Should().Be(6);

        }

        [Test]
        [TestCase(1, 1, 2)]
        [TestCase(11, 1, 12)]
        [TestCase(10, 10, 20)]
        [TestCase(0, 1, 1)]
        public void FunctionTestSimple02(int a, int b, int result)
        {

            var code = "Application {" +
                "function test(test1, test2){" +
                "return test1 + test2;" +
                "}" +
                "entry Main() {" +
                $"intro var = test({a},{b});" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });
            comp.Run();

            comp.CheckRegister("var").GetValueAs<int>().Should().Be(result);


        }

        [Test]
        public void ArrayTest()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro array[3];" +
                "array[0] = 3;" +
                "intro test = array[0] + 3;" +

                "intro index = 0;" +
                "intro recursive = array[index];" +
                "array[index] = index;" +
                "index = index + 1;" +
                "array[index] = index;" +
                "index = index + 1;" +
                "array[index] = index;" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });
            comp.Run();

            comp.CheckRegister("array_0").GetValueAs<int>().Should().Be(0);
            comp.CheckRegister("array_1").GetValueAs<int>().Should().Be(1);
            comp.CheckRegister("array_2").GetValueAs<int>().Should().Be(2);
            comp.CheckRegister("test").GetValueAs<int>().Should().Be(6);
            comp.CheckRegister("recursive").GetValueAs<int>().Should().Be(3);

        }

        [Test]
        public void OutTransactionTest()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro array = 3;" +
                "_OUT(array,\"LOL\");" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });
            var result = comp.Run();

            result.OutputReceiver.Should().Contain("LOL");
            result.OutputValue.Should().Contain(3);

        }

        [Test]
        public void TestMetaDataAccess()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro array = _META[3];" +
                "if(_META[3] == \"From\"){" +
                "intro test = 1;" +
                "}" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });
            var triggerTrans = new Transaction("From", 2, "PoolAddress")
                .AddFee(0)
                .AddData("Main")
                .Final();

            comp.Run(triggerTrans);

            comp.CheckRegister("array").GetValueAs<string>().Should().Be("From");
            comp.CheckRegister("test").GetValueAs<int>().Should().Be(1);

        }

        [Test]
        public void TestDataAccess()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro array = _DATA[2];" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });
            var triggerTrans = new Transaction("From", 2, "PoolAddress")
                .AddFee(0)
                .AddData("Main")
                .AddData("CALLER")
                .Final();

            comp.Run(triggerTrans);

            comp.CheckRegister("array").GetValueAs<string>().Should().Be("CALLER");

        }

        [Test]
        public void TestOr()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro array = 3;" +
                "if(1 == 2 || 1 == 1){" +
                "intro test = 1;" +
                "}" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            comp.Run();

            comp.CheckRegister("test").GetValueAs<int>().Should().Be(1);

        }
        [Test]
        public void TestSmallerBigger()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro array = 3;" +
                "if(1 < 2){" +
                "intro test1 = 1;" +
                "}" +
                "if(2 > 1){" +
                "intro test2 = 1;" +
                "}" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            comp.Run();

            comp.CheckRegister("test1").GetValueAs<int>().Should().Be(1);
            comp.CheckRegister("test2").GetValueAs<int>().Should().Be(1);

        }

        [Test]
        public void TestEqualSmallerBigger()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro array = 3;" +
                "if(1 <= 1){" +
                "intro test1 = 1;" +
                "}" +
                "if(1 <= 2){" +
                "intro test2 = 1;" +
                "}" +
                "if(2 >= 2){" +
                "intro test3 = 1;" +
                "}" +
                "if(3 >= 2){" +
                "intro test4 = 1;" +
                "}" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            comp.Run();

            comp.CheckRegister("test1").GetValueAs<int>().Should().Be(1);
            comp.CheckRegister("test2").GetValueAs<int>().Should().Be(1);
            comp.CheckRegister("test3").GetValueAs<int>().Should().Be(1);
            comp.CheckRegister("test4").GetValueAs<int>().Should().Be(1);

        }

        [Test]
        public void TestArrayLength()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro array[3];" +
                "intro length = _LENGTH(array);" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            comp.Run();

            comp.CheckRegister("length").GetValueAs<int>().Should().Be(3);


        }

        [Test]
        public void TestFunctionReturn()
        {

            var code = "Application {" +
                "function test(){" +
                "return 3;" +
                "}" +
                "entry Main() {" +
                "intro length = test();" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            comp.Run();

            comp.CheckRegister("length").GetValueAs<int>().Should().Be(3);

        }

        [Test]
        public void FunctionStringTest()
        {

            var code = "Application {" +
                "function test(multiplier){" +
                "return \"x\" * multiplier;" +
                "}" +
                "entry Main() {" +
                "intro length = test(3);" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            comp.Run();

            comp.CheckRegister("length").GetValueAs<string>().Should().Be("xxx");

        }

        [Test]
        public void EntryParaMeterTest()
        {

            var code = "Application {" +
                "entry Main(length,stuff) {" +
                "intro var = length;" +
                "intro result = stuff * var;" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            var triggerTrans = new Transaction("you", 2, "pool")
                .AddFee(0)
                .AddData("Main")
                .AddData("3")
                .AddData("x")
                .Final();

            comp.Run(triggerTrans);

            comp.CheckRegister("var").GetValueAs<int>().Should().Be(3);
            comp.CheckRegister("result").GetValueAs<string>().Should().Be("xxx");

        }

        [Test]
        public void WhileLoopTest()
        {

            var code = "Application {" +
                "entry Main() {" +

                "intro array[3];" +
                "intro i = -1;" +

                "while(i < 3){" +
                  "i = i + 1;" +
                  "array[i] = 2;" +
                "}" +

                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list);

            comp.Run();

            //   comp.CheckRegister("i").GetValueAs<int>().Should().Be(13);
            comp.CheckRegister("array_0").GetValueAs<int>().Should().Be(2);
            comp.CheckRegister("array_1").GetValueAs<int>().Should().Be(2);
            comp.CheckRegister("array_2").GetValueAs<int>().Should().Be(2);
            //comp.CheckRegister("array_4").GetValueAs<int>().Should().Be(3);
            // comp.CheckRegister("array_29").GetValueAs<int>().Should().Be(3);

        }

        [Test]
        [ExpectedNoException]
        public void VoidLengthTest()
        {

            var code = "Application {" +
                "entry Main() {" +
                "intro array[3];" +
                "_LENGTH(array);" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            comp.Run();

        }

        [Test]
        public void SameVarNameTest()
        {

            var code = "Application {" +
                "function test(){" +
                "intro test = 3;" +
                "return test + 3;" +
                "}" +
                "entry Main() {" +
                "intro test = test();" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            comp.Run();

            var result = comp.CheckRegisterCollection("test");

            result[0].GetValueAs<int>().Should().Be(3);
            result[1].GetValueAs<int>().Should().Be(6);

        }

        [Test]
        public void AndTest()
        {

            var code = "Application {" +
                "entry Main() {" +
                "if(1 == 1 && 1 == 1){" +
                "intro test = 3;" +
                "}" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var comp = new Computer(list, new Dictionary<string, ISCType>() { { "state", new SC_Int(0) } });

            comp.Run();

            comp.CheckRegister("test").GetValueAs<int>().Should().Be(3);

        }

        [Test]
        public void StateArrayTest()
        {

            var code = "Application {" +
                "var array[2];" +
                "entry Main() {" +
                "intro index = 0;" +
                "array[index] = 2;" +
                "}" +
                "}";

            var list = CreateExpressionList(code);

            var stateDict = new Dictionary<string, ISCType>() {
                { "array_0", new SC_Int(0) },
                { "array_1", new SC_Int(0) }
            };

            var comp = new Computer(list, stateDict);

            comp.Run();

            var state = comp.GetCompleteState();
            state["array_0"].GetValueAs<int>().Should().Be(2);

        }
    }
}
