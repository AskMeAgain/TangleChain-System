using System;
using System.Collections.Generic;
using NUnit.Framework;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Classes;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using IXIComponents.Simple;
using TangleChainIXI.Classes.Helper;
using TangleChainIXI.Smartcontracts.Classes;

namespace IntegrationTest
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class Scenario01
    {

        private static IXISettings _settings;
        private static string _coinName;

        private static IEnumerable<TestCaseData> IXICores()
        {
            _settings = new IXISettings().Default(true);
            _coinName = "smart_test" + Utils.GenerateRandomInt(5);

            yield return new TestCaseData((null as IXICore).SimpleSetup(_coinName, _settings));

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
                .Final(_settings);

            Computer comp = new Computer(smart);

            var maybe = comp.Run(trans);

            maybe.HasValue.Should().BeTrue();
            var result = maybe.Value;

            result.OutputValue[0].Should().Be(1);
            result.OutputReceiver[0].Should().Be(receiverAddr);

            var varList = comp.GetCompleteState();

            varList.GetFromRegister("Counter").GetValueAs<int>().Should().Be(1);

        }

        [Test, TestCaseSource("IXICores")]
        public async Task Scenario(IXICore ixiCore)
        {

            //we need to create chainsettings first!
            ChainSettings cSett = new ChainSettings(1000, 0, 0, 2, 30, 1000, 5);


            //create genesis transaction
            Transaction genTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, _coinName));
            genTrans.SetGenesisInformation(cSett)
                .Final(_settings)
                .Upload();

            //create genesis block
            Block genBlock = new Block(0, Utils.GenerateRandomString(81), _coinName);
            genBlock.Add(genTrans)
                .Final(_settings)
                .GenerateProofOfWork(ixiCore)
                .Upload();

            var result0 = ixiCore.DownloadChain(genBlock.SendTo, genBlock.Hash);

            result0.Should().NotBeNull();
            result0.Should().Be(genBlock);

            Console.WriteLine("=============================================================\n\n");
            //now creating block height 1

            //get pool addr
            string poolAddr = Utils.GetTransactionPoolAddress(1, _coinName, cSett.TransactionPoolInterval);

            //upload simple transaction on 1. block
            Transaction simpleTrans = new Transaction(_settings.PublicKey, 1, poolAddr);
            simpleTrans.AddFee(0)
                .Final(_settings)
                .Upload();

            //add smartcontract
            Smartcontract smart = CreateSmartcontract("cool contract", poolAddr)
                .Final(_settings)
                .Upload();

            Block block1 = new Block(genBlock.Height + 1, genBlock.NextAddress, _coinName);

            block1.Add(simpleTrans)
                .Add(smart)
                .Final(_settings)
                .GenerateProofOfWork(ixiCore)
                .Upload();

            var result1 = ixiCore.DownloadChain(block1.SendTo, block1.Hash);
            result1.Should().NotBeNull();
            result1.Should().Be(block1);

            Console.WriteLine("=============================================================\n\n");

            //now creating second block to trigger stuff!
            Transaction triggerTrans = new Transaction(_settings.PublicKey, 2, poolAddr);

            triggerTrans.AddFee(0)
                .AddOutput(100, smart.ReceivingAddress)
                .AddData("PayIn")
                .AddData("Str_0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF")
                .Final(_settings)
                .Upload();

            Block block2 = new Block(2, block1.NextAddress, _coinName);

            block2.Add(triggerTrans)
                .Final(_settings)
                .GenerateProofOfWork(ixiCore)
                .Upload();

            var result2 = ixiCore.DownloadChain(block2.SendTo, block2.Hash);
            result2.Should().NotBeNull();
            result2.Should().Be(block2);

            Console.WriteLine("=============================================================\n\n");

            //now we add another block and trigger smartcontract again!
            //first create transaction
            Transaction triggerTrans2 = new Transaction(_settings.PublicKey, 2, poolAddr);
            triggerTrans2.AddFee(0)
                .AddOutput(100, smart.ReceivingAddress)
                .AddData("PayIn")
                .AddData("Str_0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF")
                .Final(_settings)
                .Upload();

            Block block3 = new Block(3, block2.NextAddress, _coinName);

            block3.Add(triggerTrans2)
                .Final(_settings)
                .GenerateProofOfWork(ixiCore)
                .Upload();

            var latest = ixiCore.DownloadChain(block3.SendTo, block3.Hash);
            latest.Should().NotBeNull();
            latest.Should().Be(block3);

            Console.WriteLine("=============================================================\n\n");

            var maybeSmartcontract = ixiCore.GetSmartcontract(smart.ReceivingAddress);

            Console.WriteLine("Coinname: " + _coinName);
            Console.WriteLine("GenesisAddress: " + genBlock.SendTo);
            Console.WriteLine("GenesisHash: " + genBlock.Hash);

            maybeSmartcontract.HasValue.Should().BeTrue();
            maybeSmartcontract.Value.Code.Variables.Values.Select(x => x.GetValueAs<string>()).Should().Contain("2");

            ixiCore.GetBalance(smart.ReceivingAddress).Should().Be(198);
            ixiCore.GetBalance("0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF").Should().Be(2);

        }
    }
}
