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


        public static Block LatestBlock;
        public static Block NewConstructedBlock;
        public static Settings InitSettings;

        public static bool constructNewBlockFlag = false;

        static void Main(string[] args) {

            Console.Title = "ConsoleMiner";
            Console.WriteLine("ConsoleMiner started\n");

            if (args[0].Equals("exit")) {
                Exit();
            }

            if (args[0].Equals("run")) {

                if (args.Length > 1 && args[1].Equals("genesis"))
                    Runner(true);
                else
                    Runner(false);
            }

            if (args[0].Equals("sync")) {
                if (!Startup()) {
                    return;
                }

                SyncChain();
            }

            if (args[0].Equals("init")) {

                if (File.Exists(Environment.CurrentDirectory + @"/init.json")) {
                    Console.WriteLine("Init file already exists. Press any key to exit program");
                } else {
                    Utils.CreateInitFile();
                    Console.WriteLine("Init File Created. Press any key to exit program");
                }

                Console.ReadKey();
            }

            if (args[0].Equals("help")) {
                Console.WriteLine("The following commands are possible: \n run \n exit \n init \n help \n");
            }

            if (args[0].Equals("balance") && args.Length == 3) {

                //balance coinname pubkey
                IXI.DataBase Db = new IXI.DataBase(args[1]);

                Console.WriteLine("The Balance of Address {0} is {1}. Press any key to exit program.\n", args[2], Db.GetBalance(args[2]));
                Console.ReadKey();
            }

            if (args[0].Equals("publickey") && args.Length == 2) {
                //publickey asdd
                Console.WriteLine("Your Public key is {0}. Press any key to exit program.\n",
                    IXI.Cryptography.GetPublicKey(args[1]));
                Console.ReadKey();

            }


        }

        private static bool InitGenesisProcess() {

            Console.WriteLine("Enter Coinname");
            string name = Console.ReadLine();

            Console.WriteLine("Enter Block Reward for Miner");
            if (!int.TryParse(Console.ReadLine(), out int reward))
                return false;

            Console.WriteLine("Enter Block Reduction Interval");
            if (!int.TryParse(Console.ReadLine(), out int reductionInterval))
                return false;

            Console.WriteLine("Enter Block Reduction Factor from 0-100 %");
            if (!int.TryParse(Console.ReadLine(), out int factor))
                return false;

            Console.WriteLine("Enter Blocksize");
            if (!int.TryParse(Console.ReadLine(), out int blockSize))
                return false;

            Console.WriteLine("Enter Block Time in seconds");
            if (!int.TryParse(Console.ReadLine(), out int blockTime))
                return false;

            Console.WriteLine("Enter Transaction Pool Interval");
            if (!int.TryParse(Console.ReadLine(), out int transInterval))
                return false;

            Transaction genesisTrans = new Transaction("GENESIS", 1, IXI.Utils.GetTransactionPoolAddress(0, name));
            genesisTrans.SetGenesisInformation(reward, reductionInterval, factor, blockSize, blockTime, transInterval);

            Console.WriteLine("Uploading Genesis Transaction");
            IXI.Core.UploadTransaction(genesisTrans);
            Console.WriteLine("Finished Uploading Genesis Transaction");

            //we construct genesis block first and upload it
            Block genesis = new Block(0, IXI.Utils.HashCurl(name + "_GENESIS", 81), name);
            genesis.AddTransactions(genesisTrans);
            genesis.Final();
            Console.WriteLine("Computing POW for block");
            genesis.GenerateProofOfWork(5);
            Console.WriteLine("Uploading Block");
            IXI.Core.UploadBlock(genesis);

            //we now change the init 
            InitSettings.Chain = (genesis.Hash, genesis.SendTo, name);
            Utils.ChangeInitFile(InitSettings);

            return true;

        }

        static void Exit() {
            Utils.DeleteFlag();

            if (!Utils.FlagIsSet())
                Environment.Exit(0);
        }

        static bool LoadedSettings() {

            if (!File.Exists(Environment.CurrentDirectory + @"/init.json"))
                return false;

            InitSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Environment.CurrentDirectory + @"/init.json"));

            IXI.Classes.Settings.PublicKey = InitSettings.PublicKey;

            if (InitSettings.NodeAddress == null)
                return false;

            return true;

        }

        static bool Startup() {

            //let the process only run if not another process is running
            if (Utils.FlagIsSet()) {
                if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                    Environment.Exit(0);
            }

            //load settings
            if (!LoadedSettings()) {
                Console.WriteLine("init.json file is not complete or missing. Press Key to exit program");
                Console.ReadKey();
                return false;
            }

            if (!IsConnectionEstablished()) {
                Console.WriteLine("Connection Failed to Node. Press any key to exit program");
                Console.ReadKey();
                return false;
            }

            //mark spot so running another instance is not possible
            Utils.WriteFlag();

            return true;

        }

        private static void Runner(bool startFromGenesis) {

            if (!Startup()) {
                return;
            }

            if (startFromGenesis) {
                if (!InitGenesisProcess()) {
                    Console.WriteLine("You entered wrong Information. Press any key to exit program");
                    Console.ReadKey();
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

        private static void SyncChain() {
            //Sync the blockchain
            IXI.DataBase Db = new IXI.DataBase(InitSettings.Chain.CoinName);
            LatestBlock = Db.GetLatestBlock();

            if (LatestBlock == null)
                DownloadingChain(InitSettings.Chain.Hash, InitSettings.Chain.Address);
            else
                DownloadingChain(LatestBlock.Hash, LatestBlock.SendTo);
        }

        private static bool IsConnectionEstablished() {
            Console.WriteLine("Testing Connection...");

            foreach (string s in InitSettings.NodeAddress) {
                if (IXI.Utils.TestConnection(s)) {
                    IXI.Classes.Settings.SetNodeAddress(s);
                    Console.WriteLine("Connection established\n");
                    return true;
                }
            }

            return false;
        }

        private static void DownloadingChain(string hash, string addr) {

            Console.WriteLine("Synchronization of Chain started");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            LatestBlock = IXI.Core.DownloadChain(addr, hash, 5, true, (Block b) => Console.WriteLine("Downloaded Block Nr:" + b.Height + " in: " + stopwatch.Elapsed.ToString("mm\\:ss")));

            stopwatch.Stop();

            Console.WriteLine("Blockchain is now synced in {0} seconds\n", stopwatch.Elapsed.ToString("mm\\:ss"));
        }

        private static CancellationTokenSource GetLatestBlockThread() {

            CancellationTokenSource source = new CancellationTokenSource();

            Thread t = new Thread(() => {

                Console.WriteLine("Starting Block Download Thread");

                CancellationToken token = source.Token;

                while (!token.IsCancellationRequested) {

                    int milliseconds = 5000;
                    Thread.Sleep(milliseconds);

                    if (token.IsCancellationRequested)
                        break;

                    Block downloadedBlock = IXI.Core.DownloadChain(LatestBlock.SendTo, LatestBlock.Hash, 5, true, null);

                    //we found a new block!
                    if (downloadedBlock.Height > LatestBlock.Height && downloadedBlock != null) {

                        Console.WriteLine("... We just found a new block! Old Height: {0} ... New Height {1}",
                            LatestBlock.Height, downloadedBlock.Height);

                        LatestBlock = downloadedBlock;
                        constructNewBlockFlag = true;
                    } else {
                        Console.WriteLine("... Chain up to date");
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

                Console.WriteLine("Starting Block Construction Thread");

                CancellationToken token = source.Token;

                int numOfTransactions = -1;

                while (!token.IsCancellationRequested) {

                    int milliseconds = 3000;
                    Thread.Sleep(milliseconds);

                    //stop thread
                    if (token.IsCancellationRequested)
                        break;

                    Console.WriteLine("...");
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

                        newestBlock.AddTransactions(transList.Take(3).ToList());
                        newestBlock.Final();

                        if (token.IsCancellationRequested)
                            break;

                        NewConstructedBlock = newestBlock;

                        Console.WriteLine("Block Nr {0} is constructed", NewConstructedBlock.Height);
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

                    Console.WriteLine("Starting POW Thread");

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
                            Console.WriteLine("... ... New Block got found. Preparing for Upload");
                            checkBlock.Nonce = nonce;
                            IXI.Core.UploadBlock(checkBlock);
                            Console.WriteLine("... ... Upload Finished");
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

    }
}
