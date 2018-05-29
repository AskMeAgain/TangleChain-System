using NUnit.Framework;
using Tangle.Net.Repository.Client;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests {
    [TestFixture]
    public class TestClasses {

        [Test]
        public void Way() {

            Way way01 = new Way("hash", "addr", 12);
            Way way02 = new Way("hash2", "addr2", 3);

            way01.AddOldWay(way02);

            Assert.AreEqual(way02, way01.Before);

            Assert.AreEqual(way01.BlockHeight, 12);
        }

        [Test]
        public void Setting() {
            
            //vars
            string correctAddr = "https://potato.iotasalad.org:14265";
            string wrongAddr = "https://potato.iotasalad.org:14333";

            int numOfTrans = 5;

            //fail connection
            var ex = Assert.Throws<IotaApiException>(() => Settings.SetNodeAddress(wrongAddr));
            Assert.That(ex.Message, Is.EqualTo("Command getNodeInfo failed! See inner exception for details."));

            //correct connection
            var info = Settings.SetNodeAddress(correctAddr);
            Assert.AreNotEqual(null,info);

            //set num of trans
            int result = Settings.SetNumOfTransPerBlock(numOfTrans);

            Assert.AreEqual(result, numOfTrans);

            //test default settings
            Settings.Default(false);
            Assert.AreEqual("https://beef.iotasalad.org:14265",Settings.NodeAddress);

            Settings.Default(true);
            Assert.AreEqual("https://testnet140.tangle.works/",Settings.NodeAddress);

        }

        [Test]
        public void Transactions() {

            //adding fees
            Transaction trans = new Transaction("from", 1, "to");
            trans.AddFees(100);

            Assert.IsNotNull(trans.Data);

            //adding NO output
            Transaction trans2 = new Transaction("from2", 2, "to2");
            trans2.AddOutput(-100, "lol");

            Assert.AreEqual(0, trans2.OutputReceiver.Count);

            //compare trans ids;
            Transaction.ID id01 = new Transaction.ID("t");
            Transaction.ID id02 = new Transaction.ID("teeest2");
            Transaction.ID id03 = new Transaction.ID("t");

            id02.Hash = null;

            id01.Hash = "asd";
            id02.Hash = "asd2";
            id03.Hash = "asd";

            Assert.AreNotEqual(id01, id02);
            Assert.AreEqual(id01, id03);

        }

        [Test]
        public void TestBlock() {

            Block block = Block.CreateBlock(3, "lol", "lol");
            block.GenerateHash();
            string hash01 = block.Hash;

            block.Owner = "lolol";
            block.GenerateHash();

            Assert.AreNotEqual(hash01, block.Hash);


        }

    }
}
