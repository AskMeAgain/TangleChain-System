using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI;

namespace TangleChainIXITest
{


    public static class Initalizing
    {

        public static (string addr, string hash, string coinName, string dupHash) SetupCoreTest()
        {

            IXISettings.Default(true);

            //vars
            string coinName = Utils.GenerateRandomString(10);

            DBManager.SetChainSettings(coinName, new ChainSettings(100, -1, 0, 4, 100, 10, 10));

            Console.WriteLine("CoinName: " + coinName);

            int difficulty = 7;

            //we now fill the transpool
            string transactionPool = FillTransactionPool("Me", "YOU", 3, coinName, 1, DBManager.GetChainSettings(coinName).TransactionPoolInterval);
            List<Transaction> transList = Core.GetAllFromAddress<Transaction>(transactionPool);

            //we then generate a genesis block
            String sendto = (coinName + "_GENESIS").HashCurl(20);

            //add some money
            Transaction genesisTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, coinName))
                .SetGenesisInformation(DBManager.GetChainSettings(coinName))
                .AddOutput(10000, "ME")
                .Final()
                .Upload();

            //we then upload the block
            Block genesisBlock = new Block(0, sendto, coinName)
                .AddTransaction(genesisTrans)
                .Final()
                .GenerateProofOfWork(difficulty)
                .Upload();

            Console.WriteLine("\nAddress: " + genesisBlock.SendTo);

            //to mine a block on top we first create a block
            Block nextBlock = new Block(1, genesisBlock.NextAddress, coinName)
                .AddTransactions(transList.Take(DBManager.GetChainSettings(coinName).TransactionsPerBlock).ToList())
                .Final()
                .GenerateProofOfWork(7)
                .Upload();

            //we also need to add another block to genesis addr
            Block dupBlock = new Block(0, sendto, "DIFFERENT NAME")
                .Final()
                .GenerateProofOfWork(7)
                .Upload();

            return (genesisBlock.SendTo, genesisBlock.Hash, coinName, dupBlock.Hash);

        }

        private static string FillTransactionPool(string owner, string receiver, int numOfTransactions, string coinName, long height, int interval)
        {

            string num = height / interval * interval + "";
            string addr = (num + "_" + coinName.ToLower()).HashCurl(81);

            for (int i = 0; i < numOfTransactions; i++)
            {

                //we create now the transactions
                new Transaction(owner, 1, addr)
                    .AddOutput(100, receiver)
                    .AddFee(0)
                    .Final()
                    .Upload();

            }

            return addr;
        }

        public static string SetupDatabaseTest()
        {

            IXISettings.Default(true);
            string coinName = Utils.GenerateRandomString(10);

            //settings
            DBManager.SetChainSettings(coinName, new ChainSettings(100, -1, 0, 4, 100, 10, 10));

            //compute difficulty!
            int nextBlockDifficulty = 7;

            //create block first
            string transPool = Utils.GetTransactionPoolAddress(0, coinName);

            Transaction trans = new Transaction("ME", -1, transPool)
                .AddFee(0)
                .SetGenesisInformation(DBManager.GetChainSettings(coinName))
                .Final()
                .Upload();

            Block genesisBlock = new Block(0, Utils.GenerateRandomString(81), coinName)
                .AddTransaction(trans)
                .Final()
                .GenerateProofOfWork(nextBlockDifficulty);

            DBManager.Add(genesisBlock);

            return coinName;
        }
    }

}
