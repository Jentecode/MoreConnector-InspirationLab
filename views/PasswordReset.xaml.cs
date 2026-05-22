using MoreConnector.Database;
using System.Windows;
using System.Windows.Controls;

namespace MoreConnector.Views
{
    public partial class PasswordReset : Page
    {
        private string _email = "";

        public PasswordReset() { InitializeComponent(); }
        private void BtnStuurCode_Click(object sender, RoutedEventArgs e)
        {
            _email = TxtEmail.Text.Trim().ToLower();

            if (!IsGeldigEmail(_email))
            {
                ToonFout("Vul een geldig e-mailadres in.");
                return;
            }

            if (!UserRepository.EmailBestaat(_email))
            {
                ToonFout("Dit e-mailadres is niet bekend.");
                return;
            }

            try
            {
                BtnStuurCode.IsEnabled = false;
                BtnStuurCode.Content   = "Versturen...";

                string token = PasswordResetRepository.MaakToken(_email);
                EmailService.StuurWachtwoordReset(_email, token);

                // Naar stap 2
                StapEmail.Visibility = Visibility.Collapsed;
                StapCode.Visibility  = Visibility.Visible;
                SubTekst.Text        = $"We hebben een 6-cijferige code gestuurd naar {_email}. Voer die hieronder in.";
                VerbergFout();
            }
            catch (System.Exception ex)
            {
                ToonFout($"Fout bij versturen: {ex.Message}");
                BtnStuurCode.IsEnabled = true;
                BtnStuurCode.Content   = "Stuur resetcode";
            }
        }
        private void BtnBevestigCode_Click(object sender, RoutedEventArgs e)
        {
            string code = TxtCode.Text.Trim();

            if (code.Length != 6)
            {
                ToonFout("Voer de 6-cijferige code in.");
                return;
            }

            if (!PasswordResetRepository.ValideerToken(_email, code))
            {
                ToonFout("Ongeldige of verlopen code. Probeer opnieuw.");
                return;
            }

            // Naar stap 3
            StapCode.Visibility            = Visibility.Collapsed;
            StapNieuwWachtwoord.Visibility = Visibility.Visible;
            SubTekst.Text                  = "Kies een nieuw wachtwoord.";
            VerbergFout();
        }
        private void BtnOpslaanWachtwoord_Click(object sender, RoutedEventArgs e)
        {
            string nieuw     = TxtNieuw.Password;
            string bevestig  = TxtBevestig.Password;

            if (nieuw.Length < 6)
            {
                ToonFout("Wachtwoord moet minstens 6 tekens zijn.");
                return;
            }
            if (nieuw != bevestig)
            {
                ToonFout("Wachtwoorden komen niet overeen.");
                return;
            }

            var user = UserRepository.GetByEmail(_email);
            if (user == null) { ToonFout("Gebruiker niet gevonden."); return; }

            UserRepository.UpdateWachtwoord(user.Id, nieuw);
            PasswordResetRepository.Verwijder(_email);

            MessageBox.Show("Wachtwoord succesvol gewijzigd! Je kan nu inloggen.",
                "Gelukt", MessageBoxButton.OK, MessageBoxImage.Information);

            Nav().NavigateToLogin();
        }

        private void BtnTerug_Click(object sender, RoutedEventArgs e)
            => Nav().NavigateToLogin();

        private void ToonFout(string tekst)
        {
            FoutTekst.Text       = tekst;
            FoutTekst.Visibility = Visibility.Visible;
        }

        private void VerbergFout() => FoutTekst.Visibility = Visibility.Collapsed;

        private static bool IsGeldigEmail(string email)
        {
            try { _ = new System.Net.Mail.MailAddress(email); return true; }
            catch { return false; }
        }

        private MoreConnector Nav()
            => (MoreConnector)Window.GetWindow(this);
    }
}
