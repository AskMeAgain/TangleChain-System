using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Tangle.Net.Repository;
using Tangle.Net.Repository.DataTransfer;

namespace TangleChain.Classes {

    public static class Settings {

        public static int TransactionPoolInterval { get; private set; }
        public static int NumberOfTransactionsPerBlock { get; private set; }
        public static string NodeAddress { get; private set; }

        public static void Default() {
            SetNodeAddress("https://beef.iotasalad.org:14265");
            SetNumOfTransPerBlock(5);
            SetTransPoolInterval(10);
        }

        public static NodeInfo SetNodeAddress(string addr) {

            //no need for try block since restiotarepo throws exception anyway

            var repository = new RestIotaRepository(new RestClient(addr));
            var info = repository.GetNodeInfo();

            NodeAddress = addr;

            return info;
        }

        public static int SetNumOfTransPerBlock(int num) {

            NumberOfTransactionsPerBlock = num;

            return NumberOfTransactionsPerBlock;

        }

        public static int SetTransPoolInterval(int num) {

            TransactionPoolInterval = num;

            return TransactionPoolInterval;

        }






    }
}
