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
        }

        private static bool GenesisProcess(string[] args)
        {

            if (args.Contains("genesis"))
            {
                if (!LedgerManager.InitGenesisProcess())
                {
                    Utils.Print("You entered wrong Information. Press any key to exit program", true);
                    return true;
                }
            }

            return false;
        }

        private static Settings Initialize()
        {
            throw new NotImplementedException();
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
