using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mail verstuurd!");
            ((Views.MoreConnector)Window.GetWindow(this)).NavigateToLogin();
        }
    }
}