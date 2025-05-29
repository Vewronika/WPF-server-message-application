using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace kot
{
    /// <summary>
    /// Interaction logic for ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        public ConnectionWindow()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;
            string address = AddressBox.Text;
            string portText = PortBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(portText))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            if (!int.TryParse(portText, out int port))
            {
                MessageBox.Show("Port must be a number.");
                return;
            }

            ConnectButton.IsEnabled = false;

            var progress = new ProgressWindow();
            progress.Owner = this;
            progress.Show();

            Random random = new Random();

            await Task.Delay(random.Next(1000, 4444));

            progress.Close();

            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(address, port);

                MessageBox.Show("Connected successfully!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}");
            }
            finally
            {
                ConnectButton.IsEnabled = true;
                //ProgressBar.Visibility = Visibility.Collapsed;
            }
        }
    }
}
