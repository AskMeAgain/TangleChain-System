using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Interfaces
{
    public interface IDataAccessor
    {
        Maybe<T> Get<T>(object info = null, long? height = null) where T : IDownloadable;

        void AddBlock(Block block);
        void AddSignable<T>(List<T> list, long height) where T : ISignable;

        long GetBalance(string userAddr);

        Maybe<Block> GetLatestBlock();
        Maybe<ChainSettings> GetChainSettings();

        List<T> GetFromBlock<T>(Block block) where T : IDownloadable, ISignable;

    }
}
