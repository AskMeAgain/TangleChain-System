using System;
using IXI = TangleChainIXI;
using TangleChainIXI.Classes;
using System.Linq;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace ConsoleMiner {
    class Program {

        static void Main(string[] args) {

            if (args[0].Equals("exit")) {
                Exit();
            }

            if (args[0].Equals("run")) {
                Run();
            }

            if (args[0].Equals("init")) {
                Utils.CreateInitFile();
            }

        }

        static void Exit() {
            Utils.DeleteFlag();

            if (!Utils.CheckFlag())
                Environment.Exit(0);
        }

        public static Settings InitSettings;

        static void LoadSettings() {

            InitSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Environment.CurrentDirectory + @"/init.json"));

            IXI.Classes.Settings.SetNodeAddress(InitSettings.NodeAddress);

            IXI.Classes.Settings.SetPrivateKey(InitSettings.PublicKey);

        }

        static void Run() {

            //let the process only run if not another process is running
            if (Utils.CheckFlag()) {
                if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                    Environment.Exit(0);
            }

            Utils.WriteFlag();

            //load settings
            LoadSettings();

            //Download all Blocks since the programm got closed
            IXI.DataBase Db = new IXI.DataBase(InitSettings.Chain.CoinName);
            LatestBlock = Db.GetLatestBlock();

            if (LatestBlock == null)
                DownloadThread(InitSettings.Chain.Hash, InitSettings.Chain.Address);
            else
                DownloadThread(LatestBlock.Hash, LatestBlock.SendTo);



            //Start All needed Threads
            //Start POW
            StartPOWThreads();

            //Start get Trans
            //TODO

            //Start Get newest DATA
            GetLatestBlockThread();

            //our exit
            while (true) {

                int milliseconds = 500;
                Thread.Sleep(milliseconds);

                if (!Utils.CheckFlag()) {

                    POWSource.Cancel();

                    Environment.Exit(0);
                }
            }
        }

        static Block LatestBlock;

        static void DownloadThread(string hash, string addr) {
            LatestBlock = IXI.Core.DownloadChain(addr, hash, 5, true, (Block b) => Console.WriteLine("Downloaded Block Nr:" + b.Height));
            Console.WriteLine("Blockchain is now synced\n");
        }

        static void GetLatestBlockThread() {

            Thread t = new Thread(() => {

                while (true) {

                    int milliseconds = 5000;
                    Thread.Sleep(milliseconds);

                    Block downloadedBlock = IXI.Core.DownloadChain(LatestBlock.SendTo, LatestBlock.Hash, 5, true, null);


                    //we found a new block!
                    if (downloadedBlock.Height > LatestBlock.Height && downloadedBlock != null) {

                        Console.WriteLine("We just found a new block! Old Height: {0} ... New Height {1}",
                            LatestBlock.Height, downloadedBlock.Height);

                        LatestBlock = downloadedBlock;

                        //cancel POW
                        POWSource.Cancel();

                        //restart POW
                        StartPOWThreads();

                    } else {
                        Console.WriteLine("... no new Block found ...");
                    }

                }

            });

            t.Start();

        }

        private static CancellationTokenSource POWSource;

        public static void StartPOWThreads() {

            POWSource = new CancellationTokenSource();

            Thread t = new Thread(() => {

                Block newBlock = new Block(LatestBlock.Height + 1, LatestBlock.NextAddress, LatestBlock.CoinName);

                //TODO fill Block with Transactions

                newBlock.Final();

                Console.WriteLine("Calculating Proof of Work for Block: " + newBlock.Height);

                newBlock.GenerateProofOfWork(5, POWSource.Token);

                if (newBlock.Nonce == -1) {
                    Console.WriteLine("Cancelling POW for Block " + newBlock.Height);
                    return;
                }

                IXI.Core.UploadBlock(newBlock);

                Console.WriteLine("Uploaded Block Nr: " + newBlock.Height);

                POWSource.Cancel();

            });

            t.Start();
        }

    }
}
