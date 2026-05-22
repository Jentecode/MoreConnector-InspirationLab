using MoreConnector.Database;
using System.Windows;
using System.Windows.Controls;

namespace MoreConnector.Views
{
    public partial class EmailVerification : Page
    {
        private readonly string _email;

        public EmailVerification(string email)
        {
            InitializeComponent();
            _email = email;
            InfoTekst.Text = $"We hebben een verificatiecode gestuurd naar {email}. Voer de 6-cijferige code in om je account te activeren.";
        }

        private void BtnBevestig_Click(object sender, RoutedEventArgs e)
        {
            string code = TxtCode.Text.Trim();

            if (code.Length != 6)
            {
                ToonFout("Voer de 6-cijferige code in.");
                return;
            }

            if (!EmailVerificationRepository.ValideerToken(_email, code))
            {
                ToonFout("Ongeldige of verlopen code. Vraag een nieuwe code aan.");
                return;
            }

            EmailVerificationRepository.MarkeerGeverifieerd(_email);

            MessageBox.Show("Je e-mailadres is bevestigd! Je kan nu inloggen.",
                "Geverifieerd ✓", MessageBoxButton.OK, MessageBoxImage.Information);

            Nav().NavigateToLogin();
        }

        private void BtnOpnieuw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnOpnieuw.IsEnabled = false;
                BtnOpnieuw.Content   = "Verzonden!";
                string token = EmailVerificationRepository.MaakToken(_email);
                EmailService.StuurEmailVerificatie(_email, token);
                VerbergFout();
            }
            catch (System.Exception ex)
            {
                ToonFout($"Fout: {ex.Message}");
                BtnOpnieuw.IsEnabled = true;
                BtnOpnieuw.Content   = "Code opnieuw sturen";
            }
        }

        private void BtnTerug_Click(object sender, RoutedEventArgs e)
            => Nav().NavigateToLogin();

        private void ToonFout(string tekst)
        {
            FoutTekst.Text       = tekst;
            FoutTekst.Visibility = Visibility.Visible;
        }

        private void VerbergFout() => FoutTekst.Visibility = Visibility.Collapsed;

        private MoreConnector Nav() => (MoreConnector)Window.GetWindow(this);
    }
}
