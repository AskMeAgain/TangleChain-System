using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI;

namespace TangleChainIXITest.CompleteExamples {

    [TestFixture]
    public class Example01 {

        [Test]
        public void StartExample01() {

            //this example creates a chain, generates a genesis transaction, mines a block ontop of the genesis block and adds 3 transactions.

            Settings.Default(true);

            //vars
            string name = Utils.GenerateRandomString(10);
            string transactionPool = Utils.FillTransactionPool(3, name, 1);
            int difficulty = 5;

            Console.WriteLine("CoinName:" + name);

            //we fill the transaction pool with some transactions        
            var transList = Core.GetAllTransactionsFromAddress(transactionPool);

            //we then generate a genesis block
            String sendto = Utils.HashCurl(name + "_GENESISI", 81);
            Block genesisBlock = new Block(0, sendto, name);

            //add some money
            Transaction genesisTrans = new Transaction("ME", -1, transactionPool);
            genesisTrans.AddFee(0);
            genesisTrans.AddOutput(10000, "ME");
            genesisTrans.Final();


            Core.UploadTransaction(genesisTrans);

            //we then upload the block
            genesisBlock.AddTransactions(genesisTrans);
            genesisBlock.Final();
            genesisBlock.GenerateProofOfWork(difficulty);
            Core.UploadBlock(genesisBlock);

            Console.WriteLine("\nAddress: " + genesisBlock.SendTo + "\nTransactionPool: " + transactionPool);

            //to mine a block on top we first create a block
            Block nextBlock = new Block(1, genesisBlock.NextAddress, name);

            //we then fill this block with transactions
            nextBlock.AddTransactions(transList.Take(Settings.NumberOfTransactionsPerBlock).ToList());

            //upload block
            nextBlock.Final();
            nextBlock.GenerateProofOfWork(difficulty);
            Core.UploadBlock(nextBlock);

            //we now store the blocks in a DB
            DataBase Db = new DataBase(name);
            Db.AddBlock(genesisBlock, true);
            Db.AddBlock(nextBlock, true);


        }
    }

}
