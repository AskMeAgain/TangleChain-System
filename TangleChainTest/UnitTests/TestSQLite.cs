using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests
{
    [TestFixture]
    public class TestSQLite
    {


        [Test]
        public void InitDB() {

            string name = Utils.GenerateRandomString(7);

            DataBase_Lite Db = new DataBase_Lite(name);

            Console.WriteLine(name);

        }

        [Test]
        public void AddGetBlock() {
            
            string name = Utils.GenerateRandomString(5);
            string addr = Utils.GenerateRandomString(81);
            int height = Utils.GenerateRandomInt(4);

            DataBase_Lite Db = new DataBase_Lite(name);

            Block block = new Block(height,addr,name);         
            block.Final();

            bool flag = Db.AddBlock(block, false);

            Assert.IsTrue(flag);

            Block result = Db.GetBlock(block.Height);

            Assert.AreEqual(result, block);

        }

        [Test]
        public void UpdateBlock() {

            string name = Utils.GenerateRandomString(5);
            string addr = Utils.GenerateRandomString(81);
            int height = Utils.GenerateRandomInt(4);

            DataBase_Lite Db = new DataBase_Lite(name);

            Block block = new Block(height,addr,name);         
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

            string name = Utils.GenerateRandomString(5);
            Console.WriteLine(name);

            //we first create a block
            Block block = new Block(0, "COOLADDRESS", name);
            block.Final();

            DataBase_Lite Db = new DataBase_Lite(name);

            Db.AddBlock(block, false);

            Transaction trans = new Transaction("ME", 1, "COOLADDRESS");
            trans.AddFee(10);
            trans.AddOutput(10, "YOU");
            trans.AddOutput(10, "YOU2");
            trans.AddOutput(10, "YOU3");
            trans.Final();

            Db.AddTransaction(trans, block.Height);


        }

    }
}
