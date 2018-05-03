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

            var transList = Core.SendBlock(testBlock);

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
        public void DownloadSpecificBlockFromAddressAndBundleHash() {

            string address = "CBVYKBQWSUMUDPPTLQFPSDHGSJYVPUOKREWSDHRAMYRGI9YALHGRZXJAKZIYZHGFPMYPMWIGUWBNVPVCB";
            string blockHash = "BYIKMJDR9ZSWSATBRZWCMSPUYRILWHANTBJOMCFHXXPTFEBINULZPSN9FDZOK9Q9HNCJPBCXEJWNV99IK";

            Block newBlock = Core.GetSpecificBlock(address, blockHash);

            Assert.AreEqual(blockHash, newBlock.Hash);
        }

        [TestMethod]
        public void CreateBlock() {

            Block block = Core.CreateBlock(2);

            Assert.AreEqual(2, block.Height);
            Assert.IsNotNull(block.Hash);
        }

        [TestMethod]
        public void CreateGenesisBlock() {




        }

        [TestMethod]
        public void ProofOfWork() {

            int difficulty = 9;
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";

            int nonce = Utils.ProofOfWork(hash, difficulty);

            Assert.IsTrue(Utils.VerifyHash(hash,nonce,difficulty));
            Assert.IsFalse(Utils.VerifyHash(hash,nonce,difficulty+30));

            Console.WriteLine("Nonce: " + nonce);
            Console.WriteLine("Difficulty: " + difficulty);
            Console.WriteLine("Hash: " + hash);

        }

        [TestMethod]
        public void VerifyHash() {

            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";
            int nonce = 479;
            int difficulty = 8;

            Assert.IsTrue(Utils.VerifyHash(hash, nonce, difficulty));
            Assert.IsFalse(Utils.VerifyHash(hash, nonce, difficulty+20));

        }

        [TestMethod]
        public void VerifyNonce() {

            int[] check_01 = new int[] {0,0,0,0,0,0,0,1,1,1,1,1,1,1,-1 };
            Assert.IsTrue(Utils.CheckPOWResult(check_01, 7));

            int[] check_02 = new int[] { 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, -1 };
            Assert.IsFalse(Utils.CheckPOWResult(check_02, 7));

        }
    }
}
