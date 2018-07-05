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

            InitSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Environment.CurrentDirectory+@"/init.json"));

            IXI.Classes.Settings.SetNodeAddress(InitSettings.NodeAddress);

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
            Block b = Db.GetLatestBlock();

            if (b == null)
                DownloadThread(InitSettings.Chain.Hash, InitSettings.Chain.Address);
            //else
                //DownloadThread(b.Hash, b.SendTo);


            //Start All needed Threads
            //Start POW
            //StartPOWThreads();

            //Start get Trans
            //TODO

            //Start Get newest DATA
            //GetLatestBlockThread();

            //our exit
            //while (true) {

            //    int milliseconds = 500;
            //    Thread.Sleep(milliseconds);

            //    if (!Utils.CheckFlag()) {
            //        foreach (Thread t in threadList)
            //            t.Abort();

            //        StopPOWThreads();

            //        Environment.Exit(0);
            //    }
            //}
        }

        public static List<Thread> threadList = new List<Thread>();

        static private string Hash;
        static Block LatestBlock;

        static void DownloadThread(string hash, string addr) {

            Thread t = new Thread(() => {
                Block latest = IXI.Core.DownloadChain(addr, hash, 5, true, (Block b) => Console.WriteLine("Downloaded Block Nr:" + b.Height));
                Hash = latest.Hash;
                Console.WriteLine("Stopped downloading first Block");
            });

            t.Start();

            threadList.Add(t);

        }

        static void GetLatestBlockThread() {

            Thread t = new Thread(() => {

                while (true) {

                    int milliseconds = 3000;
                    Thread.Sleep(milliseconds);

                    Block downloadedBlock = IXI.Core.DownloadChain(LatestBlock.SendTo, LatestBlock.Hash, 5, false, (Block b) => Console.WriteLine("Downloaded Block Nr:" + b.Height));

                    //we found a new block!
                    if (downloadedBlock != LatestBlock) {

                        LatestBlock = downloadedBlock;

                        //cancel POW
                        StopPOWThreads();

                        //restart POW
                        StartPOWThreads();

                        //store block now
                        IXI.DataBase Db = new IXI.DataBase(LatestBlock.CoinName);
                        Db.AddBlock(LatestBlock, true);
                    }

                }

            });

            t.Start();

            threadList.Add(t);

        }

        private static void StopPOWThreads() {
            foreach (Thread thread in POWThreads)
                thread.Abort();
            POWThreads.Clear();
        }

        private static List<Thread> POWThreads = new List<Thread>();

        public static void StartPOWThreads() {

            Thread t = new Thread(() => {

                Block newBlock = new Block(LatestBlock.Height + 1, LatestBlock.NextAddress, LatestBlock.CoinName);

                //fill Block with Transactions

                newBlock.Final();
                newBlock.GenerateProofOfWork(5);

                IXI.Core.UploadBlock(newBlock);

            });

            t.Start();

            POWThreads.Add(t);
        }

    }
}
