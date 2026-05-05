using MoreConnector.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace MoreConnector
{
    public partial class Login : Page
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
       
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            bool loginSuccess = true; // replace with real check
            if (loginSuccess)
            {
                ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new Feed());
            }
            else
            {
                MessageBox.Show("Login mislukt!");
            }
        }
        private void TxtCreateAccount_Click(object sender, MouseButtonEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).NavigateToCreateAccount();
        }
        private void TxtForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).NavigateToPasswordReset();
        }
        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordPlaceholder != null)
            {
                PasswordPlaceholder.Visibility = TxtPassword.Password.Length > 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }
        private void TxtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}