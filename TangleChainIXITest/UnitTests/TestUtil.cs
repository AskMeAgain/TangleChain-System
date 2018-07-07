using System.Collections.Generic;
using NUnit.Framework;
using TangleChainIXI.Classes;
using TangleChainIXI;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestUtil {

        [Test]
        public void BlockJSON() {

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

            var check01 = new int[] { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, -1 };
            Assert.IsTrue(Utils.CheckPOWResult(check01, 7));

            var check02 = new int[] { 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, -1 };
            Assert.IsFalse(Utils.CheckPOWResult(check02, 7));

        }

        [Test]
        public void DoProofOfWork() {

            int difficulty = 7;
            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";

            int nonce = Utils.ProofOfWork(hash, difficulty);

            Assert.IsTrue(Utils.VerifyHash(hash, nonce, difficulty));
            Assert.IsFalse(Utils.VerifyHash(hash, nonce, difficulty + 30));

        }

        [Test]
        public void VerifyHash() {

            string hash = "ASDASDASDASDASDASDASDASDASDASDASD";
            int nonce = 479;
            int difficulty = 8;

            Assert.IsTrue(Utils.VerifyHash(hash, nonce, difficulty));
            Assert.IsFalse(Utils.VerifyHash(hash, nonce, difficulty + 20));

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
