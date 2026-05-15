using MoreConnector.Database;
using MoreConnector.Models;
using MoreConnector.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MoreConnector
{
    public partial class Login : Page
    {
        public Login() { InitializeComponent(); }

        private void TxtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtUsername.Text == "gebruikersnaam")
            { TxtUsername.Text = ""; TxtUsername.Foreground = Brushes.Black; }
        }

        private void TxtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text))
            { TxtUsername.Text = "gebruikersnaam"; TxtUsername.Foreground = Brushes.Gray; }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var main  = (Views.MoreConnector)Window.GetWindow(this);
            var state = AppState.Instance;

            string invoer   = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(invoer) || invoer == "gebruikersnaam")
            {
                MessageBox.Show("Vul je e-mailadres in.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vul je wachtwoord in.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ── DB LOGIN ─────────────────────────────────────────────────────
            try
            {
                var user = UserRepository.Login(invoer, password);
                if (user == null)
                {
                    MessageBox.Show("E-mailadres of wachtwoord is onjuist.", "Inlogfout",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Username instellen (niet gedwongen vanuit email)
                if (string.IsNullOrWhiteSpace(user.Username))
                    user.Username = invoer.Contains("@") ? invoer.Split('@')[0] : invoer;
                state.HuidigeGebruiker = user;
                state.LaadAlles();

                // Admin check via is_admin veld
                if (user.IsAdmin)
                {
                    user.Role = "Admin";
                    main.NavigateToFeed(); // admins gaan ook naar feed, maar zien Beheer knop
                }
                else
                {
                    main.NavigateToFeed();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Fout bij inloggen:\n{ex.Message}", "Fout",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtCreateAccount_Click(object sender, MouseButtonEventArgs e)
            => ((Views.MoreConnector)Window.GetWindow(this)).NavigateToCreateAccount();

        private void TxtForgotPassword_Click(object sender, MouseButtonEventArgs e)
            => ((Views.MoreConnector)Window.GetWindow(this)).NavigateToPasswordReset();

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordPlaceholder != null)
                PasswordPlaceholder.Visibility =
                    TxtPassword.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
