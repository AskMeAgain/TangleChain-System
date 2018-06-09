using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TangleChainIXI.Classes;
using TangleChainIXI;
using System.Data.SQLite;

namespace DebugApp {
    public partial class Form1 : Form {

        public static long BlockHeight;

        public Form1() {
            InitializeComponent();

            BlockHeight = 0;

            Settings.Default(true);

        }

        private void ButtonLoadChain_Click(object sender, EventArgs e) {

            DataBase Db = new DataBase(TextBoxCoinName.Text);

            Block block = Db.GetLatestBlock();

            string address = block.SendTo;
            string hash = block.Hash;
            int difficulty = 5;

            Block latestBlock = Core.DownloadChain(address, hash, difficulty, true);

            LabelHeight.Text = latestBlock.Height + "";

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

        private void ButtonMineNextBlock_Click(object sender, EventArgs e) {

            LoadLatestDBImage();

            DataBase Db = new DataBase(TextBoxCoinName.Text);
            int difficulty = 5;

            Block latestBlock = Db.GetBlock(BlockHeight);

            //create block
            Block block = new Block(BlockHeight+1, latestBlock.NextAddress, latestBlock.CoinName);

            //we dont fill block with transactions yet

            block.Final();
            block.GenerateProofOfWork(difficulty);

            Core.UploadBlock(block);

            Db.AddBlock(block, true);
            BlockHeight = block.Height;

            LabelHeight.Text = BlockHeight + "";

        }

        private void ButtonLoadFromDB_Click(object sender, EventArgs e) {
            LoadLatestDBImage();
        }

        private void LoadLatestDBImage() {
            DataBase Db = new DataBase(TextBoxCoinName.Text);

            SQLiteDataReader reader = Db.QuerySQL($"SELECT MAX(Height) FROM Block");

            reader.Read();

            BlockHeight = (long) reader[0];
            LabelHeight.Text = BlockHeight + "";
        }
    }
}
