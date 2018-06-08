using NUnit.Framework;
using System;
using System.Collections.Generic;
using IXI_TangleChain;
using IXI_TangleChain.Classes;
using TangleNetTransaction = Tangle.Net.Entity.Transaction;
using System.Linq;

namespace TangleChain_IXI_Test.UnitTests {

    [TestFixture]
    public class TestCore {

        [Test]
        public void BlockUpload() {

            string name = Utils.GenerateRandomString(81);
            int difficulty = 5;
            Block testBlock = new Block(3,name,"coolname");
            testBlock.Final();
            testBlock.GenerateProofOfWork(difficulty);

            Settings.Default(true);

            var transList = Core.UploadBlock(testBlock);

            var trans = TangleNetTransaction.FromTrytes(transList[0]);

            Assert.IsTrue(trans.IsTail);

            Block newBlock = Block.FromJSON(trans.Fragment.ToUtf8String());

            Assert.AreEqual(testBlock, newBlock);
        }

        [Test]
        public void BlockSpecificDownload() {

            Settings.Default(true);

            string address = "UVYBKQJWXWKHAZDBNQLIZFSRSHNSMXKXGYWTYZFKVPYOCIUYXALJINKQPGTMLLDZDYK9TBVEDBEQDFSVT";
            string blockHash = "LLLBRI9HJKCMJHUMBDZMSETHRBJJ9WKVXCCTFEMEADTRLUEWRAJFOSLFZCIWAFPHKVWPOXBUNOWJOILGU";

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
            var findTrans = transList.Where(m => m.Equals(trans));

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

            Settings.Default(true);

            string addr = "UVYBKQJWXWKHAZDBNQLIZFSRSHNSMXKXGYWTYZFKVPYOCIUYXALJINKQPGTMLLDZDYK9TBVEDBEQDFSVT";
            int difficulty = 5;

            var blockList = Core.GetAllBlocksFromAddress(addr, difficulty, null);

            Assert.AreEqual(1,blockList.Count);
        }

    }
}
