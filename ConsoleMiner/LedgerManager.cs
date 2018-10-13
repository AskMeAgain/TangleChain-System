using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TangleChainIXI.Classes;
using TangleChainIXI;

namespace ConsoleMiner
{
    public class LedgerManager
    {
        public Block LatestBlock { get; set; }
        public Settings Settings { get; set; }

        public LedgerManager(Settings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Synchronizes the chain and 
        /// returns the latest block and outputs
        /// </summary>
        /// <returns></returns>
        public Block SyncChain()
        {

            Utils.Print("Synchronization of Chain started", false);

            LatestBlock = DBManager.GetLatestBlock(Settings.CoinName);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Block block = Core.DownloadChain(Settings.CoinName, Settings.GenesisAddress, Settings.GenesisHash, true, (Block b) =>
                Utils.Print("Downloaded Block Nr:" + b.Height + " in: " + stopwatch.Elapsed.ToString("mm\\:ss"), false));

            stopwatch.Stop();

            Utils.Print("Blockchain is now synced in {0} seconds\n", false, stopwatch.Elapsed.ToString("mm\\:ss"));

            return block;

        }

        //private static void FillPool(int numOfRounds)
        //{

        //    //setup
        //    if (!Startup())
        //    {
        //        return;
        //    }

        //    //first we get the chain settings
        //    Utils.Print("Downloading Block", false);
        //    Block genesis = Core.GetSpecificFromAddress<Block>(InitSettings.MinedChain.Address, InitSettings.MinedChain.Hash);

        //    Utils.Print("Getting Chainsettings now", false);
        //    DBManager.AddBlock(genesis);
        //    Utils.Print("Chain IXISettings Received", false);

        //    //we then compute stuff about the process
        //    ChainSettings cSett = DBManager.GetChainSettings(InitSettings.MinedChain.CoinName);
        //    int numOfTransPerInterval = cSett.TransactionsPerBlock * cSett.TransactionPoolInterval;

        //    Utils.Print("Uploading {0} x {1} Transactions now\n", numOfTransPerInterval.ToString(), numOfRounds.ToString(), false);

        //    //create the transactions
        //    for (int i = 1; i <= numOfRounds; i++)
        //    {

        //        int height = i * cSett.TransactionPoolInterval;
        //        string poolAddress = IXI.Utils.GetTransactionPoolAddress(height, InitSettings.MinedChain.CoinName);
        //        //to fill with transactions:
        //        //generate transaction first
        //        for (int ii = 0; ii < cSett.TransactionsPerBlock; ii++)
        //        {
        //            Transaction trans = new Transaction(IXISettings.GetPublicKey(), 1, poolAddress)
        //                .AddFee(0)
        //                .AddOutput(10, "YOU!!!!!!!")
        //                .Final()
        //                .Upload();
        //        }

        //        //string s = IXI.Core.FillTransactionPool(InitSettings.PublicKey, InitSettings.PublicKey, numOfTransPerInterval,
        //        //  InitSettings.MinedChain.CoinName, height);
        //        Utils.Print("Pool address: {0} of height {1}" + InitSettings.MinedChain.CoinName, poolAddress, height.ToString(), false);
        //    }

        //    Utils.Print("\nFinished filling transpool. Press key to exit program", true);

        //}

        public static bool InitGenesisProcess()
        {

            Utils.Print("Enter Coinname", false);
            string name = Console.ReadLine();

            Utils.Print("Enter Block Reward for Miner", false);
            if (!Int32.TryParse(Console.ReadLine(), out int reward))
                return false;

            Utils.Print("Enter Block Reduction Interval", false);
            if (!Int32.TryParse(Console.ReadLine(), out int reductionInterval))
                return false;

            Utils.Print("Enter Block Reduction Factor from 0-100 %", false);
            if (!Int32.TryParse(Console.ReadLine(), out int factor))
                return false;

            Utils.Print("Enter Blocksize", false);
            if (!Int32.TryParse(Console.ReadLine(), out int blockSize))
                return false;

            Utils.Print("Enter Block Time in seconds", false);
            if (!Int32.TryParse(Console.ReadLine(), out int blockTime))
                return false;

            Utils.Print("Enter Transaction Pool Interval", false);
            if (!Int32.TryParse(Console.ReadLine(), out int transInterval))
                return false;

            Utils.Print("Enterintadjustment number", false);
            if (!Int32.TryParse(Console.ReadLine(), out int difficultyAdj))
                return false;

            Transaction genesisTrans = new Transaction("GENESIS", 1, TangleChainIXI.Utils.GetTransactionPoolAddress(0, name));
            genesisTrans.SetGenesisInformation(reward, reductionInterval, factor, blockSize, blockTime, transInterval, difficultyAdj);
            genesisTrans.Final();

            Utils.Print("Uploading Genesis Transaction to {0}", false, genesisTrans.SendTo);
            TangleChainIXI.Core.Upload(genesisTrans);
            Utils.Print("Finished Uploading Genesis Transaction", false);

            //we construct genesis block first and upload it
            Block genesis = new Block(0, (name + "_GENESIS").HashCurl(81), name);

            genesis.AddTransaction(genesisTrans)
                .Final()
                .Print("Computing POW for block")
                .GenerateProofOfWork(7)
                .Print("Uploading Block")
                .Upload();



            Utils.Print("Genesis Block successfully created. Press any key to exit program", true);

            return true;

        }
    }
}
