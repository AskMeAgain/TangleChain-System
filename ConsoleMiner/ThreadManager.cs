using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TangleChainIXI;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

namespace ConsoleMiner
{
    public class ThreadManager
    {

        private string CoinName;
        private bool ConstructNewBlockFlag;
        private bool stopPOW;

        private Block LatestBlock;
        private Block NewConstructedBlock;
        private CancellationTokenSource POWsource;
        private CancellationTokenSource constructBlockSource;
        private CancellationTokenSource latestBlocksource;

        public ThreadManager(Block latestBlock)
        {

            LatestBlock = latestBlock;
            CoinName = latestBlock.CoinName;

            //Start All needed Threads
            //Start POW
            POWsource = StartPOWThreads();

            //Start construction block
            constructBlockSource = ConstructBlockThread();

            //Start Get newest DATA thread
            latestBlocksource = GetLatestBlockThread();
        }

        public void Cancel()
        {
            constructBlockSource.Cancel();
            latestBlocksource.Cancel();
            POWsource.Cancel();
        }

        #region Threads

        private CancellationTokenSource GetLatestBlockThread()
        {

            CancellationTokenSource source = new CancellationTokenSource();

            Thread t = new Thread(() =>
            {

                Utils.Print("Starting Block Download Thread", false);

                CancellationToken token = source.Token;

                while (!token.IsCancellationRequested)
                {

                    int milliseconds = 5000;
                    Thread.Sleep(milliseconds);

                    if (token.IsCancellationRequested)
                        break;

                    Utils.Print("... Checking Downloads ... ", false);

                    Block downloadedBlock = Core.DownloadChain(CoinName, LatestBlock.SendTo, LatestBlock.Hash, true, null);

                    //we found a new block!
                    if (downloadedBlock.Height > LatestBlock.Height && downloadedBlock != null)
                    {

                        Utils.Print("... We just found a new block! Old Height: {0} ... New Height {1}", false, LatestBlock.Height.ToString(), downloadedBlock.Height.ToString());

                        LatestBlock = downloadedBlock;
                        ConstructNewBlockFlag = true;
                    }
                    else
                    {
                        Utils.Print("... Chain up to date", false);
                    }

                }

            })
            {
                IsBackground = true
            };
            t.Start();

            return source;

        }

        private CancellationTokenSource ConstructBlockThread()
        {

            CancellationTokenSource source = new CancellationTokenSource();

            Thread t = new Thread(() =>
            {

                Utils.Print("Starting Block Construction Thread", false);
                ChainSettings cSett = DBManager.GetChainSettings(LatestBlock.CoinName);
                CancellationToken token = source.Token;

                int numOfTransactions = -1;
                int numOfContracts = -1;

                while (!token.IsCancellationRequested)
                {

                    int milliseconds = 5000;
                    Thread.Sleep(milliseconds);

                    //stop thread
                    if (token.IsCancellationRequested)
                        break;

                    string poolAddr = TangleChainIXI.Utils.GetTransactionPoolAddress(LatestBlock.Height + 1, LatestBlock.CoinName);
                    var poolHeight = (int)(LatestBlock.Height + 1) / cSett.TransactionPoolInterval;

                    var smartList = Core.GetAllFromAddress<Smartcontract>(poolAddr);
                    var transList = Core.GetAllFromAddress<Transaction>(poolAddr);

                    DBManager.Add(LatestBlock.CoinName, smartList, null, poolHeight);
                    DBManager.Add(LatestBlock.CoinName, transList, null, poolHeight);

                    Utils.Print("...", false);

                    //means we didnt changed anything && we dont need to construct a new block
                    if (numOfTransactions == transList.Count && numOfContracts == smartList.Count && !ConstructNewBlockFlag)
                        continue;

                    if ((ConstructNewBlockFlag && NewConstructedBlock.Height <= LatestBlock.Height) || NewConstructedBlock == null)
                    {
                        //if newconstr. is null then we definitly need to construct one
                        var selectedTrans = DBManager.GetTransactionFromPool(LatestBlock.CoinName, poolHeight, cSett.TransactionsPerBlock);
                        var selectedSmart = DBManager.GetSmartcontractFromPool(LatestBlock.CoinName, poolHeight, cSett.TransactionsPerBlock);

                        //TODO SELECT HIGHEST FEES!

                        //the new block which will include all new transactions
                        Block newestBlock = new Block(LatestBlock.Height + 1, LatestBlock.NextAddress,
                                LatestBlock.CoinName)
                            .AddTransactions(selectedTrans)
                            .Final()
                            .GenerateProofOfWork(token);

                        NewConstructedBlock = newestBlock;
                        Utils.Print("... Block Nr {0} is constructed with {1} Transactions", false, NewConstructedBlock.Height.ToString(), NewConstructedBlock.TransactionHashes.Count + "");

                        //flags
                        ConstructNewBlockFlag = false;
                        stopPOW = false;
                        numOfTransactions = transList.Count;
                        numOfContracts = smartList.Count;
                    }
                }

            })
            {
                IsBackground = true
            };
            t.Start();

            return source;
        }

        private CancellationTokenSource StartPOWThreads()
        {

            CancellationTokenSource source = new CancellationTokenSource();

            for (int i = 0; i < 1; i++)
            {
                Thread t = new Thread(() =>
                {

                    Utils.Print("Starting POW Thread", false);

                    CancellationToken token = source.Token;

                    int nonce = 0;
                    Block checkBlock = NewConstructedBlock;

                    while (!token.IsCancellationRequested)
                    {

                        nonce++;

                        if (nonce % 50 == 0)
                        {
                            checkBlock = NewConstructedBlock;
                        }

                        if (checkBlock == null || stopPOW)
                        {
                            continue;
                        }

                        if (Cryptography.VerifyNonce(checkBlock.Hash, nonce, NewConstructedBlock.Difficulty))
                        {
                            Utils.Print("... ... New Block got found. Preparing for Upload", false);
                            checkBlock.Nonce = nonce;
                            checkBlock.Upload();
                            Utils.Print("... ... Upload of Block {0} Finished", false, checkBlock.Height.ToString());
                            stopPOW = true;
                        }

                    }

                })
                {
                    IsBackground = true
                };
                t.Start();
            }

            return source;
        }

        #endregion 
    }
}
