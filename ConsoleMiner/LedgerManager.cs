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

            (string addr, string hash) settings = (Settings.GenesisAddress, Settings.GenesisHash);

            //incase we already did some syncing before
            if (LatestBlock != null) {
                settings = (LatestBlock.SendTo,LatestBlock.Hash);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Block block = Core.DownloadChain(Settings.CoinName, settings.addr, settings.hash, true,
                (Block b) =>
                    Utils.Print("Downloaded Block Nr:" + b.Height + " in: " + stopwatch.Elapsed.ToString("mm\\:ss"),
                        false));

            stopwatch.Stop();

            Utils.Print("Blockchain is now synced in {0} seconds\n", false, stopwatch.Elapsed.ToString("mm\\:ss"));

            return block;

        }

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
            genesisTrans.Upload();
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
