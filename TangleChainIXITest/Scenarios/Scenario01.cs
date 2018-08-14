using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.Scenarios {
    [TestFixture]
    public class Scenario01 {

        private string addr = "TYEAHPEBFLNRSQBMMWIPLFPYVJUZYQJLOJRRJOTKLT9AWNYNCZEOH9GOIGGYVGEQCBCIZKCYDZDSFPCHM";
        private string hash = "PQODJUCAYGDBQGCMWSSDPNNNOMQJBNELICNLPPBUYNTHNBHFDMLRRSDXJNPKEDRKJEELWOMCJLPCHWAXZ";
        private string coinName = "AAAAAAAAA";

        [OneTimeSetUp]
        public void ChainInit() {

            //this is a one time setup kinda thing. We setup the chain and do our stuff on it.

            IXISettings.Default(true);

            //only create chain if we dont have a local copy
            if (!DataBase.Exists(coinName)) {
                Block block = CreateChain(coinName);

                addr = block.SendTo;
                hash = block.Hash;
            }

        }

        [Test]
        public void TestDownload() {

            IXISettings.Default(true);
            DataBase Db = new DataBase(coinName);

            //delete DB, because we want to test download function
            Db.DeleteDatabase();

            Block latest = Core.DownloadChain(addr, hash, true, null, coinName);

            Assert.AreEqual(7, latest.Height);

            latest.Print();

        }

        private static Block CreateChain(string coinName) {

            DataBase Db = new DataBase(coinName);
            Difficulty startDifficulty = new Difficulty(7);

            //create genesis transaction
            ChainSettings cSett = new ChainSettings(1000, 0, 0, 2, 30, 3, 3);
            Transaction genTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, coinName));
            genTrans.SetGenesisInformation(cSett);
            genTrans.Final();
            Core.UploadTransaction(genTrans);

            //create genesis block
            Block genBlock = new Block(0, Utils.GenerateRandomString(81), coinName);
            genBlock.AddTransactions(genTrans);

            //we hardcore final() because we want to set time directly for testing purposes
            genBlock.Time = 0;
            genBlock.Owner = IXISettings.PublicKey;
            genBlock.GenerateHash();
            genBlock.NextAddress = Cryptography.GenerateNextAddress(genBlock.Hash, genBlock.SendTo);

            genBlock.GenerateProofOfWork(startDifficulty);
            Core.UploadBlock(genBlock);
            Db.AddBlock(genBlock, true);

            Console.WriteLine($"Genesis block got uploaded to: {genBlock.SendTo} \n Genesis Transaction got uploaded to: {genTrans.SendTo}");

            //we build first block now
            Block firstBlock = BuildNewBlock(startDifficulty, coinName, genBlock, 10);
            Db.AddBlock(firstBlock, true);

            //we build second block now
            Block secondBlock = BuildNewBlock(startDifficulty, coinName, firstBlock, 20);
            Db.AddBlock(secondBlock, true);


            //we build third block now
            Difficulty newDifficulty = Db.GetDifficulty(secondBlock.Height + 1);
            //first test for dynamic difficulty adjustment!
            Assert.AreEqual(startDifficulty.PrecedingZeros + 1, newDifficulty.PrecedingZeros);
            Block thirdBlock = BuildNewBlock(newDifficulty, coinName, secondBlock, 30);
            Db.AddBlock(thirdBlock, true);

            //build block chain A, we now do a chainsplit
            //4 A
            Difficulty newDifficulty2 = Db.GetDifficulty(thirdBlock.Height + 1);
            Block fourthBlockA = BuildNewBlock(newDifficulty2, coinName, thirdBlock, 40);
            //check again! difficulty should be the same as before
            Assert.AreEqual(newDifficulty.PrecedingZeros, newDifficulty2.PrecedingZeros);
            Db.AddBlock(fourthBlockA, true);

            //5 A
            Block fivethBlockA = BuildNewBlock(Db.GetDifficulty(fourthBlockA.Height + 1), coinName, fourthBlockA, 50);
            Db.AddBlock(fivethBlockA, true);

            //6 A
            Block sixthBlockA = BuildNewBlock(Db.GetDifficulty(fivethBlockA.Height + 1), coinName, fivethBlockA, 60);
            Assert.AreEqual(9, sixthBlockA.Difficulty.PrecedingZeros);
            Db.AddBlock(sixthBlockA, true);

            //now chain B
            //4B
            Block fourthBlockB = BuildNewBlock(Db.GetDifficulty(thirdBlock.Height + 1), coinName, thirdBlock, 41);
            Db.AddBlock(fourthBlockB, true);
            //5B
            Block fivethBlockB = BuildNewBlock(Db.GetDifficulty(fourthBlockB.Height + 1), coinName, fourthBlockB, 49);
            Db.AddBlock(fivethBlockB, true);
            //6B
            Block sixthBlockB = BuildNewBlock(Db.GetDifficulty(fivethBlockB.Height + 1), coinName, fivethBlockB, 60);
            Db.AddBlock(sixthBlockB, true);
            //7B
            Block seventhBlockB = BuildNewBlock(Db.GetDifficulty(sixthBlockB.Height + 1), coinName, sixthBlockB, 70);
            Db.AddBlock(seventhBlockB, true);

            Assert.AreEqual(9, sixthBlockB.Difficulty.PrecedingZeros);

            return genBlock;
        }

        private static Block BuildNewBlock(Difficulty difficulty, string coinName, Block blockBefore, int time) {

            Block Block = new Block(blockBefore.Height + 1, blockBefore.NextAddress, coinName);

            //we hardcore final() because we want to set time directly for testing purposes
            Block.Time = time;
            Block.Owner = IXISettings.PublicKey;
            Block.GenerateHash();
            Block.NextAddress = Cryptography.GenerateNextAddress(Block.Hash, Block.SendTo);

            Block.GenerateProofOfWork(difficulty);

            Core.UploadBlock(Block);

            return Block;
        }
    }
}
