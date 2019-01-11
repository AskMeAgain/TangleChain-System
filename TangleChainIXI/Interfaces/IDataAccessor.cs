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
        void AddTransaction(List<Transaction> trans, long height);
        void AddSmartcontract(List<Smartcontract> smart, long height);

        void SetChainSettings(ChainSettings settings);
        ChainSettings GetChainSettings();

        List<Transaction> GetTransactionsFromBlock(Block block);

    }
}
