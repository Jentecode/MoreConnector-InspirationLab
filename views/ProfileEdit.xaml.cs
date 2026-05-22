using MoreConnector.Database;
using MoreConnector.Models;
using System.Windows;
using System.Windows.Controls;

namespace MoreConnector.Views
{
    public partial class ProfileEditPage : Page
    {
        private readonly AppState _state = AppState.Instance;

        public ProfileEditPage()
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Profiel);
            LaadHuidigeData();
        }

        private void LaadHuidigeData()
        {
            var user = _state.HuidigeGebruiker;
            if (user == null) return;

            VoornaamInput.Text       = user.Voornaam;
            AchternaamInput.Text     = user.Achternaam;
            EmailInput.Text          = user.Email;
            TelefoonnummerInput.Text = user.Telefoonnummer;
            StudierichtingInput.Text = user.Studierichting;
            BioInput.Text            = user.Bio;
            UsernameInput.Text       = user.Username;

            LaadFotoPreview(user.ProfielFotoPad);
        }

        private void LaadFotoPreview(string pad)
        {
            var bmp = ImageHelper.LaadGeschaald(pad, 220);
            if (bmp != null)
            {
                FotoPreview.Source = bmp;
                FotoAchtergrond.Visibility = Visibility.Collapsed;
            }
            else
            {
                FotoPreview.Source = null;
                FotoAchtergrond.Visibility = Visibility.Visible;
            }
        }

        private void OnOpslaanClick(object sender, RoutedEventArgs e)
        {
            string voornaam       = VoornaamInput.Text.Trim();
            string achternaam     = AchternaamInput.Text.Trim();
            string email          = EmailInput.Text.Trim();
            string telefoon       = TelefoonnummerInput.Text.Trim();
            string studierichting = StudierichtingInput.Text.Trim();
            string bio            = BioInput.Text.Trim();
            string username       = UsernameInput.Text.Trim();

            if (string.IsNullOrEmpty(voornaam) || string.IsNullOrEmpty(achternaam))
            {
                MessageBox.Show("Voornaam en achternaam zijn verplicht.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!UsernameValidator.IsGeldig(voornaam) || !UsernameValidator.IsGeldig(achternaam))
            {
                MessageBox.Show("Voor- of achternaam bevat ongepaste woorden.", "Ongepaste naam",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var usernameError = UsernameValidator.Valideer(username);
            if (usernameError != null)
            {
                MessageBox.Show(usernameError, "Ongepaste gebruikersnaam",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(email) || !IsGeldigEmail(email))
            {
                MessageBox.Show("Vul een geldig e-mailadres in.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _state.PasProfielToe(voornaam, achternaam, email, telefoon, studierichting, bio, username,
                _state.HuidigeGebruiker?.ProfielFotoPad ?? "");

            MessageBox.Show("Profiel opgeslagen!", "Opgeslagen", MessageBoxButton.OK, MessageBoxImage.Information);
            Nav().AuthFrame.Navigate(new ProfilePage());
        }
        private void OnWachtwoordWijzigenClick(object sender, RoutedEventArgs e)
        {
            var user = _state.HuidigeGebruiker;
            if (user == null) return;

            string huidig = TxtHuidigWachtwoord.Password;
            string nieuw1 = TxtNieuwWachtwoord.Password;
            string nieuw2 = TxtBevestigWachtwoord.Password;

            if (string.IsNullOrWhiteSpace(huidig))
            {
                MessageBox.Show("Vul je huidig wachtwoord in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (nieuw1.Length < 6)
            {
                MessageBox.Show("Nieuw wachtwoord moet minstens 6 tekens zijn.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (nieuw1 != nieuw2)
            {
                MessageBox.Show("Nieuwe wachtwoorden komen niet overeen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool gelukt = UserRepository.WijzigEigenWachtwoord(user.Id, huidig, nieuw1);
                if (!gelukt)
                    MessageBox.Show("Huidig wachtwoord is onjuist.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                {
                    MessageBox.Show("Wachtwoord succesvol gewijzigd!", "Opgeslagen", MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtHuidigWachtwoord.Password   = "";
                    TxtNieuwWachtwoord.Password    = "";
                    TxtBevestigWachtwoord.Password = "";
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Fout: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnWijzigFotoClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Afbeeldingen|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp",
                Title  = "Kies profielfoto"
            };
            if (dialog.ShowDialog() == true && _state.HuidigeGebruiker != null)
            {
                _state.HuidigeGebruiker.ProfielFotoPad = dialog.FileName;
                LaadFotoPreview(dialog.FileName);
            }
        }

        private void OnVerwijderFotoClick(object sender, RoutedEventArgs e)
        {
            if (_state.HuidigeGebruiker != null) _state.HuidigeGebruiker.ProfielFotoPad = "";
            LaadFotoPreview("");
        }

        private void OnAccountVerwijderenClick(object sender, RoutedEventArgs e)
        {
            var bevestig = MessageBox.Show("Weet je zeker dat je je account wil verwijderen?",
                "Account verwijderen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (bevestig == MessageBoxResult.Yes)
            {
                if (_state.HuidigeGebruiker != null)
                    try { UserRepository.Verwijder(_state.HuidigeGebruiker.Id); } catch { }
                _state.HuidigeGebruiker = null;
                Nav().NavigateToLogin();
            }
        }

        private void SideNav_Home(object sender, RoutedEventArgs e)           => Nav().AuthFrame.Navigate(new Feed());
        private void SideNav_Activiteiten(object sender, RoutedEventArgs e)   => Nav().AuthFrame.Navigate(new ActivityPage());
        private void SideNav_Berichten(object sender, RoutedEventArgs e)      => Nav().AuthFrame.Navigate(new MessagePage());
        private void SideNav_Gebruikers(object sender, RoutedEventArgs e)     => Nav().AuthFrame.Navigate(new GebruikersPage());
        private void SideNav_Notificaties(object sender, RoutedEventArgs e)   => Nav().AuthFrame.Navigate(new NotificatiePage());
        private void SideNav_Aanmaken(object sender, RoutedEventArgs e)       => Nav().AuthFrame.Navigate(new AanmakenKeuze());
        private void SideNav_Profiel(object sender, RoutedEventArgs e)        => Nav().AuthFrame.Navigate(new ProfilePage());
        private void SideNav_Admin(object sender, RoutedEventArgs e)          => Nav().AuthFrame.Navigate(new AdminPage());
        private void SideNav_Uitloggen(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("Wil je uitloggen?", "Uitloggen", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes) { AppState.Instance.HuidigeGebruiker = null; Nav().NavigateToLogin(); }
        }
        private void OnProfielAvatarClick(object sender, RoutedEventArgs e) => Nav().AuthFrame.Navigate(new ProfilePage());
        private void OnHomeClick(object sender, RoutedEventArgs e)          => Nav().AuthFrame.Navigate(new Feed());
        private void OnBerichtenClick(object sender, RoutedEventArgs e)     => Nav().AuthFrame.Navigate(new MessagePage());
        private void OnAanmakenClick(object sender, RoutedEventArgs e)      => Nav().AuthFrame.Navigate(new AanmakenKeuze());
        private void OnProfielClick(object sender, RoutedEventArgs e)       => Nav().AuthFrame.Navigate(new ProfilePage());

        private static bool IsGeldigEmail(string email)
        {
            try { _ = new System.Net.Mail.MailAddress(email); return true; }
            catch { return false; }
        }

        private MoreConnector Nav() => (MoreConnector)Window.GetWindow(this);
    }
}
