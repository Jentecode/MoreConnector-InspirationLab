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
            if (TxtUsername.Text == "e-mailadres")
            { TxtUsername.Text = ""; TxtUsername.Foreground = Brushes.Black; }
        }

        private void TxtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text))
            { TxtUsername.Text = "e-mailadres"; TxtUsername.Foreground = Brushes.Gray; }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var main  = (Views.MoreConnector)Window.GetWindow(this);
            var state = AppState.Instance;

            string invoer   = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(invoer) || invoer == "e-mailadres")
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
            try
            {
                var user = UserRepository.Login(invoer, password);
                if (user == null)
                {
                    // Controleer of het account gebanned is (is_active=0)
                    try
                    {
                        using var conn2 = MoreConnector.Database.DbConnection.GetConnection();
                        using var chk = conn2.CreateCommand();
                        chk.CommandText = "SELECT COALESCE(is_active,1) FROM users WHERE email=@e AND PASSWORD=@p LIMIT 1";
                        chk.Parameters.AddWithValue("@e", invoer);
                        chk.Parameters.AddWithValue("@p", UserRepository.HashPassword(password));
                        var res = chk.ExecuteScalar();
                        if (res != null && Convert.ToInt32(res) == 0)
                        {
                            MessageBox.Show("Je account is geblokkeerd. Neem contact op met een beheerder.", "Account geblokkeerd",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    catch { }
                    MessageBox.Show("E-mailadres of wachtwoord is onjuist.", "Inlogfout",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Username instellen (niet gedwongen vanuit email)
                if (string.IsNullOrWhiteSpace(user.Username))
                    user.Username = invoer.Contains("@") ? invoer.Split('@')[0] : invoer;
                // Check e-mailverificatie
                if (!user.IsVerified)
                {
                    MessageBox.Show("Verifieer eerst je e-mailadres.\nCheck je inbox voor de verificatiecode.",
                        "Niet geverifieerd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    main.NavigateToEmailVerification(user.Email);
                    return;
                }

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
