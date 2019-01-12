using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI;
using TangleChainIXI.Classes;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using TangleChainIXI.Interfaces;
using TangleChainIXI.NewClasses;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXITest.Scenarios
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class Scenario02
    {
        public string _coinName = "smart_test" + Utils.GenerateRandomInt(5);
        private List<Task> taskList = new List<Task>();

        private IXICore _ixiCore;
        private ITangleAccessor _tangleAccessor;

        [OneTimeSetUp]
        public void Init()
        {
            IXISettings.Default(true);
            IXISettings.SetPrivateKey("secure2");

            _ixiCore = IXICore.SimpleSetup(_coinName);
        }

        private Smartcontract CreateSmartcontract(string name, string sendto)
        {

            return new Smartcontract(name, sendto)
                .SetFee(1)
                .SetReceivingAddress(Utils.GenerateRandomString(81))

                .AddVariable("Counter", new SC_Int(0))

                .AddExpression(05, "PayIn")
                .AddExpression(15, "Int_2", "R_0") //loads Data[2] into R_0

                //we add one to counter
                .AddExpression(10, "Counter", "R_1") //copies counter to R_1
                .AddExpression(01, "Int_1", "R_3") //introduces 1 to R_3
                .AddExpression(03, "R_1", "R_3", "R_2") //adds R_1 and R_3 together
                .AddExpression(06, "R_2", "Counter") //writes R_3 into counter

                //set out transaction
                .AddExpression(09, "R_0", "R_3")
                .AddExpression(05, "Exit");

        }

        [Test]
        public void TestSmartcontract()
        {

            Smartcontract smart = CreateSmartcontract("name", Utils.GenerateRandomString(81));

            Transaction trans = new Transaction("0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF", -1, Utils.GenerateRandomString(81)); //secure 1
            var receiverAddr = "0xFe84b71404D9217522a619658E829CaABa397A20";
            trans.AddFee(0)
                .AddData("PayIn")
                .AddData("Str_" + receiverAddr) //secure 2
                .AddOutput(100, "you")
                .Final();

            Computer comp = new Computer(smart);

            var result = comp.Run(trans);

            result.OutputValue[0].Should().Be(1);
            result.OutputReceiver[0].Should().Be(receiverAddr);

            var varList = comp.GetCompleteState();

            varList.GetFromRegister("Counter").GetValueAs<int>().Should().Be(1);

        }

        [Test]
        public async Task TestDownloadSmartcontractAsync()
        {

            var task = CreateSmartcontract("lol", Utils.GenerateRandomString(81)).Final().UploadAsync();

            var smart = await task;

            Console.WriteLine(smart.SendTo);

            var result = _tangleAccessor.GetSmartcontract(smart.Hash, smart.SendTo);

            result.Should().Be(smart);

        }

    }
}
