using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Classes;
using static TangleChainIXI.Utils;
using TangleChainIXI;
using System.IO;
using FluentAssertions;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestDataBase {
        private string DataBaseName;

        [OneTimeSetUp]
        public void SetupChain() {
            DataBaseName = Initalizing.SetupDatabaseTest();
        }

        [Test]
        public void AddGetBlock() {

            IXISettings.Default(true);

            string name = GenerateRandomString(5);
            string addr = GenerateRandomString(81);
            long height = GenerateRandomInt(4);

            Block block = new Block(height, addr, name);
            block.Final();

            //DONT DO THIS NORMALLY. HACK!
            block.Difficulty = new Difficulty(2);

            DBManager.AddBlock(block, false,false);

            Block result = DBManager.GetBlock(name,block.Height);

            result.Should().Be(block);
            DBManager.GetBlock(name, -1).Should().BeNull();

            DBManager.DeleteBlock(name,height);

            DBManager.GetBlock(name, height).Should().BeNull();

        }

        [Test]
        public void DBExists() {

            IXISettings.Default(true);

            string name = GenerateRandomString(20);

            Assert.IsFalse(DataBase.Exists(name));

            DBManager.GetLatestBlock(name);

            Assert.IsTrue(DataBase.Exists(name));

        }

        [Test]
        public void UpdateBlock() {

            string name = GenerateRandomString(5);
            string addr = GenerateRandomString(81);
            long height = GenerateRandomInt(4);

            

            Block block = new Block(height, addr, name);
            block.Final();

            //HACK AGAIN, DONT DO THIS.
            block.Difficulty = new Difficulty();

            DBManager.AddBlock(block, false,false);

            block.Owner = "LOL";
            block.Final();

            DBManager.AddBlock(block, false,false);

            Block checkBlock = DBManager.GetBlock(name,block.Height);

            checkBlock.Print();
            block.Print();

            Assert.AreEqual(checkBlock, block);

        }

        [Test]
        public void LatestBlock()
        {

            long height = 1000000;


            Block block = new Block(height, "you", DataBaseName);
            block.Final();
            block.GenerateProofOfWork(new Difficulty(2));

            DBManager.AddBlock(block, false, false);

            Block result = DBManager.GetLatestBlock(DataBaseName);

            Assert.AreEqual(height, result.Height);

        }

        [Test]
        public void AddBlockAndTransaction() {

            IXISettings.Default(true);

            Block block = new Block(100, "COOLADDRESS", DataBaseName);
            block.Final();

            //DONT DO THIS. HACK!
            block.Difficulty = new Difficulty(2);

            DBManager.AddBlock(block, false,false);

            Transaction trans = new Transaction("ME", 1, GetTransactionPoolAddress(block.Height, DataBaseName));
            trans.AddFee(10);
            trans.AddOutput(10, "YOU");
            trans.AddOutput(10, "YOU2");
            trans.AddOutput(10, "YOU3");
            trans.Final();

            DBManager.AddTransaction(DataBaseName,trans, block.Height, null);

            Transaction result = DBManager.GetTransaction(DataBaseName,trans.Hash, block.Height);

            Assert.AreEqual(result, trans);

        }

        [Test]
        public void GetChainSettings() {

            ChainSettings settings = DBManager.GetChainSettings(DataBaseName);

            settings.Print();

            Assert.AreEqual(settings.BlockReward, 100);
            Assert.AreEqual(settings.BlockTime, 100);
            Assert.AreEqual(settings.TransactionPoolInterval, 10);

        }

    }
}
