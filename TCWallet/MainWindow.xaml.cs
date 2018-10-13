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

            TangleChainIXI.Classes.IXISettings.Default(true);
        }

        private void OnLoadChain(object sender, RoutedEventArgs e)
        {
            Core.DownloadChain(CoinName.Text, Address.Text, Hash.Text, true, (b) => Console.WriteLine(b.Height));
            MessageBox.Show("worked!");
        }
    }
}
