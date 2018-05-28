using System.Collections.Generic;
using NUnit.Framework;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests {

    [TestFixture]
    public class TestUtil {

        [Test]
        public void ConvertingBlock() {

            //create dummy block first
            Block testBlock = new Block();

            //convert to json
            string json = testBlock.ToJSON();

            //convert string to block
            Block newBlock = Block.FromJSON(json);

            Assert.AreEqual(testBlock, newBlock);
        }

        [Test]
        public void VerifyNonce() {

            int[] check_01 = new int[] { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, -1 };
            Assert.IsTrue(Utils.CheckPOWResult(check_01, 7));

            int[] check_02 = new int[] { 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, -1 };
            Assert.IsFalse(Utils.CheckPOWResult(check_02, 7));

        }

        [Test]
        public void ProofOfWork() {

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

            Block newBlock = new Block() {
                Hash = hash,
                Nonce = nonce
            };

            Assert.IsTrue(Utils.VerifyBlock(newBlock,difficulty));

        }

        [Test]
        public void ConvertBlocklistToWays() {

            int difficulty = 5;
            string address = "JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";

            List<Block> blocks = Core.GetAllBlocksFromAddress(address, difficulty, null);
            List<Way> ways = Utils.ConvertBlocklistToWays(blocks);

            Assert.AreEqual(blocks.Count, ways.Count);
            Assert.AreEqual(blocks[0].Hash, ways[0].BlockHash);

        }

        [Test]
        public void TestRandomGenerator() {
            int length = 10;
            string result = Utils.GenerateRandomString(length);

            Assert.AreEqual(result.Length, length);


        }
    }
}
