using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Classes;
using static TangleChainIXI.Utils;
using TangleChainIXI;
using System.IO;
using System.Linq;
using FluentAssertions;
using TangleChainIXI.Smartcontracts;

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

            Block block = new Block(height, addr, name).Final();

            //DONT DO THIS NORMALLY. HACK!
            block.Difficulty = 2;

            DBManager.AddBlock(block);

            DBManager.GetBlock(name,block.Height).Should().Be(block);
            DBManager.GetBlock(name, -1).Should().BeNull();


            DBManager.DeleteBlock(name,height);
            DBManager.GetBlock(name, height).Should().BeNull();

        }

        [Test]
        public void DBExists() {

            IXISettings.Default(true);

            string name = GenerateRandomString(20);

            DataBase.Exists(name).Should().BeFalse();

            DBManager.GetLatestBlock(name);

            DataBase.Exists(name).Should().BeTrue();

        }

        [Test]
        public void UpdateBlock() {

            string name = GenerateRandomString(5);
            string addr = GenerateRandomString(81);
            long height = GenerateRandomInt(4);

            

            Block block = new Block(height, addr, name).Final();

            //HACK AGAIN, DONT DO THIS.
            block.Difficulty = 7;

            DBManager.AddBlock(block);

            block.Owner = "LOL";
            block.Final();

            DBManager.AddBlock(block);

            Block checkBlock = DBManager.GetBlock(name,block.Height);

            checkBlock.Print();
            block.Print();

            checkBlock.Should().Be(block);

        }

        [Test]
        public void LatestBlock()
        {

            long height = 1000000;

            Block block = new Block(height, "you", DataBaseName).Final().GenerateProofOfWork(2);

            DBManager.AddBlock(block);

            DBManager.GetLatestBlock(DataBaseName).Height.Should().Be(height);


        }

        [Test]
        public void AddBlockAndTransaction() {

            IXISettings.Default(true);

            Block block = (Block) new Block(100, "COOLADDRESS", DataBaseName)
            .Final();

            //DONT DO THIS. HACK!
            block.Difficulty = 2;

            DBManager.AddBlock(block);

            Transaction trans = (Transaction) new Transaction("ME", 1, GetTransactionPoolAddress(block.Height, DataBaseName))
                .AddFee(10)
                .AddOutput(10, "YOU")
                .AddOutput(10, "YOU2")
                .AddOutput(10, "YOU3")
                .Final();

            DBManager.AddTransaction(DataBaseName,trans, block.Height, null);

            DBManager.GetTransaction(DataBaseName,trans.Hash, block.Height).Should().Be(trans);

        }

        [Test]
        public void GetChainSettings() {

            ChainSettings settings = DBManager.GetChainSettings(DataBaseName);

            settings.Print();

            settings.BlockReward.Should().Be(100);
            settings.BlockTime.Should().Be(100);
            settings.TransactionPoolInterval.Should().Be(10);

        }

        [Test]
        public void TestAddSmartcontractToPool() {

            string DBName = GenerateRandomString(10);

            IXISettings.Default(true);

            Smartcontract smart = new Smartcontract("test", Utils.GenerateRandomString(81));

            smart.Final();
            
            //we add smartcontract to pool
            DBManager.AddSmartcontract(DBName, smart, null, 3);

            var result = DBManager.GetSmartcontractsFromTransPool(DBName, 3, 1).First();

            result.Should().Be(smart);


            //now we move the contract to a real block:
            Block block = new Block(3, GenerateRandomString(81), DBName);
            block.Final();

            DBManager.AddBlock(block);
            DBManager.AddSmartcontract(DBName, smart, 3, null);

            DBManager.GetSmartcontractsFromTransPool(DBName, 3, 1).Count.Should().Be(0);
            DBManager.GetSmartcontract(DBName, smart.ReceivingAddress).Should().Be(smart);

        }

    }
}
