using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TangleChainIXI.Classes;
using TangleChainIXI;
using TangleChainIXI.Classes.Helper;
using IXIComponents.Simple;
using IXIUtils = TangleChainIXI.Classes.Helper.Utils;


namespace ConsoleMiner
{
    public class LedgerManager
    {
        public Block LatestBlock { get; set; }
        public Settings Settings { get; set; }

        private readonly IXICore _ixiCore;
        private readonly IXISettings _ixiSettings;

        public LedgerManager(Settings settings, IXICore ixicore, IXISettings ixisettings)
        {
            Settings = settings;

            _ixiSettings = ixisettings;
            _ixiSettings.SetNodeAddress(settings.NodeList.First());

            _ixiCore = ixicore;
        }

        /// <summary>
        /// Synchronizes the chain and 
        /// returns the latest block and outputs
        /// </summary>
        /// <returns></returns>
        public Block SyncChain()
        {

            Utils.Print("Synchronization of Chain started", false);

            var maybeBlock = _ixiCore.GetLatestBlock();

            string addr, hash;
            if (maybeBlock.HasValue)
            {
                addr = maybeBlock.Value.SendTo;
                hash = maybeBlock.Value.Hash;
            }
            else
            {
                addr = Settings.GenesisAddress;
                hash = Settings.GenesisHash;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Block block = _ixiCore.DownloadChain(addr, hash,
                (Block b) =>
                    Utils.Print("Downloaded Block Nr:" + b.Height + " in: " + stopwatch.Elapsed.ToString("mm\\:ss"),
                        false));

            stopwatch.Stop();

            Utils.Print("Blockchain is now synced in {0} seconds\n", false, stopwatch.Elapsed.ToString("mm\\:ss"));

            return block;

        }

        public bool InitGenesisProcess()
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

            Transaction genesisTrans = new Transaction("GENESIS", 1, TangleChainIXI.Classes.Helper.Utils.GetTransactionPoolAddress(0, name));
            genesisTrans.SetGenesisInformation(reward, reductionInterval, factor, blockSize, blockTime, transInterval, difficultyAdj);
            genesisTrans.Final(_ixiSettings);

            Utils.Print("Uploading Genesis Transaction to {0}", false, genesisTrans.SendTo);
            genesisTrans.Upload();
            Utils.Print("Finished Uploading Genesis Transaction", false);

            //we construct genesis block first and upload it
            Block genesis = new Block(0, Hasher.Hash(81, name, "_GENESIS"), name);

            genesis.Add(genesisTrans)
                .Final(_ixiSettings)
                .Print("Computing POW for block")
                .GenerateProofOfWork(_ixiCore)
                .Print("Uploading Block")
                .Upload();

            Utils.Print("Genesis Block successfully created. Press any key to exit program", true);

            return true;

        }

        public Block CreateGenesis(string[] args)
        {
            var argsList = args.ToList();
            argsList.RemoveAt(0);


            var blockAddr = IXIUtils.GenerateRandomString(81);
            var genesisBlock = new Block(0, blockAddr, Settings.CoinName);

            var poolAddr = IXIUtils.GetTransactionPoolAddress(0, Settings.CoinName);
            var genesisTrans = new Transaction("me", -1, poolAddr)
                .SetGenesisInformation(new ChainSettings(argsList.ToArray()))
                .Final(_ixiSettings)
                .Upload();

            genesisBlock.Add(genesisTrans)
                .Final(_ixiSettings)
                .GenerateProofOfWork(_ixiCore)
                .Upload();

            Console.WriteLine("Genesis Block at: {0} \nwith Hash {1}", genesisBlock.SendTo, genesisBlock.Hash);

            return genesisBlock;
        }
    }
}
