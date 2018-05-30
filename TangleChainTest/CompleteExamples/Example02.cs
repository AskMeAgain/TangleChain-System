using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.CompleteExamples
{
    [TestFixture]
    public class Example02 {

        //[Test]
        public void StartExample02() {
            //is a 0 1 22 33 4 5|5  6|6 7 tree

            Settings.Default(true);
            Settings.SetNumOfTransPerBlock(3);

            int difficulty = 5;
            string coinName = Utils.GenerateRandomString(10);

            Console.WriteLine("Coinname: " + coinName);

            //before we start we need to add 20 transactions to transasction pool
            Utils.FillTransactionPool(20, coinName, 4);

            //we first create genesis block
            Block genesis = Core.CreateAndUploadGenesisBlock(coinName, "ME", 100000);

            //we print the genesis address so we can use this somewhere else
            Console.WriteLine("Genesis Address: " + genesis.SendTo);
            Console.WriteLine("Genesis Hash: " + genesis.Hash);

            //we then attach a single block to it
            Block nextBlock = Core.MineBlock(coinName, genesis.Height + 1, genesis.NextAddress, difficulty, true);

            //we then split the network with two blocks
            Block nextBlock2 = Core.MineBlock(coinName, nextBlock.Height + 1, nextBlock.NextAddress, difficulty, true);
            Core.MineBlock(coinName, nextBlock.Height + 1, nextBlock.NextAddress, difficulty, false);

            //we then split the chain again
            Block nextBlock3 = Core.MineBlock(coinName, nextBlock2.Height + 1, nextBlock2.NextAddress, difficulty, true);
            Core.MineBlock(coinName, nextBlock2.Height + 1, nextBlock2.NextAddress, difficulty, false);

            //we mine a last block ontop
            Block last = Core.MineBlock(coinName, nextBlock3.Height + 1, nextBlock3.NextAddress, difficulty, true);

            //we now split the chain in two other chains
            Block lastA = Core.MineBlock(coinName, last.Height + 1, last.NextAddress, difficulty, true);
            Block lastB = Core.MineBlock(coinName, last.Height + 1, last.NextAddress, difficulty, false);

            Block lastAA = Core.MineBlock(coinName, lastA.Height + 1, lastA.NextAddress, difficulty, true);
            Block lastBB = Core.MineBlock(coinName, lastB.Height + 1, lastB.NextAddress, difficulty, false);

            Block lastAAA = Core.MineBlock(coinName, lastAA.Height + 1, lastAA.NextAddress, difficulty, true);

            Console.WriteLine("Last Hash: " + lastAAA.Hash);
            Console.WriteLine("Last Address: " + lastAAA.SendTo);
        }
    }
}
