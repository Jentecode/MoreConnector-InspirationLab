using MoreConnector.Database;
using MoreConnector.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MoreConnector
{
    public partial class AccountCreation : Page
    {
        public AccountCreation()
        {
            InitializeComponent();
        }

        private void TxtVoornaam_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtVoornaam.Text == "Voornaam") { TxtVoornaam.Text = ""; TxtVoornaam.Foreground = Brushes.Black; }
        }
        private void TxtVoornaam_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtVoornaam.Text)) { TxtVoornaam.Text = "Voornaam"; TxtVoornaam.Foreground = Brushes.Gray; }
        }

        private void TxtAchternaam_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtAchternaam.Text == "Achternaam") { TxtAchternaam.Text = ""; TxtAchternaam.Foreground = Brushes.Black; }
        }
        private void TxtAchternaam_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtAchternaam.Text)) { TxtAchternaam.Text = "Achternaam"; TxtAchternaam.Foreground = Brushes.Gray; }
        }

        private void TxtNickname_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtNickname.Text == "Nickname / gebruikersnaam") { TxtNickname.Text = ""; TxtNickname.Foreground = Brushes.Black; }
        }
        private void TxtNickname_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNickname.Text)) { TxtNickname.Text = "Nickname / gebruikersnaam"; TxtNickname.Foreground = Brushes.Gray; }
        }

        private void TxtEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtEmail.Text == "e-mailadres") { TxtEmail.Text = ""; TxtEmail.Foreground = Brushes.Black; }
        }
        private void TxtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEmail.Text)) { TxtEmail.Text = "e-mailadres"; TxtEmail.Foreground = Brushes.Gray; }
        }

        private void TxtTelefoon_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtTelefoon.Text == "telefoonnummer") { TxtTelefoon.Text = ""; TxtTelefoon.Foreground = Brushes.Black; }
        }
        private void TxtTelefoon_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTelefoon.Text)) { TxtTelefoon.Text = "telefoonnummer"; TxtTelefoon.Foreground = Brushes.Gray; }
        }

        private void TxtStudierichting_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtStudierichting.Text == "studierichting") { TxtStudierichting.Text = ""; TxtStudierichting.Foreground = Brushes.Black; }
        }
        private void TxtStudierichting_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtStudierichting.Text)) { TxtStudierichting.Text = "studierichting"; TxtStudierichting.Foreground = Brushes.Gray; }
        }

        private void BtnTerug_Click(object sender, RoutedEventArgs e)
            => ((Views.MoreConnector)Window.GetWindow(this)).NavigateToLogin();

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            string voornaam = TxtVoornaam.Text.Trim();
            string achternaam = TxtAchternaam.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string nickname = TxtNickname.Text.Trim();
            if (nickname == "Nickname / gebruikersnaam") nickname = "";

            if (string.IsNullOrWhiteSpace(voornaam) || voornaam == "Voornaam" ||
                string.IsNullOrWhiteSpace(achternaam) || achternaam == "Achternaam")
            {
                MessageBox.Show("Voornaam en achternaam zijn verplicht.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Models.UsernameValidator.IsGeldig(voornaam) || !Models.UsernameValidator.IsGeldig(achternaam))
            {
                MessageBox.Show("Voor- of achternaam bevat ongepaste woorden.", "Ongepaste naam",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Models.UsernameValidator.IsGeldig(email.Split('@')[0]))
            {
                MessageBox.Show("E-mailadres bevat ongepaste woorden.", "Ongepaste inhoud",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || email == "e-mailadres" || !IsGeldigEmail(email))
            {
                MessageBox.Show("Vul een geldig e-mailadres in.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TxtWachtwoord.Password.Length < 6)
            {
                MessageBox.Show("Wachtwoord moet minstens 6 tekens zijn.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Check of email al in gebruik is
                if (UserRepository.EmailBestaat(email))
                {
                    MessageBox.Show("Dit e-mailadres is al in gebruik.", "Registratie mislukt",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string studierichting = TxtStudierichting.Text.Trim();
                if (studierichting == "studierichting") studierichting = "";

                string gebruikersnaam = string.IsNullOrWhiteSpace(nickname)
                    ? email.Split('@')[0]
                    : nickname;

                // Check ongepaste username
                var usernameError = Models.UsernameValidator.Valideer(gebruikersnaam);
                if (usernameError != null)
                {
                    MessageBox.Show(usernameError, "Ongepaste gebruikersnaam",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int newId = UserRepository.Registreer(
                    voornaam, achternaam, email,
                    TxtWachtwoord.Password,
                    studierichting, "", gebruikersnaam);

                // Stuur verificatiecode
                string token = Database.EmailVerificationRepository.MaakToken(email);
                Database.EmailService.StuurEmailVerificatie(email, token);

                MessageBox.Show($"Account aangemaakt! Er is een verificatiecode verstuurd naar {email}.\nVerifieer je e-mailadres om in te loggen.",
                    "Bijna klaar!", MessageBoxButton.OK, MessageBoxImage.Information);
                ((Views.MoreConnector)Window.GetWindow(this)).NavigateToEmailVerification(email);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Fout bij aanmaken account:\n{ex.Message}", "Fout",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtWachtwoord_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (WachtwoordPlaceholder != null)
                WachtwoordPlaceholder.Visibility = TxtWachtwoord.Password.Length > 0
                    ? Visibility.Collapsed : Visibility.Visible;
        }

        private static bool IsGeldigEmail(string email)
        {
            try { _ = new System.Net.Mail.MailAddress(email); return true; }
            catch { return false; }
        }
    }
}
