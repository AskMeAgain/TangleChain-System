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
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Interfaces;
using TangleChainIXI.NewClasses;

namespace TangleChainIXI
{

    public class IXICore
    {

        private readonly ILogicManager _logicManager;

        public IXICore(ILogicManager logicManager)
        {
            _logicManager = logicManager;
        }

        public Block GetLatestBlock()
        {
            return _logicManager.GetLatestBlock();
        }

        public Block DownloadChain(string address, string hash, Action<Block> Hook = null)
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

        public long GetBalance(string addr)
        {
            return _logicManager.GetBalance(addr);
        }

        public int GetDifficulty(long? height)
        {
            return _logicManager.GetDifficulty(height);
        }

        public ChainSettings GetChainSettings()
        {
            return _logicManager.GetChainSettings();
        }

        public static IXICore SimpleSetup(string coinName)
        {

            var builder = new ContainerBuilder();

            builder.RegisterInstance(new CoinName(coinName));
            builder.RegisterType<SimpleLogicManager>().As<ILogicManager>().InstancePerLifetimeScope();
            builder.RegisterType<SimpleTangleAccessor>().As<ITangleAccessor>().InstancePerLifetimeScope();
            builder.RegisterType<SimpleDataAccessor>().As<IDataAccessor>().InstancePerLifetimeScope();
            builder.RegisterType<SimpleConsensus>().As<IConsensus>().InstancePerLifetimeScope();

            builder.RegisterType<IXICore>().AsSelf().InstancePerLifetimeScope();

            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                return scope.Resolve<IXICore>();
            }

            throw new ArgumentException("oop");
        }
    }
}
