using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Interfaces
{
    public interface IDataAccessor
    {
        string CoinName { get; set; }

        Block GetBlock(string address, string hash);
        Transaction GetTransaction();
        Smartcontract GetSmartcontract();
        List<Block> GetBlocks(string address);

        void AddBlock(Block block);

        List<Transaction> GetTransactionsFromBlock(Block block);
    }
}
