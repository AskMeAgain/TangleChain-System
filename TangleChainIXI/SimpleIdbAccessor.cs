using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI
{
    public class SimpleIdbAccessor : IDataAccessor
    {
        public string CoinName { get; set; }


        public Block GetBlock(string address, string hash) {
            throw new NotImplementedException();
        }

        public Transaction GetTransaction() {
            throw new NotImplementedException();
        }

        public Smartcontract GetSmartcontract() {
            throw new NotImplementedException();
        }

        public SimpleIdbAccessor(string coinName) {
            CoinName = coinName;
        }

        
    }
}
