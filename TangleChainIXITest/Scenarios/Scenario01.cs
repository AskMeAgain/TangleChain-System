using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.Scenarios {
    [TestFixture]
    public class Scenario01 {

        private string addr = "ZTDHTJEOGEQPSYKTVEPHZMMEOBRAKLLKPEQNAMOCQWDBU9MXC9HCOHZNGZURSDEVTF9OHYTJRMRDILLOQ";
        private string hash = "OSCEAZB9REIHJYWUQNXUOJOT9XVBKSORSSMTWQIZKL9IWFKQRRYMNLLJRYIBMXFDKWVWUTMDYQEBBSZIF";
        private string coinName = "AAAAAAAAANDD";

        private int transFees = 0;
        private int transOutput = 10;

        [OneTimeSetUp]
        public void ChainInit() {

            //this is a one time setup kinda thing. We setup the chain and do our stuff on it.

            IXISettings.Default(true);

            //if (!DataBase.Exists(coinName)) {
                Block block = CreateChain(coinName);

                addr = block.SendTo;
                hash = block.Hash;

                block.Print();
            //}

        }

        [Test]
        public void TestDownload() {

            IXISettings.Default(true);

            Block latest = Core.DownloadChain(coinName, addr, hash, true, true,null);

            Assert.AreEqual(7, latest.Height);

            long balance = DBManager.GetBalance(coinName, IXISettings.GetPublicKey());


            long blockReward = (latest.Height + 1) * DBManager.GetChainSettings(coinName).BlockReward;
            long stuffOut = transOutput * latest.Height;

            long expected = blockReward - stuffOut;

            Assert.AreEqual(expected, balance);

            latest.Print();

        }

        private Block CreateChain(string coinName) {

            Difficulty startDifficulty = new Difficulty(7);

            //create genesis transaction
            ChainSettings cSett = new ChainSettings(1000, 0, 0, 2, 30, 3, 3);
            DBManager.SetChainSettings(coinName, cSett);

            Transaction genTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, coinName));
            genTrans.SetGenesisInformation(cSett);
            genTrans.Final();
            Core.Upload(genTrans);

            //create genesis block
            Block genBlock = new Block(0, Utils.GenerateRandomString(81), coinName);
            genBlock.AddTransactions(genTrans);

            //we hardcore final() because we want to set time directly for testing purposes
            genBlock.Time = 0;
            genBlock.Owner = IXISettings.PublicKey;
            genBlock.GenerateHash();
            genBlock.NextAddress = Cryptography.GenerateNextAddress(genBlock.Hash, genBlock.SendTo);

            genBlock.GenerateProofOfWork(startDifficulty);
            genBlock.Upload();
            DBManager.AddBlock(genBlock, true,true);

            Console.WriteLine($"Genesis block got uploaded to: {genBlock.SendTo} \n Genesis Transaction got uploaded to: {genTrans.TransactionPoolAddress}");

            //we build first block now
            Block firstBlock = BuildNewBlock(startDifficulty, coinName, genBlock, 10);
            DBManager.AddBlock(firstBlock, true,true);

            //we build second block now
            Block secondBlock = BuildNewBlock(startDifficulty, coinName, firstBlock, 20);
            DBManager.AddBlock(secondBlock, true,true);
      

            //we build third block now
            Difficulty newDifficulty = DBManager.GetDifficulty(coinName,secondBlock.Height + 1);
            //first test for dynamic difficulty adjustment!
            Assert.AreEqual(startDifficulty.PrecedingZeros + 1, newDifficulty.PrecedingZeros);
            Block thirdBlock = BuildNewBlock(newDifficulty, coinName, secondBlock, 30);
            DBManager.AddBlock(thirdBlock, true,true);

            //build block chain A, we now do a chainsplit
            //4 A
            Difficulty newDifficulty2 = DBManager.GetDifficulty(coinName,thirdBlock.Height + 1);
            Block fourthBlockA = BuildNewBlock(newDifficulty2, coinName, thirdBlock, 40);
            //check again! difficulty should be the same as before
            Assert.AreEqual(newDifficulty.PrecedingZeros, newDifficulty2.PrecedingZeros);
            DBManager.AddBlock(fourthBlockA, true,true);

            //5 A
            Block fivethBlockA = BuildNewBlock(DBManager.GetDifficulty(coinName,fourthBlockA.Height + 1), coinName, fourthBlockA, 50);
            DBManager.AddBlock(fivethBlockA, true,true);

            //6 A
            Block sixthBlockA = BuildNewBlock(DBManager.GetDifficulty(coinName,fivethBlockA.Height + 1), coinName, fivethBlockA, 60);
            Assert.AreEqual(9, sixthBlockA.Difficulty.PrecedingZeros);
            DBManager.AddBlock(sixthBlockA, true,true);

            //now chain B
            //4B
            Block fourthBlockB = BuildNewBlock(DBManager.GetDifficulty(coinName,thirdBlock.Height + 1), coinName, thirdBlock, 41);
            DBManager.AddBlock(fourthBlockB, true,true);
            //5B
            Block fivethBlockB = BuildNewBlock(DBManager.GetDifficulty(coinName,fourthBlockB.Height + 1), coinName, fourthBlockB, 49);
            DBManager.AddBlock(fivethBlockB, true,true);
            //6B
            Block sixthBlockB = BuildNewBlock(DBManager.GetDifficulty(coinName,fivethBlockB.Height + 1), coinName, fivethBlockB, 60);
            DBManager.AddBlock(sixthBlockB, true,true);
            //7B
            Block seventhBlockB = BuildNewBlock(DBManager.GetDifficulty(coinName,sixthBlockB.Height + 1), coinName, sixthBlockB, 70);
            DBManager.AddBlock(seventhBlockB, true,true);

            Assert.AreEqual(9, sixthBlockB.Difficulty.PrecedingZeros);

            return genBlock;
        }

        private Block BuildNewBlock(Difficulty difficulty, string coinName, Block blockBefore, int time) {

            Block Block = new Block(blockBefore.Height + 1, blockBefore.NextAddress, coinName);

            Transaction trans = new Transaction(IXISettings.GetPublicKey(), 1,
                Utils.GetTransactionPoolAddress(blockBefore.Height + 1, coinName));

            trans.AddFee(transFees);
            trans.AddOutput(transOutput, "you lol");
            trans.Final();

            Core.Upload(trans);

            Block.AddTransactions(trans);

            //we hardcore final() because we want to set time directly for testing purposes
            Block.Time = time;
            Block.Owner = IXISettings.PublicKey;
            Block.GenerateHash();
            Block.NextAddress = Cryptography.GenerateNextAddress(Block.Hash, Block.SendTo);

            Block.GenerateProofOfWork(difficulty);

            Block.Upload();

            return Block;
        }
    }
}
