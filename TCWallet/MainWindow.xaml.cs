using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TangleChainIXI;
using TangleChainIXI.Classes;
using System.Threading;

namespace TCWallet {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public static long BlockHeight;
        public static bool isRunning;

        public MainWindow() {
            InitializeComponent();

            SendButton.Click += OnClick_Send;
            LoadChain.Click += OnClick_LoadChain;
            Refresh.Click += OnClick_Refresh;

            IXISettings.Default(true);

            isRunning = false;

            AddToLog("Started Application");
        }

        private void OnClick_Refresh(object sender, RoutedEventArgs e) {
            UpdateVariables();
        }

        private void OnClick_Send(object sender, RoutedEventArgs e) {

            //we need to set settings
            IXISettings.Default(true);

            string pubKey = Cryptography.GetPublicKey(PrivKey.Text);
            string receive = Receiver.Text;

            string poolAddress = Utils.GetTransactionPoolAddress(BlockHeight, CoinName.Text);
            string poolAddress2 = Utils.GetTransactionPoolAddress(
                BlockHeight + DBManager.GetChainSettings(CoinName.Text).TransactionPoolInterval, CoinName.Text);

            int fee = int.Parse(Fee.Text);
            int amount = int.Parse(Amount.Text);


            Transaction trans = new Transaction(pubKey, 1, poolAddress);

            //sending to the next pool incase we didnt got selected for the first one!
            Transaction transSecond = new Transaction(pubKey, 1, poolAddress2);


            trans.AddFee(fee);
            trans.AddOutput(amount, receive);
            trans.Final();

            transSecond.AddFee(fee);
            transSecond.AddOutput(amount, receive);
            transSecond.Final();

            MessageBoxResult result = MessageBox.Show("Do you want to Send the Coins?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) {
                AddToLog("Started uploading Transaction");
                Core.Upload(trans);
                Core.Upload(transSecond);
                AddToLog("Transaction send to " + trans.TransactionPoolAddress);
                AddToLog("Transaction send to " + transSecond.TransactionPoolAddress);

            }

        }

        private void OnClick_LoadChain(object sender, RoutedEventArgs e) {

            if (isRunning)
                return;

            AddToLog("Started Downloading Chain!");
            isRunning = true;

            string addr = BlockAddress.Text;
            string hash = BlockHash.Text;
            string name = CoinName.Text;

            Thread t = new Thread(() => {

                Action<TangleChainIXI.Classes.Block> ActionTest = (b) => { AddToLog("Currently block: " + b.Height); };

                Core.DownloadChain(addr, hash, true, ActionTest, name);
                UpdateVariables();

                isRunning = false;

                AddToLog("Stopped Downloading Chain!");

            });
            t.Start();

            UpdateVariables();
        }

        public void AddToLog(string s) {
            Dispatcher.Invoke(() => {
                Log.AppendText(s + Environment.NewLine);
                Log.ScrollToEnd();
            });
        }

        public void UpdateVariables() {

            Dispatcher.Invoke(() => {

                if (!DataBase.Exists(CoinName.Text))
                    return;

                TangleChainIXI.Classes.Block latest = DBManager.GetLatestBlock(CoinName.Text);

                if (latest != null) {
                    Height.Content = "Height " + latest.Height;
                    BlockHeight = latest.Height;
                }


                long coins = DBManager.GetBalance(CoinName.Text, Cryptography.GetPublicKey(PrivKey.Text));

                Coins.Content = coins + " Coins";

            });

        }
    }

}
