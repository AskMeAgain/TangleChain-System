using System.Collections.Generic;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using System;
using Tangle.Net.Cryptography;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestUtil {

        [Test]
        public void BlockJSON() {

            IXISettings.Default(true);

            //create dummy block first
            Block testBlock = new Block(3,"sendto", "name");

            //HACK, DONT DO THIS NORMALLY. BETTER COMPUTE POW WITH FUNCTION
            testBlock.Difficulty = new Difficulty(3);

            testBlock.Final();


            //convert to json
            string json = testBlock.ToJSON();

            //convert string to block
            Block newBlock = Block.FromJSON(json);

            Assert.AreEqual(testBlock, newBlock);
        }

        [Test]
        public void VerifyNonce() {

            Difficulty difficulty = new Difficulty(7);

            //smaller
            var check01 = "99C";
            Assert.IsTrue(Utils.VerifyHashAgainstDifficulty(Converter.TrytesToTrits(check01), difficulty));

            //higher
            var check02 = "99A";
            Assert.IsFalse(Utils.VerifyHashAgainstDifficulty(Converter.TrytesToTrits(check02), difficulty));

        }

        [Test]
        public void DoProofOfWork() {

            Difficulty difficulty = new Difficulty(7);
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";

            long nonce = Utils.ProofOfWork(hash, difficulty);

            Assert.IsTrue(Utils.VerifyHashAndNonceAgainstDifficulty(hash, nonce, difficulty));

            difficulty.PrecedingZeros += 30;

            Assert.IsFalse(Utils.VerifyHashAndNonceAgainstDifficulty(hash, nonce, difficulty));

            Console.WriteLine("Hash: " + hash);
            Console.WriteLine("Nonce" + nonce);

        }

        [Test]
        public void VerifyHash() {

            //precomputed
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";
            int nonce = 479;
            Difficulty difficulty = new Difficulty();

            Assert.IsTrue(Utils.VerifyHashAndNonceAgainstDifficulty(hash, nonce, difficulty));

        }

        [Test]
        public void TestRandomGenerator() {
            int length = 10;
            string result = Utils.GenerateRandomString(length);

            Assert.AreEqual(result.Length, length);
        }

        [Test]
        public void TestConnection() {

            Assert.IsTrue(Utils.TestConnection(@"https://iota.getway.org/:443"));
            Assert.IsFalse(Utils.TestConnection(@"https://google.org/:3000"));

        }


        [Test]
        public void TestWrongHash() {

            Difficulty difficulty = new Difficulty(6);
            IXISettings.Default(false);

            Block block = new Block(3, "lol", "test");
            block.Final();
            block.GenerateProofOfWork(difficulty);

            block.Hash = "LOLOLOLOL";

            Assert.IsFalse(Utils.VerifyBlock(block, difficulty));

            block.Nonce = 0;

            Assert.IsFalse(Utils.VerifyBlock(block,difficulty));

        }

        [Test]
        public void TestDifficultyChange() {

            Assert.AreEqual(Utils.CalculateDifficultyChange(26),2);
            Assert.AreEqual(Utils.CalculateDifficultyChange(10),1);
            Assert.AreEqual(Utils.CalculateDifficultyChange(0.1),-2);
            Assert.AreEqual(Utils.CalculateDifficultyChange(27),2);
            Assert.AreEqual(Utils.CalculateDifficultyChange(2187),6);

            Assert.AreNotEqual(Utils.CalculateDifficultyChange(27),0);

        }
    }
}
