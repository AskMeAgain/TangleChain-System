using System.Collections.Generic;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;
using System;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestUtil {

        [Test]
        public void BlockJSON() {

            IXISettings.Default(true);

            //create dummy block first
            Block testBlock = new Block(3,"sendto", "name");
            testBlock.Final();

            //convert to json
            string json = testBlock.ToJSON();

            //convert string to block
            Block newBlock = Block.FromJSON(json);

            Assert.AreEqual(testBlock, newBlock);
        }

        [Test]
        public void VerifyNonce() {

            Difficulty difficulty = new Difficulty();

            //smaller
            var check01 = "AAE";
            Assert.IsTrue(Utils.CheckPOWResult(check01, difficulty));

            //higher
            var check02 = "AAH";
            Assert.IsFalse(Utils.CheckPOWResult(check02, difficulty));

            //even
            var check03 = "AAF";
            Assert.IsTrue(Utils.CheckPOWResult(check03, difficulty));
        }

        [Test]
        public void DoProofOfWork() {

            Difficulty difficulty = new Difficulty();
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";

            long nonce = Utils.ProofOfWork(hash, difficulty);

            Assert.IsTrue(Utils.VerifyHash(hash, nonce, difficulty));

            difficulty.PrecedingZeros += 30;

            Assert.IsFalse(Utils.VerifyHash(hash, nonce, difficulty));

            Console.WriteLine("Hash: " + hash);
            Console.WriteLine("Nonce" + nonce);
            difficulty.Print();
        }

        [Test]
        public void VerifyHash() {

            //precomputed
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";
            int nonce = 4064;
            Difficulty difficulty = new Difficulty();

            Assert.IsTrue(Utils.VerifyHash(hash, nonce, difficulty));

        }

        [Test]
        public void TestRandomGenerator() {
            int length = 10;
            string result = Utils.GenerateRandomString(length);

            Assert.AreEqual(result.Length, length);
        }

        [Test]
        public void TestConnection() {

            string con1 = @"https://mint.iotasalad.org:14265";
            string con2 = @"https://mint.iotasalad.org:14266";

            Assert.IsTrue(Utils.TestConnection(con1));
            Assert.IsFalse(Utils.TestConnection(con2));


        }
    }
}
