using System;
using IXI = TangleChainIXI;
using TangleChainIXI.Classes;
using System.Linq;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Threading;

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

        static void Run() {

            int counter = 0;

            if (Utils.CheckFlag()) {
                Environment.Exit(0);
            }

            Utils.WriteFlag();

            while (true) {

                if (!Utils.CheckFlag())
                    Environment.Exit(0);

                int milliseconds = 500;
                Thread.Sleep(milliseconds);

                Console.WriteLine("  " + counter++);

            }
        }


    }
}
