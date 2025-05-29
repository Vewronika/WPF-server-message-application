using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public class ServerListener
        {
            private TcpListener tcpListener;
            private bool running = false;
            private readonly List<Client> clients = new List<Client>();
        }


        public class Client
        {

        }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void BroadcastButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void KickButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}