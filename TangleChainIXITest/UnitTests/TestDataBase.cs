﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Classes;
using static TangleChainIXI.Utils;
using TangleChainIXI;
using System.IO;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestDataBase {
        private string DataBaseName;

        [OneTimeSetUp]
        public void SetupChain() {

            DataBaseName = Initalizing.SetupDatabaseTest();

        }

        [OneTimeTearDown]
        public void Destroy() {

            DataBase Db = new DataBase(DataBaseName);
            Db.DeleteDatabase();

        }

        [Test]
        public void InitDB() {

            string name =  GenerateRandomString(7);

            DataBase Db = new DataBase(name);

            Console.WriteLine(name);

        }

        [Test]
        public void AddGetBlock() {

            string name = GenerateRandomString(5);
            string addr =  GenerateRandomString(81);
            long height =  GenerateRandomInt(4);

            DataBase Db = new DataBase(name);

            Block block = new Block(height, addr, name);
            block.Final();
        
            //DONT DO THIS. HACK!
            block.Difficulty = new Difficulty(2);

            bool flag = Db.AddBlock(block, false);

            Assert.IsFalse(flag);

            Block result = Db.GetBlock(block.Height);

            Assert.AreEqual(result, block);

        }

        [Test]
        public void UpdateBlock() {

            string name =  GenerateRandomString(5);
            string addr =  GenerateRandomString(81);
            long height =  GenerateRandomInt(4);

            DataBase Db = new DataBase(name);

            Block block = new Block(height, addr, name);
            block.Final();

            //HACK AGAIN, DONT DO THIS.
            block.Difficulty = new Difficulty();

            Db.AddBlock(block, false);

            block.Owner = "LOL";
            block.Final();

            bool result = Db.AddBlock(block, false);

            Assert.IsFalse(result);

            Block checkBlock = Db.GetBlock(block.Height);

            checkBlock.Print();
            block.Print();

            Assert.AreEqual(checkBlock, block);

        }

        [Test]
        public void AddBlockAndTransaction() {

            IXISettings.Default(true);

            Block block = new Block(100, "COOLADDRESS", DataBaseName);
            block.Final();

            //DONT DO THIS. HACK!
            block.Difficulty = new Difficulty(2);

            DataBase Db = new DataBase(DataBaseName);

            Db.AddBlock(block, false);

            Transaction trans = new Transaction("ME",1,  GetTransactionPoolAddress(block.Height, DataBaseName));
            trans.AddFee(10);
            trans.AddOutput(10, "YOU");
            trans.AddOutput(10, "YOU2");
            trans.AddOutput(10, "YOU3");
            trans.Final();

            Db.AddTransaction(trans, block.Height,null);

            Transaction result = Db.GetTransaction(trans.Hash, block.Height);

            Assert.AreEqual(result, trans);


        }

        [Test]
        public void GetChainSettings() {

            DataBase Db = new DataBase(DataBaseName);

            ChainSettings settings = Db.GetChainSettings();

            settings.Print();

            Assert.AreEqual(settings.BlockReward, 100);
            Assert.AreEqual(settings.BlockTime, 100);
            Assert.AreEqual(settings.TransactionPoolInterval, 10);

        }

        [Test]
        public void DeleteDatabase() {

            DataBase Db = new DataBase("test");

            Db.DeleteDatabase();

            Assert.IsFalse(Directory.Exists($@"{IXISettings.StorePath}test\"));

        }

    }
}
