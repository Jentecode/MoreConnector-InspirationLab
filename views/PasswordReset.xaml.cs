using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MoreConnector
{
    public partial class PasswordReset : Page
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

        private void BtnTerug_Click(object sender, RoutedEventArgs e)
            => ((Views.MoreConnector)Window.GetWindow(this)).NavigateToLogin();

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || email == "gebruikersnaam of email-adres" || !email.Contains("@"))
            {
                MessageBox.Show("Vul een geldig e-mailadres in.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: stuur reset-mail via backend
            MessageBox.Show("Mail verstuurd!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            ((Views.MoreConnector)Window.GetWindow(this)).NavigateToLogin();
        }
    }
}
