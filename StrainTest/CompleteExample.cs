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
    public class CompleteExample
    {
        [OneTimeSetUp]
        public void Init() {
            IXISettings.Default(true);
        }

        [Test]
        public void Multisig()
        {

            string code = 
                
                "Multisignature {" +

                    "var user0;" +
                    "var user1;" +
                    "var vote0;" +
                    "var vote1;" +
                    "var balance0;"+
                    "var balance1;"+

                    "entry Init(u1,u2){" +
                      "vote0 = 0;" +
                      "vote1 = 0;" +
                      "user0 = u1;" +
                      "user1 = u2;" +
                    "}" +

                    "entry Vote(to,balance){" +
                      "intro i = 0;" +
                      "intro index = -1;" +
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
                      "if(vote0 == vote1){" +
                        "if(balance0 == balance1){" +
                          "_OUT(balance0,vote1);" +
                          "vote0 = 0;"+
                          "vote1 = 0;"+
                        "}" +
                      "}" +
                    "}" +
                "}";

            var strain = new Strain(code);

            var list = strain.Compile();

            var stateDict = new Dictionary<string, ISCType>() {
                {"user0", new SC_String()},
                {"user1", new SC_String()},
                {"vote0", new SC_String()},
                {"vote1", new SC_String()},
                {"balance0", new SC_Int(0)},
                {"balance1", new SC_Int(0)}
            };

            var comp = new Computer(list,stateDict);

            var triggertrans = new Transaction("person1", 2, "pool")
                .AddFee(0)
                .AddData("Init")
                .AddData("person1")
                .AddData("person2")
                .Final();
            //init contract
            comp.Run(triggertrans);

            var state1 = comp.GetCompleteState();

            var comp2 = new Computer(list, state1);
            var triggertrans2 = new Transaction("person1", 2, "pool")
                .AddFee(0)
                .AddData("Vote")
                .AddData("person3")
                .AddData("Int_3")
                .Final();

            //vote with person 1
            comp2.Run(triggertrans2);
            var state2 = comp2.GetCompleteState();

            //doing a test vote to get the money out!
            var testTrigger = new Transaction("person2", 2, "pool")
                .AddFee(0)
                .AddData("Send")
                .Final();
            var shouldBeNull = comp2.Run(testTrigger);
            shouldBeNull.Should().BeNull();

            var comp3 = new Computer(list, state2);
            var triggertrans3 = new Transaction("person2", 2, "pool")
                .AddFee(0)
                .AddData("Vote")
                .AddData("person3")
                .AddData("Int_3")
                .Final();

            //vote with person 2
            comp3.Run(triggertrans3);
            var state3 = comp3.GetCompleteState();

            var comp4 = new Computer(list, state3);
            var triggertrans4 = new Transaction("person2", 2, "pool")
                .AddFee(0)
                .AddData("Send")
                .Final();

            //vote with person 2
            var result = comp4.Run(triggertrans4);

            result.OutputReceiver.Should().Contain("person3");
            result.OutputValue.Should().Contain(3);

        }
    }
}
