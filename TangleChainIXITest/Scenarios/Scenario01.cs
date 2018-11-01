using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.Scenarios
{
    [TestFixture]
    public class Scenario01
    {

        private string addr;
        private string hash;
        private string coinName = Utils.GenerateRandomString(10);

        private int transFees = 0;
        private int transOutput = 10;

        [OneTimeSetUp]
        public void ChainInit()
        {

            //IXISettings.Default(true);

            //Block block = CreateChain(coinName);

            //addr = block.SendTo;
            //hash = block.Hash;

            //block.Print();

            addr = "MBSOTDSFTXCNNYKMHGXHHNUUVKUMTIDAWBL9MTSBQMS9CMWNACGTLDCZIYTJJGMLUCRMNKAPPWSVWUSIP";
            hash = "ZBDLVBLWKIENBIABIPATFXCHCYNTZ9W9QAZDSE9YEHXKBX9MJCPSXVJHLOTWOQCIHREUNLCONVSJGEMK9";
            coinName = "QRIPHPBWDD";

        }

        [Test]
        public void TestDownload()
        {

            IXISettings.Default(true);

            Block latest = Core.DownloadChain(coinName, addr, hash, true, null);

            Assert.AreEqual(7, latest.Height);

            long balance = DBManager.GetBalance(coinName, IXISettings.GetPublicKey());

            long blockReward = (latest.Height + 1) * DBManager.GetChainSettings(coinName).BlockReward;
            long stuffOut = transOutput * latest.Height;

            long expected = blockReward - stuffOut;

            Assert.AreEqual(expected, balance);

            latest.Print();

        }

        private Block CreateChain(string coinName)
        {

            int startDifficulty = 7;

            //create genesis transaction
            ChainSettings cSett = new ChainSettings(1000, 0, 0, 2, 30, 3, 3);
            DBManager.SetChainSettings(coinName, cSett);

            Transaction genTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, coinName));
            genTrans.SetGenesisInformation(cSett);
            genTrans.Final();
            genTrans.Upload();

            //create genesis block
            Block genBlock = new Block(0, Utils.GenerateRandomString(81), coinName);
            genBlock.Add(genTrans);

            //we hardcore final() because we want to set time directly for testing purposes
            genBlock.Time = 0;
            genBlock.Owner = IXISettings.PublicKey;
            genBlock.GenerateHash();
            genBlock.NextAddress = Cryptography.GenerateNextAddress(genBlock.Hash, genBlock.SendTo);
            genBlock.IsFinalized = true;

            genBlock.GenerateProofOfWork(startDifficulty);
            genBlock.Upload();
            DBManager.Add(genBlock);

            Console.WriteLine($"Genesis block got uploaded to: {genBlock.SendTo} \n Genesis Transaction got uploaded to: {genTrans.SendTo}");

            //we build first block now
            Block firstBlock = BuildNewBlock(startDifficulty, coinName, genBlock, 10);
            DBManager.Add(firstBlock);

            //we build second block now
            Block secondBlock = BuildNewBlock(startDifficulty, coinName, firstBlock, 20);
            DBManager.Add(secondBlock);


            //we build third block now
            int newDifficulty = DBManager.GetDifficulty(coinName, secondBlock.Height + 1);
            //first test for dynamic difficulty adjustment!
            Assert.AreEqual(startDifficulty + 1, newDifficulty);
            Block thirdBlock = BuildNewBlock(newDifficulty, coinName, secondBlock, 30);
            DBManager.Add(thirdBlock);

            //build block chain A, we now do a chainsplit
            //4 A
            int newDifficulty2 = DBManager.GetDifficulty(coinName, thirdBlock.Height + 1);
            Block fourthBlockA = BuildNewBlock(newDifficulty2, coinName, thirdBlock, 40);
            //check again! difficulty should be the same as before
            Assert.AreEqual(newDifficulty, newDifficulty2);
            DBManager.Add(fourthBlockA);

            //5 A
            Block fivethBlockA = BuildNewBlock(DBManager.GetDifficulty(coinName, fourthBlockA.Height + 1), coinName, fourthBlockA, 50);
            DBManager.Add(fivethBlockA);

            //6 A
            Block sixthBlockA = BuildNewBlock(DBManager.GetDifficulty(coinName, fivethBlockA.Height + 1), coinName, fivethBlockA, 60);
            Assert.AreEqual(9, sixthBlockA.Difficulty);
            DBManager.Add(sixthBlockA);

            //now chain B
            //4B
            Block fourthBlockB = BuildNewBlock(DBManager.GetDifficulty(coinName, thirdBlock.Height + 1), coinName, thirdBlock, 41);
            DBManager.Add(fourthBlockB);
            //5B
            Block fivethBlockB = BuildNewBlock(DBManager.GetDifficulty(coinName, fourthBlockB.Height + 1), coinName, fourthBlockB, 49);
            DBManager.Add(fivethBlockB);
            //6B
            Block sixthBlockB = BuildNewBlock(DBManager.GetDifficulty(coinName, fivethBlockB.Height + 1), coinName, fivethBlockB, 60);
            DBManager.Add(sixthBlockB);
            //7B
            Block seventhBlockB = BuildNewBlock(DBManager.GetDifficulty(coinName, sixthBlockB.Height + 1), coinName, sixthBlockB, 70);
            DBManager.Add(seventhBlockB);

            Assert.AreEqual(9, sixthBlockB.Difficulty);

            return genBlock;
        }

        private Block BuildNewBlock(int difficulty, string coinName, Block blockBefore, int time)
        {

            Block Block = new Block(blockBefore.Height + 1, blockBefore.NextAddress, coinName);
            string addr = Utils.GetTransactionPoolAddress(blockBefore.Height + 1, coinName);
            Transaction trans = new Transaction(IXISettings.GetPublicKey(), 1, addr)
                .AddFee(transFees)
                .AddOutput(transOutput, "you lol")
                .Final()
                .Upload();

            Block.Add(trans);

            //we hardcore final() because we want to set time directly for testing purposes
            Block.Time = time;
            Block.Owner = IXISettings.PublicKey;
            Block.GenerateHash();
            Block.NextAddress = Cryptography.GenerateNextAddress(Block.Hash, Block.SendTo);
            Block.IsFinalized = true;

            Block.GenerateProofOfWork(difficulty).Upload();

            return Block;
        }
    }
}
