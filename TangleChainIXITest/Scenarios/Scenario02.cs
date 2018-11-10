using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI;
using TangleChainIXI.Classes;
using FluentAssertions;
using System.Linq;

namespace TangleChainIXITest.Scenarios
{
    [TestFixture]
    public class Scenario02
    {
        public string coinName = "smart_test" + Utils.GenerateRandomInt(5);

        public Smartcontract CreateSmartcontract(string name, string sendto)
        {

            Smartcontract smart = new Smartcontract(name, sendto);
            smart.SetFee(1);
            smart.ReceivingAddress = Utils.GenerateRandomString(81);

            smart.AddVariable("Counter")

                .AddExpression(05, "PayIn")
                .AddExpression(07, "Int_2", "R_0")

                //we add one to counter
                .AddExpression(10, "Counter", "R_1")
                .AddExpression(01, "Int_1", "R_3")
                .AddExpression(03, "R_1", "R_3", "R_2")
                .AddExpression(06, "R_2", "Counter")

                //set out transaction
                .AddExpression(09, "R_0", "R_3")
                .AddExpression(05, "Exit");

            return smart;

        }

        [Test]
        public void TestSmartcontract()
        {

            Smartcontract smart = CreateSmartcontract("name", Utils.GenerateRandomString(81));

            Transaction trans = new Transaction("0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF", -1, Utils.GenerateRandomString(81)); //secure 1
            trans.AddFee(0)
                .AddData("PayIn")
                .AddData("Str_0xFe84b71404D9217522a619658E829CaABa397A20") //secure 2
                .AddOutput(100, "you")
                .Final();

            Computer comp = new Computer(smart);

            var result = comp.Run(trans);

            result.OutputValue[0].Should().Be(1);

            var varList = comp.GetCompleteState().Code.Variables;

            varList.Select(x => x.Name).Should().Contain("Counter");
            varList.Select(x => x.Value).Should().Contain("Int_1");

        }

        [Test]
        public void Scenario()
        {
            //set information
            IXISettings.Default(true);
            IXISettings.SetPrivateKey("secure2");
            int startDifficulty = 7;

            //we need to create chainsettings first!
            ChainSettings cSett = new ChainSettings(1000, 0, 0, 2, 30, 1000, 3);
            DBManager.SetChainSettings(coinName, cSett);

            string poolAddr = Utils.GetTransactionPoolAddress(1, coinName);

            //create genesis transaction
            Transaction genTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, coinName));
            genTrans.SetGenesisInformation(cSett)
                .Final()
                .Upload();

            //create genesis block
            Block genBlock = new Block(0, Utils.GenerateRandomString(81), coinName);
            genBlock.Add(genTrans)
                .Final()
                .GenerateProofOfWork(startDifficulty)
                .Upload();

            Console.WriteLine("=============================================================\n\n");
            //now creating block height 1

            //upload simple transaction on 1. block
            Transaction simpleTrans = new Transaction(IXISettings.PublicKey, 1, poolAddr);
            simpleTrans.AddFee(0)
                .Final()
                .Upload();

            //add smartcontract
            Smartcontract smart = CreateSmartcontract("cool contract", poolAddr);
            smart.Final()
                .Upload();

            //block 1
            Block block1 = Block1(coinName, genBlock, simpleTrans, smart);

            Console.WriteLine("=============================================================\n\n");

            //now creating second block to trigger stuff!
            Transaction triggerTrans = new Transaction(IXISettings.PublicKey, 2, poolAddr);

            triggerTrans.AddFee(0)
                .AddOutput(100, smart.ReceivingAddress)
                .AddData("PayIn")
                .AddData("Str_0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF")
                .Final()
                .Upload();

            Block block2 = new Block(2, block1.NextAddress, coinName);

            block2.Add(triggerTrans)
                .Final()
                .GenerateProofOfWork()
                .Upload();

            //now we add another block and trigger smartcontract again!
            //first create transaction
            Transaction triggerTrans2 = new Transaction(IXISettings.PublicKey, 2, poolAddr);
            triggerTrans2.AddFee(0)
                .AddOutput(100, smart.ReceivingAddress)
                .AddData("PayIn")
                .AddData("Str_0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF")
                .Final()
                .Upload();

            Block block3 = new Block(3, block2.NextAddress, coinName);

            block3.Add(triggerTrans2)
                .Final()
                .GenerateProofOfWork()
                .Upload();

            //NOW STATE S_counter SHOULD BE __2
            var latest = Core.DownloadChain(coinName, genBlock.SendTo, genBlock.Hash, true, null);

            latest.Should().Be(block3);

            var smartcontract = DBManager.GetSmartcontract(coinName, smart.ReceivingAddress);

            Console.WriteLine("Coinname: " + coinName);
            smartcontract.Code.Variables.Select(x => x.Value).Should().Contain("Int_2");

            DBManager.GetBalance(coinName, smart.ReceivingAddress).Should().Be(198);
            DBManager.GetBalance(coinName, "0x14D57d59E7f2078A2b8dD334040C10468D2b5ddF").Should().Be(2);

        }

        private Block Block1(string coinName, Block blockBefore, Transaction simpleTrans, Smartcontract smart)
        {

            Block Block = new Block(blockBefore.Height + 1, blockBefore.NextAddress, coinName);

            Block.Add(simpleTrans)
                .Add(smart)
                .Final()
                .GenerateProofOfWork()
                .Upload();

            return Block;
        }

    }
}
