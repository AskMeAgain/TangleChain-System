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
        public void DownloadBlocksFromAddress() {

            string address = "ZBVYKBQWSUMUDPPTLQFPSDHGSJYVPUOKREWSDHRAMYRGI9YALHGRZXJAKZIYZHGFPMYPMWIGUWBNVPVCB";

            List<Block> blocks = Core.GetAllBlocksFromAddress(address);

            Assert.IsTrue(blocks.Count > 0);

        }

        [TestMethod]
        public void DownloadSpecificBlock() {

            string address = "CBVYKBQWSUMUDPPTLQFPSDHGSJYVPUOKREWSDHRAMYRGI9YALHGRZXJAKZIYZHGFPMYPMWIGUWBNVPVCB";
            string blockHash = "BYIKMJDR9ZSWSATBRZWCMSPUYRILWHANTBJOMCFHXXPTFEBINULZPSN9FDZOK9Q9HNCJPBCXEJWNV99IK";

            Block newBlock = Core.GetSpecificBlock(address, blockHash);

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

            Block testBlock = new Block();

            Block genesis = Core.CreateAndUploadGenesisBlock();

            Block newBlock = Core.GetSpecificBlock(genesis.SendTo, genesis.Hash);

            Assert.AreEqual(genesis, newBlock);      
            Assert.AreNotEqual(genesis, testBlock);

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
            Block newBlock = Core.GetSpecificBlock(address, block.Hash);
            newBlock.GenerateHash();

            Assert.AreEqual(block,newBlock);
        }
    }
}
