using System.Windows;
using System.Windows.Media;

namespace MoreConnector
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void TxtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtUsername.Text == "gebruikersnaam of email-adres")
            {
                TxtUsername.Text = "";
                TxtUsername.Foreground = Brushes.Black;
            }
        }

        private void TxtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text))
            {
                TxtUsername.Text = "gebruikersnaam of email-adres";
                TxtUsername.Foreground = Brushes.Gray;
            }
        }

        private void TxtPhone_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtPhone.Text == "Telefoonnummer")
            {
                TxtPhone.Text = "";
                TxtPhone.Foreground = Brushes.Black;
            }
        }

        private void TxtPhone_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtPhone.Text))
            {
                TxtPhone.Text = "Telefoonnummer";
                TxtPhone.Foreground = Brushes.Gray;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Add your login logic here
            MessageBox.Show("Inloggen geklikt!");
        }
    }
}