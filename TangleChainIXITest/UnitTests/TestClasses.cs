using NUnit.Framework;
using Tangle.Net.Repository.Client;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.UnitTests {
    [TestFixture]
    public class TestClasses {

        [Test]
        public void TestWay() {

            Way way01 = new Way("hash", "addr", 12);
            Way way02 = new Way("hash2", "addr2", 3);

            way01.AddOldWay(way02);

            Assert.AreEqual(way02, way01.Before);

            Assert.AreEqual(way01.BlockHeight, 12);
        }

        [Test]
        public void TestSettings() {
            
            //TODO

        }

        [Test]
        public void TestTransactions() {

            //adding fees
            Transaction trans = new Transaction("from", 1, "to");
            trans.AddFee(100);

            Assert.IsNotNull(trans.Data);

            //adding NO output
            Transaction trans2 = new Transaction("from2", 2, "to2");
            trans2.AddOutput(-100, "lol");

            Assert.AreEqual(0, trans2.OutputReceiver.Count);

        }

    }
}
