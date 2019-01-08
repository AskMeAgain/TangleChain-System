using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly IBlockManager _blockManager;

        public Core(IBlockManager blockManager)
        {
            _blockManager = blockManager;
        }

        public Block GetBlock(int height) {
            return _blockManager.GetBlock(height);
        }

        public Block DownloadChain(string address, string hash, Action<Block> Hook)
        {

            Block block = _blockManager.GetSpecificBlock(address, hash);

            if (!block.Verify(_blockManager.GetDifficulty(block.Height)))
                throw new ArgumentException("Provided Block is NOT VALID!");

            Hook?.Invoke(block);

            //we store first block! stupid hack
            _blockManager.AddBlock(new List<Block>() { block });

            while (true)
            {

                //first we need to get the correct way
                Way way = _blockManager.FindCorrectWay(block.NextAddress, block.Height + 1);

                //we repeat the whole until we dont have a newer way
                if (way == null)
                    break;

                //we then download this whole chain
                _blockManager.AddBlock(way.ToBlockList());

                //we just jump to the latest block
                block = way.CurrentBlock;

                Hook?.Invoke(block);

            }

            return block;

        }
    }
}
