using System.IO;
using System.Net;
using System.Net.Http;
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
using static Server.MainWindow;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ServerListener server;


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
            try
            {
                string username = UsernameBox.Text;
                string password = PasswordBox.Password;
                string address = AddressBox.Text;
                int port = int.Parse(PortBox.Text);

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Password cannot be empty.");
                    return;
                }

                server = new ServerListener(address, port, password);

                server.LogEvent += AppendLog;
                server.ClientConnected += AddClient;
                server.ClientDisconnected += RemoveClient;
                server.BroadcastReceived += AppendLog;

                server.Start();

                AppendLog("Server started.");
                StartButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start server: {ex.Message}");
            }
        }




        private void AppendLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ServerLog.AppendText(message + Environment.NewLine);
                ServerLog.ScrollToEnd();
            });
        }

        private void AddClient(string username)
        {
            Dispatcher.Invoke(() =>
            {
                ClientsList.Items.Add(username);
            });
        }

        private void RemoveClient(string username)
        {
            Dispatcher.Invoke(() =>
            {
                ClientsList.Items.Remove(username);
            });
        }

        private void BroadcastBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    int ind = BroadcastBox.CaretIndex;
                    BroadcastBox.Text = BroadcastBox.Text.Insert(ind, Environment.NewLine);
                    BroadcastBox.CaretIndex = ind + Environment.NewLine.Length;
                    e.Handled = true;
                }
                else
                {
                    string message = BroadcastBox.Text.Trim();
                    if (!string.IsNullOrEmpty(message))
                    {
                        server?.BroadcastSystemMessage(message);
                        AppendLog($"[Broadcast] {message}");
                        BroadcastBox.Clear();
                    }
                    e.Handled = true;
                }
            }
        }
    }







    public class ServerListener
        {
            private TcpListener tcpListener;
            private bool running = false;
            private readonly List<Client> clients = new List<Client>();

            public event Action<string> LogEvent;
            public event Action<string> ClientConnected;
            public event Action<string> ClientDisconnected;
            public event Action<string> BroadcastReceived;



            private readonly string expectedPassword;

            public ServerListener(string ipAddress, int port, string expected)
            {
                tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
                expectedPassword = expected;
            }


            public void Start()
            {
                running = true;
                tcpListener.Start();
                Log($"Server started on {tcpListener.LocalEndpoint}");

                Thread acceptThread = new Thread(AcceptClients);
                acceptThread.IsBackground = true;
                acceptThread.Start();
            }


            private void Log(string message)
            {
                LogEvent?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
            }


            public void Stop()
            {
                running = false;
                tcpListener.Stop();
                foreach (var client in clients)
                    client.Disconnect();
                clients.Clear();
                Log("Server stopped.");
            }



            private void AcceptClients()
            {
                while (running)
                {
                    try
                    {
                        TcpClient tcpClient = tcpListener.AcceptTcpClient();
                        var handler = new Client(tcpClient, expectedPassword);
                        handler.LogEvent += Log;
                        handler.Disconnected += OnClientDisconnected;
                        handler.Authorized += OnClientAuthorized;
                        handler.MessageReceived += OnMessageReceived;

                        Thread clientThread = new Thread(handler.HandleClient);
                        clientThread.IsBackground = true;
                        clientThread.Start();
                    }
                    catch (SocketException ex)
                    {
                        Log($"SocketException: {ex.Message}");
                    }
                }
            }



            private void OnClientAuthorized(Client handler)
            {
                clients.Add(handler);
                ClientConnected?.Invoke(handler.Username);
                BroadcastSystemMessage($"User {handler.Username} connected.");
            }



            private void OnClientDisconnected(Client handler)
            {
                clients.Remove(handler);
                ClientDisconnected?.Invoke(handler.Username);
                BroadcastSystemMessage($"User {handler.Username} disconnected.");
            }



            private void OnMessageReceived(Client sender, string message)
            {
                BroadcastReceived?.Invoke($"{sender.Username}: {message}");
                foreach (var client in clients)
                {
                    if (client != sender)
                        client.SendMessage($"{sender.Username}: {message}");
                }
            }


            public void BroadcastSystemMessage(string message)
            {
                Log(message);
/*                foreach (var client in clients)
                    client.SendMessage($"[SYSTEM]: {message}");*/

            foreach (var client in clients.ToList())
            {
                try
                {
                    client.SendMessage($"[SYSTEM]: {message}");
                }
                catch (Exception ex)
                {
                    Log($"Failed to send system message to '{client.Username}': {ex.Message}");
                }
            }

        }



        public void KickClient(string username)
            {
                var client = clients.Find(c => c.Username == username);
                if (client != null)
                {
                    client.SendMessage("[SYSTEM]: You have been kicked.");
                    client.Disconnect();
                }
            }
        }




        public class Client
        {
            private TcpClient tcpClient;
            private NetworkStream _stream;
            private StreamReader _reader;
            private StreamWriter _writer;
            private string expectedPassword;

            public string Username { get; private set; }


            public event Action<string> LogEvent;
            public event Action<Client> Authorized;
            public event Action<Client> Disconnected;
            public event Action<Client, string> MessageReceived;


            public Client(TcpClient client, string expectedPassword)
            {
                tcpClient = client;
                expectedPassword = expectedPassword;
            }


        public void HandleClient()
        {
            try
            {
                _stream = tcpClient.GetStream();
                _reader = new StreamReader(_stream, Encoding.UTF8);
                _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

                var attemptedUsername = _reader.ReadLine();
                if (string.IsNullOrWhiteSpace(attemptedUsername))
                {
                    Log("Client did not provide a username.");
                    Disconnect();
                    return;
                }

                Username = attemptedUsername;
                Log($"Connection attempt from '{Username}'");

                string password = _reader.ReadLine();
                if (string.IsNullOrWhiteSpace(password))
                {
                    Log($"Client '{Username}' did not provide a password.");
                    Disconnect();
                    return;
                }

                if (password == expectedPassword)
                {
                    SendMessage("AUTH_OK");
                    Authorized?.Invoke(this);
                }
                else
                {
                    SendMessage("AUTH_FAIL");
                    Disconnect();
                    return;
                }

                string message;
                while ((message = _reader.ReadLine()) != null)
                {
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (Exception ex)
            {
                Log($"Error with client '{Username}': {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }


        public void SendMessage(string message)
            {
                try
                {
                    _writer.WriteLine(message);
                }
                catch (Exception ex)
                {
                    Log($"Send error to '{Username}': {ex.Message}");
                }
            }



            public void Disconnect()
            {
                tcpClient?.Close();
                Disconnected?.Invoke(this);
            }

            private void Log(string message)
            {
                LogEvent?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
            }
        }



}