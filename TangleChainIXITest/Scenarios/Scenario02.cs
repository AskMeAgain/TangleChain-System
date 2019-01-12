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
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXITest.Scenarios
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class Scenario02
    {
        public string _coinName = "smart_test" + Utils.GenerateRandomInt(5);

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
        public void Scenario()
        {

            //we need to create chainsettings first!
            ChainSettings cSett = new ChainSettings(1000, 0, 0, 2, 30, 1000, 5);


            //create genesis transaction
            Transaction genTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, _coinName));
            genTrans.SetGenesisInformation(cSett)
                .Final()
                .Upload();

            //create genesis block
            Block genBlock = new Block(0, Utils.GenerateRandomString(81), _coinName);
            genBlock.Add(genTrans)
                .Final()
                .GenerateProofOfWork(_ixiCore)
                .Upload();

            var result0 = _ixiCore.DownloadChain(genBlock.SendTo, genBlock.Hash);

            result0.Should().NotBeNull();
            result0.Should().Be(genBlock);

            Console.WriteLine("=============================================================\n\n");
            //now creating block height 1

            //get pool addr
            string poolAddr = Utils.GetTransactionPoolAddress(1, _coinName, cSett.TransactionPoolInterval);

            //upload simple transaction on 1. block
            Transaction simpleTrans = new Transaction(IXISettings.PublicKey, 1, poolAddr);
            simpleTrans.AddFee(0)
                .Final()
                .Upload();

            //add smartcontract
            Smartcontract smart = CreateSmartcontract("cool contract", poolAddr)
                .Final()
                .Upload();

            Block block1 = new Block(genBlock.Height + 1, genBlock.NextAddress, _coinName);

            block1.Add(simpleTrans)
                .Add(smart)
                .Final()
                .GenerateProofOfWork(_ixiCore)
                .Upload();

            var result1 = _ixiCore.DownloadChain(block1.SendTo, block1.Hash);
            result1.Should().NotBeNull();
            result1.Should().Be(block1);

            Console.WriteLine("=============================================================\n\n");

            //now creating second block to trigger stuff!
            Transaction triggerTrans = new Transaction(IXISettings.PublicKey, 2, poolAddr);

            triggerTrans.AddFee(0)
                .AddOutput(100, smart.ReceivingAddress)
                .AddData("PayIn")
                .AddData("Str_0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF")
                .Final()
                .Upload();

            Block block2 = new Block(2, block1.NextAddress, _coinName);

            block2.Add(triggerTrans)
                .Final()
                .GenerateProofOfWork(_ixiCore)
                .Upload();

            var result2 = _ixiCore.DownloadChain(block2.SendTo, block2.Hash);
            result2.Should().NotBeNull();
            result2.Should().Be(block2);

            Console.WriteLine("=============================================================\n\n");

            //now we add another block and trigger smartcontract again!
            //first create transaction
            Transaction triggerTrans2 = new Transaction(IXISettings.PublicKey, 2, poolAddr);
            triggerTrans2.AddFee(0)
                .AddOutput(100, smart.ReceivingAddress)
                .AddData("PayIn")
                .AddData("Str_0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF")
                .Final()
                .Upload();

            Block block3 = new Block(3, block2.NextAddress, _coinName);

            block3.Add(triggerTrans2)
                .Final()
                .GenerateProofOfWork(_ixiCore)
                .Upload();

            var latest = _ixiCore.DownloadChain(block3.SendTo, block3.Hash);
            latest.Should().NotBeNull();
            latest.Should().Be(block3);

            Console.WriteLine("=============================================================\n\n");

            var smartcontract = _ixiCore.GetSmartcontract(smart.ReceivingAddress);

            smartcontract.Should().NotBeNull();

            Console.WriteLine("Coinname: " + _coinName);
            Console.WriteLine("GenesisAddress: " + genBlock.SendTo);
            Console.WriteLine("GenesisHash: " + genBlock.Hash);


            smartcontract.Code.Variables.Values.Select(x => x.GetValueAs<string>()).Should().Contain("2");

            _ixiCore.GetBalance(smart.ReceivingAddress).Should().Be(198);
            _ixiCore.GetBalance("0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF").Should().Be(2);

        }
    }
}
