using System.Windows;
using System.Windows.Media;

namespace MoreConnector
{
    public partial class PasswordReset : Window
    {
        public PasswordReset()
        {
            InitializeComponent();
        }

        private void TxtEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtEmail.Text == "gebruikersnaam of email-adres")
            {
                TxtEmail.Text = "";
                TxtEmail.Foreground = Brushes.Black;
            }
        }

        private void TxtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                TxtEmail.Text = "gebruikersnaam of email-adres";
                TxtEmail.Foreground = Brushes.Gray;
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            // Add your password reset logic here
            MessageBox.Show("Mail verstuurd!");
        }
    }
}