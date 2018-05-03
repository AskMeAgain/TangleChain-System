using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TangleChain;

namespace TangleChainTest.UnitTests {

    [TestClass]
    public class UnitTestUtil {

        [TestMethod]
        public void ConvertingBlock() {

            //create dummy block first
            Block testBlock = new Block();           

            //convert to json
            string json = Utils.GetStringFromBlock(testBlock);

            //convert string to block
            Block newBlock = Utils.GetBlockFromJSON(json);

            Assert.AreEqual(testBlock, newBlock);
        }

        [TestMethod]
        public void VerifyNonce() {

            int[] check_01 = new int[] { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, -1 };
            Assert.IsTrue(Utils.CheckPOWResult(check_01, 7));

            int[] check_02 = new int[] { 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, -1 };
            Assert.IsFalse(Utils.CheckPOWResult(check_02, 7));

        }

        [TestMethod]
        public void ProofOfWork() {

            int difficulty = 9;
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";

            int nonce = Utils.ProofOfWork(hash, difficulty);

            Assert.IsTrue(Utils.VerifyHash(hash, nonce, difficulty));
            Assert.IsFalse(Utils.VerifyHash(hash, nonce, difficulty + 30));

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
            Assert.IsFalse(Utils.VerifyHash(hash, nonce, difficulty + 20));

        }
    }
}
