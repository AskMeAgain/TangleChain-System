using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI;
using TangleChainIXI.Classes;
using FluentAssertions;

namespace TangleChainIXITest.Scenarios
{
    [TestFixture]
    public class Scenario02
    {
        public string coinName = "smart_test" + Utils.GenerateRandomInt(5);

        public Smartcontract CreateSmartcontract(string name, string sendto)
        {

            Smartcontract smart = new Smartcontract(name, sendto);
            smart.ReceivingAddress = Utils.GenerateRandomString(81);

            smart.Code.AddExpression(new Expression(05, "PayIn"));
            smart.Code.AddExpression(new Expression(06, "T_4", "R_0"));
            smart.Code.AddExpression(new Expression(09, "R_0", "__1"));
            smart.Code.AddExpression(new Expression(05, "Exit"));

            return smart;

        }

        [Test]
        public void TestSmartcontract()
        {

            Smartcontract smart = CreateSmartcontract("name", Utils.GenerateRandomString(81));

            Transaction trans = new Transaction("ME", -1, Utils.GenerateRandomString(81));
            trans.AddFee(0);
            trans.Data.Add("PayIn");
            trans.AddOutput(100, "you");
            trans.Final();

            Computer comp = new Computer(smart);

            var result = comp.Run(trans);

            result.OutputValue[0].Should().Be(1);

        }


        [Test]
        public void Scenario()
        {

            //set genesis
            Difficulty startDifficulty = new Difficulty(7);

            //create genesis transaction
            ChainSettings cSett = new ChainSettings(1000, 0, 0, 2, 30, 3, 3);
            DBManager.SetChainSettings(coinName, cSett);

            Transaction genTrans = new Transaction("ME", -1, Utils.GetTransactionPoolAddress(0, coinName));
            genTrans.SetGenesisInformation(cSett);
            genTrans.Final();
            Core.UploadTransaction(genTrans);

            //create genesis block
            Block genBlock = new Block(0, Utils.GenerateRandomString(81), coinName);
            genBlock.AddTransactions(genTrans);
            genBlock.Final();
            genBlock.GenerateProofOfWork(startDifficulty);
            Core.UploadBlock(genBlock);

            //upload genesis

            //upload genesis block!

            //upload simple transaction on 1. block
            //add smartcontract

            //create 1. block

            //check stuff!

        }

    }
}
