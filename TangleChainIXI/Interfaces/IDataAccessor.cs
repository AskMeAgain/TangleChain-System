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
        Transaction GetTransaction(string hash, string height);
        Smartcontract GetSmartcontract(string receivingAddr);
        List<Block> GetBlocks(string address);

        void AddBlock(Block block);
        void AddTransaction(List<Transaction> trans, long height);
        void AddSmartcontract(List<Smartcontract> smart, long height);

        void SetChainSettings(ChainSettings settings);
        ChainSettings GetChainSettings();

    }
}
