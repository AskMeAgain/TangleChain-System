﻿using System;
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

        public static int BlockHeight;
        public static bool isRunning;

        public MainWindow() {
            InitializeComponent();

            SendButton.Click += OnClick_Send;
            LoadChain.Click += OnClick_LoadChain;

            IXISettings.Default(true);

            TangleChainIXI.Classes.Block latest = DBManager.GetLatestBlock(CoinName.Text);
            Height.Content = "Height " + latest.Height;

            isRunning = false;
        }

        private void OnClick_Send(object sender, RoutedEventArgs e) {

            //we need to set settings
            IXISettings.Default(false);

            string pubKey = Cryptography.GetPublicKey(PrivKey.Text);
            string receive = Receiver.Text;
            string poolAddress = Utils.GetTransactionPoolAddress(BlockHeight, CoinName.Text);

            int fee = int.Parse(Fee.Text);
            int amount = int.Parse(Amount.Text);


            Transaction trans = new Transaction(pubKey, 1, poolAddress);

            trans.AddFee(fee);
            trans.AddOutput(amount, receive);

            trans.Final();

            MessageBoxResult result = MessageBox.Show("Do you want to Send the Coins?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) {
                Core.UploadTransaction(trans);
            }

        }

        private void OnClick_LoadChain(object sender, RoutedEventArgs e) {

            if (isRunning)
                return;

            isRunning = true;

            Core.DownloadChain(BlockAddress.Text, BlockHash.Text, true,
                b => { Height.Content = "Height: " + b.Height; }, CoinName.Text);
            isRunning = false;
        }

    }

}
