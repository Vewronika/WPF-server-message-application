using System.Collections.ObjectModel;
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

namespace kot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class Message
    {
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public bool IsOwnMessage { get; set; }
    }



    public partial class MainWindow : Window
    {
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }



        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(SendBox.Text))
            {
                var newMessage = new Message
                {
                    Username = "User1",
                    Content = SendBox.Text
                };

                Messages.Add(newMessage);
                SendBox.Clear();
            }
        }

        private void MenuItem_About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Group char:>.",
            "Group chat", MessageBoxButton.OK,
            MessageBoxImage.Information);
            

    }

        private void MenuItem_Connect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ConnectionWindow();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                MenuItem_Connect.IsEnabled = false;
                MenuItem_Disconnect.IsEnabled = true;

                Messages.Add(new Message
                {
                    Username = "System",
                    Content = "Connected"
                });
            }

        }

        private void MenuItem_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Connect.IsEnabled = true;
            MenuItem_Disconnect.IsEnabled = false;
            /*            SystemMessageAText.Text = "Disonnected";
                        SystemMessageA.Visibility = Visibility.Visible;*/

            Messages.Add(new Message
            {
                Username = "System",
                Content = "Disonnected"
            });
        }

        private void SendBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    var c = SendBox.CaretIndex;
                    SendBox.Text = SendBox.Text.Insert(c, Environment.NewLine);
                    SendBox.CaretIndex = c + Environment.NewLine.Length;
                    e.Handled = true;
                }

                else
                {
                    if (!string.IsNullOrWhiteSpace(SendBox.Text))
                    {
                        var newMessage = new Message
                        {
                            Username = "User1",
                            Content = SendBox.Text
                        };

                        Messages.Add(newMessage);
                        SendBox.Clear();
                    }

                    e.Handled = true;
                }
            }
            
        }

        private void SendBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }



}