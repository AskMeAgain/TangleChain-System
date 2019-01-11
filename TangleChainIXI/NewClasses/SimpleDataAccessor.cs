using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.NewClasses
{
    public class SimpleDataAccessor : IDataAccessor
    {
        public string _coinName { get; set; }

        public SimpleDataAccessor(string coinName)
        {
            _coinName = coinName;
        }

        public Block GetBlock(string address, string hash)
        {
            throw new NotImplementedException();
        }

        public Block GetBlock(long height)
        {
            throw new NotImplementedException();
        }

        public Transaction GetTransaction()
        {
            throw new NotImplementedException();
        }

        public Smartcontract GetSmartcontract(string receivingAddr)
        {
            throw new NotImplementedException();
        }

        public List<Block> GetBlocks(string address)
        {
            throw new NotImplementedException();
        }

        public void AddBlock(Block block)
        {
            throw new NotImplementedException();
        }

        public void AddTransaction(Transaction trans)
        {
            throw new NotImplementedException();
        }

        public void AddSmartcontract(Smartcontract smart)
        {
            throw new NotImplementedException();
        }

        public void SetChainSettings(ChainSettings settings)
        {
            throw new NotImplementedException();
        }

        public ChainSettings GetChainSettings()
        {
            throw new NotImplementedException();
        }

        public List<Transaction> GetTransactionsFromBlock(Block block)
        {
            throw new NotImplementedException();
        }
    }
}
