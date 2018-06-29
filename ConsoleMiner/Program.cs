using System;
using TangleChainIXI;
using TangleChainIXI.Classes;
using System.Linq;

namespace ConsoleMiner {
    class Program {

        static void Main(string[] args) {
            Console.WriteLine("TangleChain Console Miner started.");

            if (!VerifyArguments(args))
                return;

            if (args[0].Equals("genesis")) {
                Genesis(args);
            }

        }

        private static void Genesis(string[] args) {
            



        }

        private static bool VerifyArguments(string[] args) {

            foreach (string argument in args) {
                if (!argument.Contains('='))
                    return false;
            }

            return true;

        }
    }
}
