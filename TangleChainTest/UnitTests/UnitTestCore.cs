using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tangle.Net.Entity;
using TangleChain;

namespace TangleChainTest {

    [TestClass]
    public class UnitTestCore {

        [TestMethod]
        public void UploadBlock() {

            Block testBlock = new Block();

            var transList = Core.UploadBlock(testBlock);

            Transaction trans = Transaction.FromTrytes(transList[0]);

            Assert.IsTrue(trans.IsTail);

            Block newBlock = Utils.GetBlockFromJSON(trans.Fragment.ToUtf8String());

            Assert.AreEqual(testBlock, newBlock);
        }

        [TestMethod]
        public void DownloadSpecificBlock() {

            string address = "GJRUPNRQWKOMNBE9EOOSLMQTTVZIZCBLMXVDN9RIQEAWICYFMFV9CNFD9UNMGIOEDTGCANDIHVTUTALZW";
            string blockHash = "9HBMTESIZRGEC9FHBX9UHJXQPDJYGK9LTPOMAM9KQYAWXJEDHTNQQFHTPPDBMQCMNTTIHGZPAKKKZO9GG";

            Block newBlock = Core.GetSpecificBlock(address, blockHash, 5);

            Assert.AreEqual(blockHash, newBlock.Hash);
        }

        [TestMethod]
        public void CreateBlock() {

            int height = 2;
            string sendTo = "lol";

            Block block = Core.CreateBlock(height, sendTo);

            Assert.AreEqual(height, block.Height);
            Assert.IsNotNull(block.Hash);
        }

        [TestMethod]
        public void CreateGenesisBlock() {

            int difficulty = 5;

            Block testBlock = new Block();

            Block genesis = Core.CreateAndUploadGenesisBlock();

            Block newBlock = Core.GetSpecificBlock(genesis.SendTo, genesis.Hash, difficulty);

            Assert.AreEqual(genesis, newBlock);      
            Assert.AreNotEqual(genesis, testBlock);

            newBlock.Print();

        }

        [TestMethod]
        public void MineBlock() {

            string address = "GGGLFNN9AOOEBWGGVKXEEIDRGHYPFWMZKTQHXPGIQTJJGYJZAOTLYRFQDDRBANPCIF9JNUXMNOTNLJHR9";
            int height = 3;
            int difficulty = 5;

            //mine block and upload it
            Block block = Core.MineBlock(height,address, difficulty);
            block.GenerateHash();

            //download exactly this block
            Block newBlock = Core.GetSpecificBlock(address, block.Hash, difficulty);
            newBlock.GenerateHash();

            Assert.AreEqual(block,newBlock);
        }

    }
}
