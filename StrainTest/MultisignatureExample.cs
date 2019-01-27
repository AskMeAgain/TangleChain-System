using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using StrainLanguage;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace StrainLanguageTest
{
    [TestFixture]
    public class MultisignatureExample {
        private IXISettings _settings;

        [OneTimeSetUp]
        public void Init()
        {
            _settings = new IXISettings().Default(true);
        }

        [Test]
        public void MultisignatureTestWithoutArray()
        {

            var strain = new Strain(CodeWithoutArray);

            var list = strain.Compile();

            var stateDict = new Dictionary<string, ISCType>() {
                {"user0", new SC_String()},
                {"user1", new SC_String()},
                {"vote0", new SC_String()},
                {"vote1", new SC_String()},
                {"balance0", new SC_Int(0)},
                {"balance1", new SC_Int(0)}
            };

            var comp = new Computer(list, stateDict,_settings);

            var triggertrans = new Transaction("person1", 2, "pool")
                .AddFee(0)
                .AddData("Init")
                .AddData("person1")
                .AddData("person2")
                .Final(_settings);
            //init contract
            comp.Run(triggertrans);

            var state1 = comp.GetCompleteState();

            var comp2 = new Computer(list, state1,_settings);
            var triggertrans2 = new Transaction("person1", 2, "pool")
                .AddFee(0)
                .AddData("Vote")
                .AddData("person3")
                .AddData("Int_3")
                .Final(_settings);

            //vote with person 1
            comp2.Run(triggertrans2);
            var state2 = comp2.GetCompleteState();

            //doing a test vote to get the money out!
            var testTrigger = new Transaction("person2", 2, "pool")
                .AddFee(0)
                .AddData("Send")
                .Final(_settings);
            var shouldBeNull = comp2.Run(testTrigger);
            shouldBeNull.Should().BeNull();

            var comp3 = new Computer(list, state2,_settings);
            var triggertrans3 = new Transaction("person2", 2, "pool")
                .AddFee(0)
                .AddData("Vote")
                .AddData("person3")
                .AddData("Int_3")
                .Final(_settings);

            //vote with person 2
            comp3.Run(triggertrans3);
            var state3 = comp3.GetCompleteState();

            var comp4 = new Computer(list, state3,_settings);
            var triggertrans4 = new Transaction("person2", 2, "pool")
                .AddFee(0)
                .AddData("Send")
                .Final(_settings);

            //vote with person 2
            var result = comp4.Run(triggertrans4);

            result.OutputReceiver.Should().Contain("person3");
            result.OutputValue.Should().Contain(3);

        }

        [Test]
        public void MultisignatureTestWithArray()
        {

            var strain = new Strain(CodeWithArray);

            var stateDict = new Dictionary<string, ISCType>() {
                {"users_0", new SC_String()},
                {"users_1", new SC_String()},
                {"votes_0", new SC_String()},
                {"votes_1", new SC_String()},
                {"balances_0", new SC_Int(0)},
                {"balances_1", new SC_Int(0)}
            };

            var smart = strain.GenerateSmartcontract("you",_settings);

            var comp = new Computer(smart);

            var initTrans = new Transaction("person1", 2, "pool")
                .AddFee(0)
                .AddData("Init")
                .AddData("person0")
                .AddData("person1")
                .Final(_settings);

            var votePerson0 = new Transaction("person0", 2, "pool")
                .AddFee(0)
                .AddData("Vote")
                .AddData("person3")
                .AddData("Int_3")
                .Final(_settings);

            var votePerson1 = new Transaction("person1", 2, "pool")
                .AddFee(0)
                .AddData("Vote")
                .AddData("person3")
                .AddData("Int_3")
                .Final(_settings);

            var sendTrans = new Transaction("person1", 2, "pool")
                .AddFee(0)
                .AddData("Send")
                .Final(_settings);

            //init contract
            comp.Run(initTrans);
            var stateInit = comp.GetCompleteState();
            
            smart.ApplyState(stateInit);
            
            var vote1Comp = new Computer(smart);
            vote1Comp.Run(votePerson1);
            var vote1State = vote1Comp.GetCompleteState();

            smart.ApplyState(vote1State);

            var vote2Comp = new Computer(smart);
            vote2Comp.Run(votePerson0);
            var vote2State = vote2Comp.GetCompleteState();

            smart.ApplyState(vote2State);

            var sendComp = new Computer(smart);
            var result = sendComp.Run(sendTrans);

            result.Should().NotBeNull();

            result.OutputReceiver.Should().Contain("person3");
            result.OutputValue.Should().Contain(3);

        }

        private static readonly string CodeWithoutArray =

            "Multisignature {" +

            "state user0;" +
            "state user1;" +
            "state vote0;" +
            "state vote1;" +
            "state balance0;" +
            "state balance1;" +

            "entry Init(u1,u2){" +
            "vote0 = 0;" +
            "vote1 = 0;" +
            "user0 = u1;" +
            "user1 = u2;" +
            "}" +

            "entry Vote(to,balance){" +
            "var i = 0;" +
            "var index = -1;" +
            "if(user0 == _META[3]){" +
            "vote0 = to;" +
            "balance0 = balance;" +
            "}" +
            "if(user1 == _META[3]){" +
            "vote1 = to;" +
            "balance1 = balance;" +
            "}" +
            "}" +

            "entry Send(){" +
            "if(vote0 == vote1 && balance0 == balance1){" +
            "_OUT(balance0,vote1);" +
            "vote0 = 0;" +
            "vote1 = 0;" +
            "}" +
            "}" +
            "}";

        private static readonly string CodeWithArray =

            "Multisignature {" +

            "state users[2];" +
            "state votes[2];" +
            "state balances[2];" +

            "entry Init(u1,u2){" +
              "votes[0] = 0;" +
              "votes[1] = 0;" +
              "balances[0] = 0;" +
              "balances[1] = 0;" +
              "users[0] = u1;" +
              "users[1] = u2;" +
            "}" +

            "entry Vote(to,balance){" +

              "var i = 0;" +

              "while(i < _LENGTH(users)){" +
                "if(users[i] == _META[3]){" +
                   "votes[i] = to;" +
                   "balances[i] = balance;" +
                "}" +
                "i = i + 1;" +
              "}" +
            "}" +

            "entry Send(){" +
              "if(votes[0] == votes[1] && balances[0] == balances[1]){" +
                "_OUT(balances[0],votes[1]);" +
                "votes[0] = 0;" +
                "votes[1] = 0;" +
              "}" +
            "}" +
            "}";

    }
}
