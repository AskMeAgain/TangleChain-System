using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Classes;
using static TangleChainIXI.Utils;
using TangleChainIXI;

namespace TangleChainIXITest.UnitTests {

    [TestFixture]
    public class TestDataBase {

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

            bool flag = Db.AddBlock(block, false);

            Assert.IsTrue(flag);

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

            Settings.Default(true);

            string name =  GenerateRandomString(5);
            Console.WriteLine(name);

            Block block = new Block(0, "COOLADDRESS", name);
            block.Final();

            DataBase Db = new DataBase(name);

            Db.AddBlock(block, false);

            Transaction trans = new Transaction("ME",1,  GetTransactionPoolAddress(block.Height, name));
            trans.AddFee(10);
            trans.AddOutput(10, "YOU");
            trans.AddOutput(10, "YOU2");
            trans.AddOutput(10, "YOU3");
            trans.Final();

            Db.AddTransaction(trans, block.Height);

            Transaction result = Db.GetTransaction(trans.Hash, block.Height);

            Assert.AreEqual(result, trans);


        }

        //[Test]
        public void GetChainSettings() {

            string name = "SAQVAD9ETH";

            DataBase Db = new DataBase(name);

            ChainSettings settings = Db.GetChainSettings();

            Assert.AreEqual(settings.BlockReward, 100);

        }

    }
}
