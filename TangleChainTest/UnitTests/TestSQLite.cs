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

            block.CoinName = "LOL";
            block.Final();

            bool result = Db.AddBlock(block, false);

            Assert.IsFalse(result);


        }
        //[Test]
        //public void AddGetTransaction() {

        //    //connection
        //    DataBase_Lite Db = new DataBase_Lite("test");

        //    //create Trans
        //    Transaction trans = new Transaction("ME", 0, "CoolAddr");
        //    trans.AddFee(10);
        //    trans.AddOutput(100, "you");
        //    trans.Final();

        //    Db.AddTransaction(trans);

        //    Transaction result = Db.GetTransaction(trans.Hash,trans.SendTo);

        //    Assert.AreEqual(result, trans);

        //}

        [Test]
        public void Init() {

            DataBase_Lite Db = new DataBase_Lite("Test");

        }

    }
}
