using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Repository;
using Tangle.Net.Utils;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI
{

    public class Core
    {

        private readonly ILogicManager _logicManager;

        public Core(ILogicManager logicManager)
        {
            _logicManager = logicManager;
        }

        public Block GetLatestBlock() {
            return _logicManager.GetLatestBlock();
        }

        public Block DownloadChain(string address, string hash, Action<Block> Hook)
        {

            Block block = _logicManager.GetSpecificBlock(address, hash);

            if (!block.Verify(_logicManager.GetDifficulty(block.Height)))
                throw new ArgumentException("Provided Block is NOT VALID!");

            Hook?.Invoke(block);

            //we store first block! stupid hack
            _logicManager.AddBlock(new List<Block>() { block });

            while (true)
            {

                //first we need to get the correct way
                var newBlocks = _logicManager.FindCorrectWay(block.NextAddress, block.Height + 1);

                //we repeat the whole until we dont have a newer way
                if (newBlocks.Count == 0)
                    break;

                //we then download this whole chain
                _logicManager.AddBlock(newBlocks);

                //we just jump to the latest block
                block = newBlocks.Last();

                Hook?.Invoke(block);

            }

            return block;

        }

    }
}
