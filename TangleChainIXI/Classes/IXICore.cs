using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Repository;
using Tangle.Net.Utils;
using TangleChainIXI.Classes;
using TangleChainIXI.Classes.Helper;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI.Classes
{

    public class IXICore
    {

        private readonly IDataAccessor _dataAccessor;
        private readonly ITangleAccessor _tangleAccessor;
        private readonly IConsensus _consensus;
        private readonly IXISettings _settings;

        public IXICore(IXISettings settings, IConsensus consensus, IDataAccessor dataAccessor, ITangleAccessor tangleAccessor)
        {
            _dataAccessor = dataAccessor;
            _tangleAccessor = tangleAccessor;
            _consensus = consensus;
            _settings = settings;
        }

        public Block DownloadChain(string address, string hash, Action<Block> Hook = null)
        {

            var maybeBlock = _tangleAccessor.GetSpecificFromAddress<Block>(hash, address, _settings);
            if (!maybeBlock.HasValue)
            {
                throw new ArgumentException("Provided Block doesnt exist");
            }

            var block = maybeBlock.Value;

            if (!block.Verify(_consensus.GetDifficulty(block.Height)))
                throw new ArgumentException("Provided Block is NOT VALID!");

            Hook?.Invoke(block);

            //we store first block! stupid hack
            _dataAccessor.AddBlock(block);

            while (true)
            {

                //first we need to get the correct way
                var newBlocks = _consensus.FindNewBlocks(block.NextAddress, block.Height + 1, _consensus.GetDifficulty(block.Height + 1));

                //we repeat the whole until we dont have a newer way
                if (newBlocks.Count == 0)
                    break;

                //we then download this whole chain
                newBlocks.ForEach(x => _dataAccessor.AddBlock(x));

                //we just jump to the latest block
                block = newBlocks.Last();

                Hook?.Invoke(block);

            }

            return block;

        }

        public Maybe<Block> GetLatestBlock()
        {
            return _dataAccessor.GetLatestBlock();
        }

        public long GetBalance(string addr)
        {
            return _dataAccessor.GetBalance(addr);
        }

        public int GetDifficulty(long? height)
        {
            return _consensus.GetDifficulty(height);
        }

        public Maybe<ChainSettings> GetChainSettings()
        {
            return _dataAccessor.GetChainSettings();
        }

        public Maybe<Smartcontract> GetSmartcontract(string receiveAddr)
        {
            return _dataAccessor.Get<Smartcontract>(receiveAddr);
        }
    }
}
