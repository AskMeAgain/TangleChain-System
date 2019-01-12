using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Interfaces
{
    public interface IDataAccessor
    {
        Block GetBlock(long height);
        Transaction GetTransaction(string hash, long height);
        Smartcontract GetSmartcontract(string receivingAddr);

        void AddBlock(Block block);
        void AddTransaction(List<Transaction> trans, long height);
        void AddSmartcontract(List<Smartcontract> smart, long height);

        ChainSettings GetChainSettings();

        long GetBalance(string userAddr);

        Block GetLatestBlock();

        List<Transaction> GetTransactionFromBlock(Block block);
        List<Smartcontract> GetSmartcontractsFromBlock(Block block);
    }
}
