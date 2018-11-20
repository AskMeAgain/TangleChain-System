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
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXITest.Scenarios
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class Scenario02
    {
        public string coinName = "smart_test" + Utils.GenerateRandomInt(5);
        private List<Task> taskList = new List<Task>();

        [OneTimeSetUp]
        public void Init()
        {
            IXISettings.Default(true);
            IXISettings.SetPrivateKey("secure2");
        }

        private Smartcontract CreateSmartcontract(string name, string sendto)
        {

            Smartcontract smart = new Smartcontract(name, sendto);
            smart.SetFee(1);
            smart.ReceivingAddress = Utils.GenerateRandomString(81);

            smart.AddVariable("Counter", new SC_Int(0))

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

            return smart;

        }

        [Test, Order(1)]
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

        [Test, Order(2)]
        public async Task TestDownloadSmartcontractAsync()
        {

            //string addr = "9BHIGLLGESKWFXLEAPNK9NV9UZOKFYNFQDNLYJRHGRUCDMCPXFIQYFAN9WKE9WQTTAGZCTGGGULVDFNTTWGILQFMHD";

            var task = CreateSmartcontract("lol", Utils.GenerateRandomString(81)).Final().UploadAsync();

            var smart = await task;

            Console.WriteLine(smart.SendTo);

            var result = Core.GetSpecificFromAddress<Smartcontract>(smart.SendTo, smart.Hash);

            result.Should().Be(smart);

        }

        [Test, Order(3)]
        public void Scenario()
        {
            //set information
            int startDifficulty = 7;

            //we need to create chainsettings first!
            ChainSettings cSett = new ChainSettings(1000, 0, 0, 2, 30, 1000, 5);
            DBManager.SetChainSettings(coinName, cSett);

            string poolAddr = Utils.GetTransactionPoolAddress(1, coinName);

            //create genesis transaction
            Transaction genTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, coinName));
            genTrans.SetGenesisInformation(cSett)
                .Final();

            //var task = ;

            taskList.Add(genTrans.UploadAsync());

            //create genesis block
            Block genBlock = new Block(0, Utils.GenerateRandomString(81), coinName);
            genBlock.Add(genTrans)
                .Final()
                .GenerateProofOfWork(startDifficulty);
            taskList.Add(genBlock.UploadAsync());

            Console.WriteLine("=============================================================\n\n");
            //now creating block height 1

            //upload simple transaction on 1. block
            Transaction simpleTrans = new Transaction(IXISettings.PublicKey, 1, poolAddr);
            simpleTrans.AddFee(0)
                .Final();
            taskList.Add(simpleTrans.UploadAsync());

            //add smartcontract
            Smartcontract smart = CreateSmartcontract("cool contract", poolAddr).Final();
            taskList.Add(smart.UploadAsync());

            //block 1
            Block block1 = Block1(coinName, genBlock, simpleTrans, smart);

           Console.WriteLine("=============================================================\n\n");

            //now creating second block to trigger stuff!
            Transaction triggerTrans = new Transaction(IXISettings.PublicKey, 2, poolAddr);

            triggerTrans.AddFee(0)
                .AddOutput(100, smart.ReceivingAddress)
                .AddData("PayIn")
                .AddData("Str_0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF")
                .Final();
            taskList.Add(triggerTrans.UploadAsync());

            Block block2 = new Block(2, block1.NextAddress, coinName);

            block2.Add(triggerTrans)
                .Final()
                .GenerateProofOfWork();
            taskList.Add(block2.UploadAsync());

            //now we add another block and trigger smartcontract again!
            //first create transaction
            Transaction triggerTrans2 = new Transaction(IXISettings.PublicKey, 2, poolAddr);
            triggerTrans2.AddFee(0)
                .AddOutput(100, smart.ReceivingAddress)
                .AddData("PayIn")
                .AddData("Str_0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF")
                .Final();
            taskList.Add(triggerTrans2.UploadAsync());

            Block block3 = new Block(3, block2.NextAddress, coinName);

            block3.Add(triggerTrans2)
                .Final()
                .GenerateProofOfWork();
            taskList.Add(block3.UploadAsync());

            //we now wait for all tasks to complete
            Task.WaitAll(taskList.ToArray());

            //NOW STATE Counter SHOULD BE Int_2
            var latest = Core.DownloadChain(coinName, genBlock.SendTo, genBlock.Hash, null);

            latest.Should().Be(block3);

            var smartcontract = DBManager.GetSmartcontract(coinName, smart.ReceivingAddress);

            smartcontract.Should().NotBeNull();

            Console.WriteLine("Coinname: " + coinName);


            smartcontract.Code.Variables.Values.Select(x => x.GetValueAs<string>()).Should().Contain("2");

            DBManager.GetBalance(coinName, smart.ReceivingAddress).Should().Be(198);
            DBManager.GetBalance(coinName, "0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF").Should().Be(2);

        }

        private Block Block1(string coinName, Block blockBefore, Transaction simpleTrans, Smartcontract smart)
        {

            Block Block = new Block(blockBefore.Height + 1, blockBefore.NextAddress, coinName);

            Block.Add(simpleTrans)
                .SetDifficulty(DBManager.GetDifficulty(coinName,Block.Height))
                .Add(smart)
                .Final()
                .GenerateProofOfWork();

            taskList.Add(Block.UploadAsync());

            return Block;
        }

    }
}
