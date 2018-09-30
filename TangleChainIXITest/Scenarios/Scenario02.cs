using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI;
using TangleChainIXI.Classes;
using FluentAssertions;

namespace TangleChainIXITest.Scenarios
{
    [TestFixture]
    public class Scenario02
    {
        public string coinName = "smart_test" + Utils.GenerateRandomInt(5);

        public Smartcontract CreateSmartcontract(string name, string sendto)
        {

            Smartcontract smart = new Smartcontract(name, sendto);
            smart.ReceivingAddress = Utils.GenerateRandomString(81);

            smart.Code.AddExpression(new Expression(05, "PayIn"));
            smart.Code.AddExpression(new Expression(06, "T_4", "R_0"));
            smart.Code.AddExpression(new Expression(09, "R_0", "__1"));
            smart.Code.AddExpression(new Expression(05, "Exit"));

            return smart;

        }

        [Test]
        public void TestSmartcontract()
        {

            Smartcontract smart = CreateSmartcontract("name", Utils.GenerateRandomString(81));

            Transaction trans = new Transaction("ME", -1, Utils.GenerateRandomString(81));
            trans.AddFee(0);
            trans.Data.Add("PayIn");
            trans.AddOutput(100, "you");
            trans.Final();

            Computer comp = new Computer(smart);

            var result = comp.Run(trans);

            result.OutputValue[0].Should().Be(1);

        }

        [Test]
        public void Scenario()
        {


            //set information
            IXISettings.Default(true);
            Difficulty startDifficulty = new Difficulty(7);

            //we need to create chainsettings first!
            ChainSettings cSett = new ChainSettings(1000, 0, 0, 2, 30, 1000, 3);
            DBManager.SetChainSettings(coinName, cSett);

            string poolAddr = Utils.GetTransactionPoolAddress(1, coinName);

            //create genesis transaction


            Transaction genTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, coinName));
            genTrans.SetGenesisInformation(cSett);
            genTrans.Final();
            genTrans.Upload();

            //create genesis block
            Block genBlock = new Block(0, Utils.GenerateRandomString(81), coinName);
            genBlock.AddTransactions(genTrans);
            genBlock.Final();
            genBlock.GenerateProofOfWork(startDifficulty);
            genBlock.Upload();

            Console.WriteLine("=============================================================\n\n");
            //now creating block height 1

            //upload simple transaction on 1. block
            Transaction simpleTrans = new Transaction(IXISettings.PublicKey, 1, poolAddr);
            simpleTrans.AddFee(0);
            simpleTrans.Final();
            simpleTrans.Upload();

            //add smartcontract
            Smartcontract smart = CreateSmartcontract("cool contract", poolAddr);
            smart.Final();
            smart.Upload();

            //block 1
            Block block1 = Block1(coinName, genBlock,simpleTrans,smart);
            block1.Print();

            Console.WriteLine("=============================================================\n\n");

            Block latest = Core.DownloadChain(coinName, genBlock.SendTo, genBlock.Hash, true, true, null);
            Smartcontract downloadedSmart = DBManager.GetSmartcontract(coinName, smart.ReceivingAddress);

            //testing the system now!
            latest.Should().Be(block1);
            downloadedSmart.Should().Be(smart);

            //

        }

        private Block Block1(string coinName, Block blockBefore, Transaction simpleTrans, Smartcontract smart)
        {

            Block Block = new Block(blockBefore.Height + 1, blockBefore.NextAddress, coinName);

            Block.AddTransactions(simpleTrans);
            Block.AddSmartcontract(smart);

            Block.Final();
            Block.GenerateProofOfWork(DBManager.GetDifficulty(coinName, 1));
            Block.Upload();

            bool result = Cryptography.VerifyHashAndNonceAgainstDifficulty(Block, DBManager.GetDifficulty(coinName, 1));

            //just a quick test
            result.Should().BeTrue();

            return Block;
        }

    }
}
