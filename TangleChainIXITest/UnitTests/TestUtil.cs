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

            string con1 = @"https://iota.getway.org/:443";

            Assert.IsTrue(Utils.TestConnection(con1));

        }
    }
}
