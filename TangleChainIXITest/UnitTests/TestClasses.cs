using NUnit.Framework;
using System.Collections.Generic;
using Tangle.Net.Repository.Client;
using TangleChainIXI.Classes;

namespace TangleChainIXITest.UnitTests {
    [TestFixture]
    public class TestClasses {

        [Test]
        public void TestBlock() {

            IXISettings.Default(true);

            Block block = new Block();

            Transaction trans1 = new Transaction("you", 1, "lol");
            trans1.Final();

            Transaction trans2 = new Transaction("you2", 1, "lol3");
            trans2.Final();

            Transaction trans3 = new Transaction("you3", 1, "lol2");
            trans3.Final();

            block.AddTransactions(new List<Transaction>() { trans1,trans2,trans3 });

        }

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

            IXISettings.Default(true);

        }

        [Test]
        public void TestTransactions() {

            //adding fees
            Transaction trans = new Transaction("from", 1, "to");
            trans.AddFee(100);
            trans.AddOutput(100, "you");
            trans.AddOutput(200, "youagain");

            Assert.AreEqual(400, trans.ComputeOutgoingValues());
            Assert.IsNotNull(trans.Data);

            //adding NO output
            Transaction trans2 = new Transaction("from2", 2, "to2");
            trans2.AddOutput(-100, "lol");

            Assert.AreEqual(0, trans2.OutputReceiver.Count);

        }

    }
}
