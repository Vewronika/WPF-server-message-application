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
        public string message { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
    }



    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }



        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = SendBox.Text;

            SendBox.Clear();
        }

        private void MenuItem_About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Group char:>.",
            "Group chat", MessageBoxButton.OK,
            MessageBoxImage.Information);
            

    }

        private void MenuItem_Connect_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Connect.IsEnabled = false;
            MenuItem_Disconnect.IsEnabled = true;

            SystemMessageAText.Text = "Connected";
            SystemMessageA.Visibility = Visibility.Visible;
        }

        private void MenuItem_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Connect.IsEnabled = true;
            MenuItem_Disconnect.IsEnabled = false;
            SystemMessageAText.Text = "Disonnected";
            SystemMessageA.Visibility = Visibility.Visible;
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

                    string message = SendBox.Text;

                    SendBox.Clear();

                    e.Handled = true;
                }
            }
            
        }

        private void SendBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }



}