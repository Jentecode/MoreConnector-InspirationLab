using System.Windows;
using System.Windows.Controls;

namespace MoreConnector.Views
{
    public partial class ProfileEditPage : Page
    {
        public ProfileEditPage()
        {
            InitializeComponent();
        }

        private void OnOpslaanClick(object sender, RoutedEventArgs e)
        {
            string voornaam = VoornaamInput.Text.Trim();
            string achternaam = AchternaamInput.Text.Trim();
            string email = EmailInput.Text.Trim();
            string telefoonnummer = TelefoonnummerInput.Text.Trim();
            string studierichting = StudierichtingInput.Text.Trim();
            string bio = BioInput.Text.Trim();

            if (string.IsNullOrEmpty(voornaam) || string.IsNullOrEmpty(achternaam))
            {
                MessageBox.Show("Voornaam en achternaam zijn verplicht.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                MessageBox.Show("Vul een geldig e-mailadres in.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: sla op in database
            MessageBox.Show("Profiel opgeslagen!", "Opgeslagen",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnWijzigFotoClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Afbeeldingen|*.jpg;*.jpeg;*.png;*.bmp"
            };
            if (dialog.ShowDialog() == true)
            {
                // TODO: sla het gekozen bestand op en toon het in de UI
            }
        }

        private void OnVerwijderFotoClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Profielfoto verwijderd.", "Foto");
        }

        private void OnAccountVerwijderenClick(object sender, RoutedEventArgs e)
        {
            var bevestig = MessageBox.Show(
                "Weet je zeker dat je je account wil verwijderen? Dit kan niet ongedaan worden gemaakt.",
                "Account verwijderen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (bevestig == MessageBoxResult.Yes)
            {
                // TODO: verwijder account uit database en navigeer naar loginscherm
                MessageBox.Show("Account verwijderd.", "Account");
            }
        }

        private void OnHomeClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new Feed());
        }

        private void OnActiviteitenClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ActivityPage());
        }

        private void OnBerichtenClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new MessagePage());
        }

        private void OnAanmakenClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ActivityMaker());
        }

        private void OnProfielClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ProfilePage());
        }
    }
}