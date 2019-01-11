using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Interfaces
{
    public interface ITangleAccessor
    {
        Transaction GetTransaction(string hash, string address);
        Smartcontract GetSmartcontract(string hash, string address);
        Block GetBlock(string hash, string address);
    }
}
