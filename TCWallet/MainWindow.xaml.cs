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
using TangleChainIXI.Smartcontracts;


namespace TCWallet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CoinName.Text = "Testv4";
            Address.Text = "JBHAKQLRTIHIZDCROTVVCJLIBEMJQVGOLNTHEJNYRIMUT9QR9PESSGRSZXBLOCQPZOXWKHNHUEQET9MTK";
            Hash.Text = "SX9AWHKNCEYIXL9UNWKJLTNSAKPSRCQLNX9GPCGNOASROEJFISFAR9NSTLVBOUYBANTKPBJTADMWDTRJT";

            IXISettings.Default(true);
        }

        private void OnLoadChain(object sender, RoutedEventArgs e)
        {
            Core.DownloadChain(CoinName.Text, Address.Text, Hash.Text, true, (b) => Console.WriteLine(b.Height));
            MessageBox.Show("worked!");
        }

        private void OnUploadSmartcontract(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(GetPoolAddress());
            Smartcontract smart = CreateSmartcontract("test", GetPoolAddress());
            smart.Final().Upload();

            MessageBox.Show($"contract uploaded to {GetPoolAddress()}");
        }

        public Smartcontract CreateSmartcontract(string name, string sendto)
        {

            Smartcontract smart = new Smartcontract(name, sendto);
            smart.AddFee(1);
            smart.ReceivingAddress = Utils.GenerateRandomString(81);

            smart.AddVariable("counter")

                .AddExpression(05, "PayIn")
                .AddExpression(00, "D_2", "R_0")

                //we add one to counter
                .AddExpression(00, "S_counter", "R_1")
                .AddExpression(01, "R_1", "__1", "R_2")
                .AddExpression(06, "R_2", "S_counter")

                //set out transaction
                .AddExpression(09, "R_0", "__1")
                .AddExpression(05, "Exit");

            return smart;

        }

        private void SendTransaction(object sender, RoutedEventArgs e) {

            Transaction trans = new Transaction(Cryptography.GetPublicKey(FromTransaction.Text), 2, GetPoolAddress());
            trans.AddFee(0)
                .AddOutput(100, ToTransaction.Text)
                .AddData(TransactionData.Text)
                .Final()
                .Upload();

            MessageBox.Show($"transaction uploaded to {GetPoolAddress()}");

        }

        public string GetPoolAddress() {
            return Utils.GetTransactionPoolAddress(DBManager.GetLatestBlock(CoinName.Text).Height, CoinName.Text);
        }
    }
}
