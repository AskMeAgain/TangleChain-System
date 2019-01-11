using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Interfaces
{
    public interface IDataAccessor
    {
        //string CoinName { get; set; }

        Block GetBlock(string address, string hash);
        Block GetBlock(long height);
        Transaction GetTransaction();
        Smartcontract GetSmartcontract(string receivingAddr);
        List<Block> GetBlocks(string address);

        void AddBlock(Block block);
        void AddTransaction(Transaction trans);
        void AddSmartcontract(Smartcontract smart);

        void SetChainSettings(ChainSettings settings);
        ChainSettings GetChainSettings();

        List<Transaction> GetTransactionsFromBlock(Block block);

    }
}
