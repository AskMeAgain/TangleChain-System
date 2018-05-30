using NUnit.Framework;
using System;
using System.Collections.Generic;
using TangleChain;
using TangleChain.Classes;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using System.Linq;

namespace TangleChainTest.UnitTests {

    [TestFixture]
    public class TestCore {

        [Test]
        public void BlockUpload() {

            Block testBlock = new Block();

            var transList = Core.UploadBlock(testBlock);

            var trans = TangleNetTransaction.FromTrytes(transList[0]);

            Assert.IsTrue(trans.IsTail);

            Block newBlock = Block.FromJSON(trans.Fragment.ToUtf8String());

            Assert.AreEqual(testBlock, newBlock);
        }

        [Test]
        public void BlockSpecificDownload() {

            Settings.Default(false);

            string address = "JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";
            string blockHash = "A9XGUQSNWXYEYZICOCHC9B9GV9EFNOWBHPCX9TSKSPDINXXCFKJJAXNHMIWCXELEBGUL9EOTGNWYTLGNO";

            Block newBlock = Core.GetSpecificBlock(address, blockHash, 5);

            Assert.AreEqual(blockHash, newBlock.Hash);
        }

        [Test]
        public void BlockFailAtSpecific() {

            Block block = Core.GetSpecificBlock(Utils.GenerateRandomString(81), "lol", 5);
            Assert.IsNull(block);
        }

        [Test]
        public void TransactionUploadDownload() {

            string sendTo = Utils.GenerateRandomString(81);

            Transaction trans = new Transaction("ME", 0, sendTo);
            trans.AddFee(30);
            trans.AddOutput(100, "YOU");
            trans.Final();

            var resultTrytes = Core.UploadTransaction(trans);
            var tnTrans = TangleNetTransaction.FromTrytes(resultTrytes[0]);

            Assert.IsTrue(tnTrans.IsTail);

            Transaction newTrans = Transaction.FromJSON(tnTrans.Fragment.ToUtf8String());

            Assert.AreEqual(trans, newTrans);

            var transList = Core.GetAllTransactionsFromAddress(sendTo);
            var findTrans = transList.Where(m => m.Identity.Equals(trans.Identity));

            Assert.AreEqual(findTrans.Count(), 1);
        }

        [Test]
        public void FillTransactionPool() {

            int n = 3;
            string coinName = Utils.GenerateRandomString(10);
            string addr = Utils.GetTransactionPoolAddress(3, coinName);
            Settings.Default(true);

            for (int i = 0; i < n; i++) {
                //we create now the transactions
                Transaction trans = new Transaction("ME", 1, addr);
                trans.AddFee(30);
                trans.AddOutput(100, "YOU");
                trans.Final();

                //we upload these transactions
                Core.UploadTransaction(trans);
            }

            var transList = Core.GetAllTransactionsFromAddress(addr);

            Assert.AreEqual(transList.Count, n);

            Console.WriteLine(addr);
        }

        [Test]
        public void BlockDownloadAllFromAddress() {

            Settings.Default(false);

            string addr = "ATHEGAXYAKPVRJHDSNAVETVUWWMBYCQHV9UDQOP99FDPSZZIRASRZAXPAMBSEMNLCTDROEWVSAHMAHSXH";
            int difficulty = 5;

            var blockList = Core.GetAllBlocksFromAddress(addr, difficulty, 2);

            Assert.AreEqual(2,blockList.Count);
        }

    }
}
