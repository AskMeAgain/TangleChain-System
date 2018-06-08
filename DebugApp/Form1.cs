using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TangleChain.Classes;
using TangleChain;

namespace DebugApp {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

            Settings.Default(true);

        }

        private void ButtonLoadChain_Click(object sender, EventArgs e) {

            string address = TextBoxAddress.Text;
            string hash = TextBoxHash.Text;
            int difficulty = 5;

            Block latestBlock = Core.DownloadChain(address, hash, difficulty, true);

        }

        private void ButtonCreateGenesis_Click(object sender, EventArgs e) {

            string Receiver = TextBoxReceiver.Text;
            string CoinName = TextBoxCoinName.Text;
            int Amount = int.Parse(TextBoxAmount.Text);
            int difficulty = 5;

            Block genesis = new Block(0, Utils.GenerateRandomString(81), CoinName);

            Transaction genesisTrans = new Transaction("GENESIS", -1, Utils.GetTransactionPoolAddress(0,CoinName));
            genesisTrans.AddFee(0);
            genesisTrans.AddOutput(Amount, Receiver);
            genesisTrans.Final();

            genesis.AddTransactions(genesisTrans);
            genesis.Final();
            genesis.GenerateProofOfWork(difficulty);

            Core.UploadTransaction(genesisTrans);
            Core.UploadBlock(genesis);

            TextBoxAddress.Text = genesis.SendTo;
            TextBoxHash.Text = genesis.Hash;

        }
    }
}
