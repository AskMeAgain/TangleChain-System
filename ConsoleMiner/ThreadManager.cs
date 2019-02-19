using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TangleChainIXI;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
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

        private readonly IXICore _ixiCore;
        private readonly IXISettings _ixiSettings;

        public ThreadManager(Block latestBlock, IXICore ixicore, IXISettings ixisettings)
        {

            LatestBlock = latestBlock;
            CoinName = latestBlock.CoinName;
            _ixiCore = ixicore;
            _ixiSettings = ixisettings;

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

                    Block downloadedBlock = _ixiCore.DownloadChain(LatestBlock.SendTo, LatestBlock.Hash, null);

                    //we found a new block
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

            var core = _ixiCore;

            Thread t = new Thread(() =>
            {

                Utils.Print("Starting Block Construction Thread", false);
                var maybeSettings = core.GetChainSettings();

                if (!maybeSettings.HasValue)
                {
                    Console.WriteLine("No chainsettings found! cant operate with this");
                    Cancel();
                }

                ChainSettings cSett = maybeSettings.Value;

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

                    string poolAddr = TangleChainIXI.Classes.Helper.Utils.GetTransactionPoolAddress(LatestBlock.Height + 1, LatestBlock.CoinName);
                    var poolHeight = (int)(LatestBlock.Height + 1) / cSett.TransactionPoolInterval;

                    var smartList = core.GetAllFromAddress<Smartcontract>(poolAddr);
                    var transList = core.GetAllFromAddress<Transaction>(poolAddr);

                    //TODO CHECK IF THE TRANS/Smartcontracts ARE LEGIT
                    UpdateLocalDatabase(smartList, poolHeight, transList);

                    Utils.Print("...", false);

                    //means we didnt changed anything && we dont need to construct a new block
                    if (numOfTransactions == transList.Count && numOfContracts == smartList.Count && !ConstructNewBlockFlag)
                        continue;

                    if ((ConstructNewBlockFlag && NewConstructedBlock.Height <= LatestBlock.Height) || NewConstructedBlock == null)
                    {
                        //if newconstr. is null then we definitly need to construct one

                        List<Transaction> selectedTrans = GetFromPool(poolHeight, cSett, out List<Smartcontract> selectedSmart);

                        //select now highest fees
                        List<ISignable> list = selectedSmart.Cast<ISignable>().ToList();
                        list.AddRange(selectedTrans.Cast<ISignable>());
                        var sortedList = list.OrderBy(x => x.GetFee()).Take(cSett.TransactionsPerBlock).ToList();

                        //the new block which will include all new transactions
                        Block newestBlock = (Block)new Block(LatestBlock.Height + 1, LatestBlock.NextAddress,
                                LatestBlock.CoinName)
                            .Add(sortedList)
                            .Final(_ixiSettings)
                            .GenerateProofOfWork(_ixiCore, token);


                        NewConstructedBlock = newestBlock;
                        Utils.Print("... Block Nr {0} is constructed with {1} Transactions and {2} Smartcontracts", false, NewConstructedBlock.Height.ToString(), NewConstructedBlock.TransactionHashes.Count + "", NewConstructedBlock.SmartcontractHashes.Count + "");

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

        private List<Transaction> GetFromPool(int poolHeight, ChainSettings cSett, out List<Smartcontract> selectedSmart)
        {
            selectedSmart = SmartPool[poolHeight].OrderBy(x => x.GetFee()).Take(cSett.TransactionsPerBlock).ToList();
            return TransPool[poolHeight].OrderBy(x => x.ComputeMinerReward()).Take(cSett.TransactionsPerBlock).ToList();
        }

        private Dictionary<int, List<Smartcontract>> SmartPool = new Dictionary<int, List<Smartcontract>>();
        private Dictionary<int, List<Transaction>> TransPool = new Dictionary<int, List<Transaction>>();

        private void UpdateLocalDatabase(List<Smartcontract> smartList, int poolHeight, List<Transaction> transList)
        {
            if (!TransPool.ContainsKey(poolHeight)) {
                TransPool.Add(poolHeight, new List<Transaction>());
            }

            if (!SmartPool.ContainsKey(poolHeight)) {
                SmartPool.Add(poolHeight, new List<Smartcontract>());
            }

            TransPool[poolHeight].RemoveAll(x => transList.Contains(x));
            transList.ForEach(x => TransPool[poolHeight].Add(x));

            SmartPool[poolHeight].RemoveAll(x => smartList.Contains(x));
            smartList.ForEach(x => SmartPool[poolHeight].Add(x));
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
