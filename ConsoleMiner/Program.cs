using System;
using IXI = TangleChainIXI;
using TangleChainIXI.Classes;
using System.Linq;
using Microsoft.Win32;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using IXIComponents.Simple;
using TangleChainIXI;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using IXIUtils = TangleChainIXI.Classes.Helper.Utils;

namespace ConsoleMiner
{
    public class Program
    {

        public static LedgerManager LedgerManager { get; set; }
        public static ThreadManager ThreadManager { get; set; }

        public static void Main(string[] args)
        {
            //we get all settings
            var settings = Initialize();

            Utils.WriteFlag();

            var _ixiSettings = new IXISettings().Default(true);
            var _ixiCore = (null as IXICore).SimpleSetup(settings.CoinName, _ixiSettings);

            LedgerManager = new LedgerManager(settings, _ixiCore, _ixiSettings);

            //create genesis block
            if (args[0].Equals("genesis"))
            {

                Console.WriteLine("Creating Genesis Block");
                var genBlock = LedgerManager.CreateGenesis(args);
                LedgerManager.Settings.GenesisHash = genBlock.Hash;
                LedgerManager.Settings.GenesisAddress = genBlock.SendTo;

                //save settings now
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "appsettings.json"), JsonConvert.SerializeObject(LedgerManager.Settings, Formatting.Indented));

            }

            //first sync chain
            var block = LedgerManager.SyncChain();
            ;
            //then run miner
            ThreadManager = new ThreadManager(block, _ixiCore, _ixiSettings);

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
        }

        private static Settings Initialize()
        {
            var settings = TryLoadSettings();


            if (settings != null)
                return settings;

            return new Settings()
            {
                NodeList = new List<string>() { "https://trinity.iota-tangle.io:14265" },
                PrivateKey = "secure",
                PublicKey = "secure".GetPublicKey(),
                DatabasePath = Environment.CurrentDirectory,
                CoreNumber = 2,
                CoinName = IXIUtils.GenerateRandomString(10)
            };

        }

        private static Settings TryLoadSettings()
        {

            try
            {
                return new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .Get<Settings>();
            }
            catch (Exception)
            {
                return null;
            }
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
