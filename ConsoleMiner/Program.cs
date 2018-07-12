using System;
using IXI = TangleChainIXI;
using TangleChainIXI.Classes;
using System.Linq;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace ConsoleMiner {
    class Program {

        private static Block LatestBlock;
        private static Block NewConstructedBlock;
        private static Settings InitSettings;

        private static bool constructNewBlockFlag = false;
        private static bool printInConsoleFlag = true;

        public static void Main(string[] args) {

            Console.Title = "ConsoleMiner";
            Print("ConsoleMiner started\n", false);

            if (args.Contains("--NoPrint"))
                printInConsoleFlag = false;

            switch (args[0].ToLower()) {
                case "fill":
                    FillPool(int.Parse(args[1]));
                    break;

                case "run":
                    if (args.Length > 1 && args[1].Equals("genesis"))
                        Runner(true);
                    else
                        Runner(false);
                    break;

                case "exit":
                    Exit();
                    break;

                case "addkey":
                    if (args.Length == 3) {

                        Settings settings = Utils.ReadInitFile();

                        if (settings == null) {
                            Print("Init file is missing. Press any key to exit program", true);
                            break;
                        }

                        if (args[1].Equals("-priv")) {
                            settings.PrivateKey = args[2];
                            settings.PublicKey = IXI.Cryptography.GetPublicKey(args[2]);
                        } else if (args[1].Equals("-pub")) {

                            //i dont know the length of an ethereum key so i use this hack 
                            if (args[2].Length == IXI.Cryptography.GetPublicKey("test").Length) {
                                settings.PublicKey = args[2];
                            } else {
                                Print("Your public key is to short/long. Press any key to exit program", true);
                            }
                        }

                        Utils.WriteInitFile(settings);
                        Print("Public key got added. Press any key to exit program", true);
                    }
                    break;

                case "sync":
                    if (!Startup()) {
                        return;
                    }
                    SyncChain();
                    break;

                case "init":
                    if (File.Exists(Environment.CurrentDirectory + @"/init.json")) {
                        Print("Init file already exists. Press any key to exit program", true);
                    } else {
                        Utils.CreateInitFile();
                        Print("Init File Created. Press any key to exit program", true);
                    }
                    break;

                case "help":
                    Print("The following commands are possible: \n run \n exit \n init \n help \n", false);
                    break;

                case "balance":

                    Settings set = Utils.ReadInitFile();
                    if (set == null) {
                        Print("Settings file not set. Press any key to exit program", true);
                        break;
                    }

                    if ((args.Length == 1)) {
                        IXI.DataBase Db = new IXI.DataBase(set.MinedChain.CoinName);
                        Print("The Balance of Address {0} is {1}. Press any key to exit program.\n", set.PublicKey,
                            Db.GetBalance(set.PublicKey).ToString(), true);
                    }

                    if (args.Length == 3) {
                        //balance pubkey
                        IXI.DataBase Db = new IXI.DataBase(set.MinedChain.CoinName);

                        Print("The Balance of Address {0} is {1}. Press any key to exit program.\n", args[2], Db.GetBalance(args[2]).ToString(), true);
                    }
                    break;

                case "publickey":
                    if (args.Length == 2) {
                        Print("Your Public key is {0}. Press any key to exit program.\n",
                            IXI.Cryptography.GetPublicKey(args[1]), true);
                    }
                    break;

            }
        }

        #region executable

        private static void Runner(bool startFromGenesis) {

            if (!Startup()) {
                return;
            }

            if (startFromGenesis) {
                if (!InitGenesisProcess()) {
                    Print("You entered wrong Information. Press any key to exit program", true);
                    return;
                }
            }

            //we need to sync the chain
            SyncChain();

            //Start All needed Threads
            //Start POW
            CancellationTokenSource POWsource = StartPOWThreads();

            //Start construction block
            CancellationTokenSource constructBlockSource = ConstructBlockThread();

            //Start Get newest DATA thread
            CancellationTokenSource latestBlocksource = GetLatestBlockThread();

            //our exit
            while (true) {

                int milliseconds = 500;
                Thread.Sleep(milliseconds);

                if (!Utils.FlagIsSet()) {

                    constructBlockSource.Cancel();
                    latestBlocksource.Cancel();
                    POWsource.Cancel();

                    Environment.Exit(0);
                }
            }
        }

        private static void Exit() {
            Utils.DeleteFlag();

            if (!Utils.FlagIsSet())
                Environment.Exit(0);
        }

        private static void SyncChain() {

            Print("Synchronization of Chain started", false);

            //Sync the blockchain
            IXI.DataBase Db = new IXI.DataBase(InitSettings.MinedChain.CoinName);
            LatestBlock = Db.GetLatestBlock();

            Stopwatch stopWatch;

            if (LatestBlock == null)
                stopWatch = DownloadingChain(InitSettings.MinedChain.Hash, InitSettings.MinedChain.Address);
            else
                stopWatch = DownloadingChain(LatestBlock.Hash, LatestBlock.SendTo);

            Print("Blockchain is now synced in {0} seconds\n", stopWatch.Elapsed.ToString("mm\\:ss"), false);

        }

        private static void FillPool(int n) {

            //setup
            if (!Startup()) {
                return;
            }

            //first we get the chain settings
            Print("Downloading Block", false);

            Block genesis = IXI.Core.GetSpecificBlock(InitSettings.MinedChain.Address, InitSettings.MinedChain.Hash, -1, false);

            Print("Getting Chainsettings", false);
            IXI.DataBase Db = new IXI.DataBase(InitSettings.MinedChain.CoinName);
            Db.AddBlock(genesis, true);

            var cSett = Db.GetChainSettings();
            Print("Chain Settings Received", false);
            //we then compute stuff about the process
            int numOfTransPerInterval = cSett.TransactionsPerBlock * n;

            Print("Uploading Transactions", false);

            //create the transactions
            for (int i = 0; i < n; i++) {
                string s = IXI.Utils.FillTransactionPool(InitSettings.PublicKey, InitSettings.PublicKey, numOfTransPerInterval,
                    InitSettings.MinedChain.CoinName, 1 + i * cSett.TransactionPoolInterval);
                Print("Pool address: {0}", s, false);
            }

            Print("Finished filling transpool. Press key to exit program", true);

        }

        #endregion

        #region helper

        private static bool InitGenesisProcess() {

            Print("Enter Coinname", false);
            string name = Console.ReadLine();

            Print("Enter Block Reward for Miner", false);
            if (!int.TryParse(Console.ReadLine(), out int reward))
                return false;

            Print("Enter Block Reduction Interval", false);
            if (!int.TryParse(Console.ReadLine(), out int reductionInterval))
                return false;

            Print("Enter Block Reduction Factor from 0-100 %", false);
            if (!int.TryParse(Console.ReadLine(), out int factor))
                return false;

            Print("Enter Blocksize", false);
            if (!int.TryParse(Console.ReadLine(), out int blockSize))
                return false;

            Print("Enter Block Time in seconds", false);
            if (!int.TryParse(Console.ReadLine(), out int blockTime))
                return false;

            Print("Enter Transaction Pool Interval", false);
            if (!int.TryParse(Console.ReadLine(), out int transInterval))
                return false;

            Transaction genesisTrans = new Transaction("GENESIS", 1, IXI.Utils.GetTransactionPoolAddress(0, name));
            genesisTrans.SetGenesisInformation(reward, reductionInterval, factor, blockSize, blockTime, transInterval);
            genesisTrans.Final();

            Print("Uploading Genesis Transaction to {0}", genesisTrans.SendTo, false);
            IXI.Core.UploadTransaction(genesisTrans);
            Print("Finished Uploading Genesis Transaction", false);

            //we construct genesis block first and upload it
            Block genesis = new Block(0, IXI.Utils.HashCurl(name + "_GENESIS", 81), name);
            genesis.AddTransactions(genesisTrans);
            genesis.Final();
            Print("Computing POW for block", false);
            genesis.GenerateProofOfWork(5);
            Print("Uploading Block", false);
            IXI.Core.UploadBlock(genesis);

            //we now change the init 
            InitSettings.MinedChain = new Settings.Chain(genesis.Hash, genesis.SendTo, name);
            Utils.WriteInitFile(InitSettings);

            return true;

        }

        private static bool LoadedSettings() {

            if (!File.Exists(Environment.CurrentDirectory + @"/init.json"))
                return false;

            InitSettings = Utils.ReadInitFile();

            IXI.Classes.Settings.PublicKey = InitSettings.PublicKey;

            //you dont have to set private key here. 
            if (InitSettings.PrivateKey != null)
                IXI.Classes.Settings.SetPrivateKey(InitSettings.PrivateKey);

            if (InitSettings.NodeAddress == null)
                return false;

            return true;

        }

        private static bool Startup() {

            //let the process only run if not another process is running
            if (Utils.FlagIsSet()) {
                if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                    Environment.Exit(0);
            }

            //load settings
            if (!LoadedSettings()) {
                Print("init.json file is not complete or missing. Press Key to exit program", true);
                return false;
            }

            if (!IsConnectionEstablished()) {
                Print("Connection Failed to Node. Press any key to exit program", true);
                return false;
            }

            //mark spot so running another instance is not possible
            Utils.WriteFlag();

            return true;

        }

        private static bool IsConnectionEstablished() {
            Print("Testing Connection...", false);

            foreach (string s in InitSettings.NodeAddress) {
                if (IXI.Utils.TestConnection(s)) {
                    IXI.Classes.Settings.SetNodeAddress(s);
                    Print("Connection established\n", false);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Threads

        private static CancellationTokenSource GetLatestBlockThread() {

            CancellationTokenSource source = new CancellationTokenSource();

            Thread t = new Thread(() => {

                Print("Starting Block Download Thread", false);

                CancellationToken token = source.Token;

                while (!token.IsCancellationRequested) {

                    int milliseconds = 5000;
                    Thread.Sleep(milliseconds);

                    if (token.IsCancellationRequested)
                        break;

                    Block downloadedBlock = IXI.Core.DownloadChain(LatestBlock.SendTo, LatestBlock.Hash, 5, true, null);

                    //we found a new block!
                    if (downloadedBlock.Height > LatestBlock.Height && downloadedBlock != null) {

                        Print("... We just found a new block! Old Height: {0} ... New Height {1}",
                            LatestBlock.Height.ToString(), downloadedBlock.Height.ToString(), false);

                        LatestBlock = downloadedBlock;
                        constructNewBlockFlag = true;
                    } else {
                        Print("... Chain up to date", false);
                    }

                }

            }) {
                IsBackground = true
            };
            t.Start();

            return source;

        }

        private static CancellationTokenSource ConstructBlockThread() {

            CancellationTokenSource source = new CancellationTokenSource();

            Thread t = new Thread(() => {

                Print("Starting Block Construction Thread", false);

                CancellationToken token = source.Token;

                int numOfTransactions = -1;

                while (!token.IsCancellationRequested) {

                    int milliseconds = 3000;
                    Thread.Sleep(milliseconds);

                    //stop thread
                    if (token.IsCancellationRequested)
                        break;

                    Print("...", false);
                    List<Transaction> transList = IXI.Core.GetAllTransactionsFromAddress(
                        IXI.Utils.GetTransactionPoolAddress(LatestBlock.Height, LatestBlock.CoinName));

                    //means we didnt changed anything && we dont need to construct a new block
                    if (numOfTransactions == transList.Count && !constructNewBlockFlag)
                        continue;

                    if ((constructNewBlockFlag && NewConstructedBlock.Height <= LatestBlock.Height) || NewConstructedBlock == null) {
                        //if newconstr. is null then we definitly need to construct one

                        constructNewBlockFlag = false;
                        numOfTransactions = transList.Count;

                        transList.OrderByDescending(m => m.ComputeOutgoingValues());

                        Block newestBlock = new Block(LatestBlock.Height + 1, LatestBlock.NextAddress,
                            LatestBlock.CoinName);

                        IXI.DataBase Db = new IXI.DataBase(InitSettings.MinedChain.CoinName);
                        var cSett = Db.GetChainSettings();

                        //TODO fill block with correct transactions

                        newestBlock.AddTransactions(transList.Take(cSett.TransactionsPerBlock).ToList());
                        newestBlock.Final();

                        if (token.IsCancellationRequested)
                            break;

                        NewConstructedBlock = newestBlock;

                        Print("Block Nr {0} is constructed", NewConstructedBlock.Height.ToString(), false);
                    }
                }

            }) {
                IsBackground = true
            };
            t.Start();

            return source;
        }

        private static CancellationTokenSource StartPOWThreads() {

            CancellationTokenSource source = new CancellationTokenSource();

            for (int i = 0; i < InitSettings.CoreNumber; i++) {
                Thread t = new Thread(() => {

                    Print("Starting POW Thread", false);

                    CancellationToken token = source.Token;

                    int nonce = 0;
                    Block checkBlock = NewConstructedBlock;

                    while (!token.IsCancellationRequested) {

                        nonce++;

                        if (nonce % 50 == 0) {
                            checkBlock = NewConstructedBlock;
                        }

                        if (checkBlock == null || constructNewBlockFlag) {
                            continue;
                        }


                        if (IXI.Utils.VerifyHash(checkBlock.Hash, nonce, 5)) {
                            Print("... ... New Block got found. Preparing for Upload", false);
                            checkBlock.Nonce = nonce;
                            IXI.Core.UploadBlock(checkBlock);
                            Print("... ... Upload Finished", false);
                            constructNewBlockFlag = true;
                            nonce = 0;
                        }

                    }

                }) {
                    IsBackground = true
                };
                t.Start();
            }

            return source;
        }

        #endregion 

        #region Utils

        private static Stopwatch DownloadingChain(string hash, string addr) {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Action<Block> action = (Block b) =>
                Print("Downloaded Block Nr:" + b.Height + " in: " + stopwatch.Elapsed.ToString("mm\\:ss"), false);

            LatestBlock = IXI.Core.DownloadChain(addr, hash, 5, true, action);

            stopwatch.Stop();

            return stopwatch;
        }

        public static void Print(string text, bool readKey) {

            if (printInConsoleFlag)
                Console.WriteLine(text);
            if (readKey)
                Console.ReadKey();
        }

        public static void Print(string text, string para1, string para2, bool readKey) {
            string s = string.Format(text, para1, para2);
            Print(s, readKey);
        }

        public static void Print(string text, string para1, bool readKey) {
            string s = string.Format(text, para1);
            Print(s, readKey);
        }

        #endregion
    }
}
