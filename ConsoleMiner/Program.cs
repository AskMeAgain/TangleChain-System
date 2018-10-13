using System;
using IXI = TangleChainIXI;
using TangleChainIXI.Classes;
using System.Linq;
using Microsoft.Win32;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using TangleChainIXI;
using Microsoft.Extensions.Configuration;

namespace ConsoleMiner
{
    public class Program
    {

        public static LedgerManager LedgerManager { get; set; }
        public static ThreadManager ThreadManager { get; set; }

        public static void Main(string[] args)
        {
            //we get all settings and do some validation tests!
            var settings = Initialize();

            LedgerManager = new LedgerManager(settings);

            if (GenesisProcess(args)) return;

            //first sync chain
            var block = LedgerManager.SyncChain();

            //then run miner
            ThreadManager = new ThreadManager(block);

            //our exit
            while (true)
            {

                int milliseconds = 500;
                Thread.Sleep(milliseconds);

                if (!Utils.FlagIsSet())
                {

                    ThreadManager.Cancel();

                    Environment.Exit(0);
                }
            }

            //switch (argsList[0].ToLower())
            //{
            //    case "fill":
            //        FillPool(int.Parse(argsList[1]));
            //        break;

            //    case "genesis":
            //        if (!Startup())
            //            return;

            //        if (!InitGenesisProcess())
            //        {
            //            Utils.Print("You entered wrong Information. Press any key to exit program", true);
            //            return;
            //        }
            //        break;

            //    case "addkey":
            //        if (argsList.Count == 3)
            //        {

            //            Settings settings = Utils.ReadInitFile();

            //            if (settings == null)
            //            {
            //                Utils.Print("Init file is missing. Press any key to exit program", true);
            //                break;
            //            }

            //            if (argsList[1].Equals("-priv"))
            //            {
            //                settings.PrivateKey = argsList[2];
            //                settings.PublicKey = IXI.Cryptography.GetPublicKey(argsList[2]);
            //            }
            //            else if (argsList[1].Equals("-pub"))
            //            {

            //                //i dont know the length of an ethereum key so i use this hack 
            //                if (argsList[2].Length == IXI.Cryptography.GetPublicKey("test").Length)
            //                {
            //                    settings.PublicKey = argsList[2];
            //                }
            //                else
            //                {
            //                    Utils.Print("Your public key is to short/long. Press any key to exit program", true);
            //                }
            //            }

            //            Utils.WriteInitFile(settings);
            //            Utils.Print("Public key got added. Press any key to exit program", true);
            //        }
            //        break;

            //    case "sync":
            //        if (!Startup())
            //        {
            //            return;
            //        }
            //        manager.SyncChain();
            //        break;

            //    case "init":
            //        if (File.Exists(Environment.CurrentDirectory + @"/init.json"))
            //        {
            //            Utils.Print("Init file already exists. Press any key to exit program", true);
            //        }
            //        else
            //        {
            //            Utils.CreateInitFile();
            //            Utils.Print("Init File Created. Press any key to exit program", true);
            //        }
            //        break;

            //    case "help":
            //        Utils.Print("The following commands are possible: \n run \n exit \n init \n help \n", false);
            //        break;

            //    case "balance":

            //        Settings set = Utils.ReadInitFile();
            //        if (set == null)
            //        {
            //            Utils.Print("IXISettings file not set. Press any key to exit program", true);
            //            break;
            //        }

            //        if ((argsList.Count == 2))
            //        {
            //            Utils.Print("The Balance of Address {0} is {1}. Press any key to exit program.\n", set.PublicKey,
            //                IXI.DBManager.GetBalance(set.MinedChain.CoinName, set.PublicKey).ToString(), true);
            //        }

            //        if (argsList.Count == 3)
            //        {
            //            //balance pubkey
            //            Utils.Print("The Balance of Address {0} is {1}. Press any key to exit program.\n", argsList[2], IXI.DBManager.GetBalance(set.MinedChain.CoinName, argsList[2]).ToString(), true);
            //        }
            //        break;

            //    case "publickey":
            //        if (argsList.Count == 2)
            //        {
            //            Utils.Print("Your Public key is {0}. Press any key to exit program.\n",
            //                IXI.Cryptography.GetPublicKey(argsList[1]), true);
            //        }
            //        break;

            //}
        }

        private static bool GenesisProcess(string[] args) {

            if (args.Contains("genesis")) {
                if (!LedgerManager.InitGenesisProcess()) {
                    Utils.Print("You entered wrong Information. Press any key to exit program", true);
                    return true;
                }
            }

            return false;
        }

        private static Settings Initialize()
        {

            //Getting settings into program!

            var settings = LoadSettings();

            if (!settings.Validate())
            {
                Utils.Print("No settings are set. Press any key to exit program", true);
                Environment.Exit(0);
            }

            //let the process only run if not another process is running
            if (Utils.FlagIsSet())
            {
                if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location))
                        .Count() > 1)
                {
                    Utils.Print("Another instance is running! Press any key to exit program", true);
                    Environment.Exit(0);
                }
            }

            //check connection to node!
            var nodeAddress = Utils.IsConnectionEstablished(settings.NodeList);
            if (nodeAddress == null)
            {
                Utils.Print("Connection Failed to Node. Press any key to exit program", true);
                Environment.Exit(0);
            }

            //mark spot so running another instance is not possible
            Utils.WriteFlag();
            Console.Title = "ConsoleMiner";
            Utils.Print("ConsoleMiner started\n", false);

            //set ixisettings
            IXISettings.SetPrivateKey(settings.PrivateKey);
            IXISettings.SetDataBasePath(settings.DatabasePath);

            return settings;
        }

        private static Settings LoadSettings()
        {

            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            return config.Get<Settings>();
        }

        #region executable

        private static void Exit()
        {
            Utils.DeleteFlag();

            if (!Utils.FlagIsSet())
                Environment.Exit(0);
        }

        #endregion
    }
}
