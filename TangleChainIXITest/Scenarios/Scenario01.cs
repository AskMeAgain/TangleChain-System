using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.Scenarios {
    [TestFixture]
    public class Scenario01 {

        [Test]
        public void Init() {

            Difficulty startDifficulty = new Difficulty(7);
            IXISettings.Default(true);
            string coinName = Utils.GenerateRandomString(10);
            DataBase Db = new DataBase(coinName);


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
            genBlock.NextAddress = Utils.GenerateNextAddr(genBlock.Hash, genBlock.SendTo);

            genBlock.GenerateProofOfWork(startDifficulty);
            Core.UploadBlock(genBlock);
            Db.AddBlock(genBlock, true);

            Console.WriteLine($"Genesis block got uploaded to: {genBlock.SendTo} \n Genesis Transaction got uploaded to: {genTrans.SendTo}");

            //we build first block now
            Block firstBlock = BuildNewBlock(startDifficulty, coinName, genBlock,10);
            Db.AddBlock(firstBlock, true);

            //we build second block now
            Block secondBlock = BuildNewBlock(startDifficulty, coinName, firstBlock, 20);
            Db.AddBlock(secondBlock, true);


            //we build third block now
            Difficulty newDifficulty = Core.GetDifficultyViaHeight(coinName, secondBlock.Height + 1);
            //first test for dynamic difficulty adjustment!
            Assert.AreEqual(startDifficulty.PrecedingZeros+1, newDifficulty.PrecedingZeros);
            Block thirdBlock = BuildNewBlock(newDifficulty, coinName, secondBlock, 30);
            Db.AddBlock(thirdBlock, true);

            //build fourth block
            Difficulty newDifficulty2 = Core.GetDifficultyViaHeight(coinName, thirdBlock.Height + 1);
            Block fourthBlock = BuildNewBlock(newDifficulty2, coinName, thirdBlock, 40);
            //check again! difficulty should be the same as before
            Assert.AreEqual(newDifficulty.PrecedingZeros, newDifficulty2.PrecedingZeros);
            Db.AddBlock(fourthBlock, true);

        }

        private static Block BuildNewBlock(Difficulty difficulty, string coinName, Block blockBefore, int time) {

            Block Block = new Block(blockBefore.Height+1, blockBefore.NextAddress, coinName);

            //we hardcore final() because we want to set time directly for testing purposes
            Block.Time = time;
            Block.Owner = IXISettings.PublicKey;
            Block.GenerateHash();
            Block.NextAddress = Utils.GenerateNextAddr(Block.Hash, Block.SendTo);

            Block.GenerateProofOfWork(difficulty);

            Core.UploadBlock(Block);

            return Block;
        }
    }
}
