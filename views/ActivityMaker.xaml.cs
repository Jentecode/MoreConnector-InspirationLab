using System.Windows;
using System.Windows.Controls;

namespace MoreConnector.Views
{
    public partial class ActivityMaker : Page
    {
        private string _geselecteerdeAfbeeldingPad = null;

        public ActivityMaker()
        {
            InitializeComponent();
        }

        private void OnAanmakenBevestigenClick(object sender, RoutedEventArgs e)
        {
            string naam = NaamInput.Text.Trim();
            string categorie = CategorieInput.Text.Trim();
            string datumTijd = DatumTijdInput.Text.Trim();
            string locatie = LocatieInput.Text.Trim();
            string beschrijving = BeschrijvingInput.Text.Trim();

            if (string.IsNullOrEmpty(naam) || string.IsNullOrEmpty(locatie) || string.IsNullOrEmpty(datumTijd))
            {
                MessageBox.Show("Vul minstens een naam, locatie en datum & tijd in.", "Validatie");
                return;
            }

            // Voeg de activiteit toe aan de gedeelde lijst van ActiviteitenPage
            ActivityPage.Activiteits.Add(new Activiteit
            {
                Titel = $"{naam} - {datumTijd}",
                Locatie = locatie,
                CanDelete = true
            });

            // Navigeer terug naar activiteiten
            NavigationService?.Navigate(new ActivityMaker());
        }

        private void OnAnnulerenClick(object sender, RoutedEventArgs e)
            => NavigationService?.GoBack();

        private void OnAfbeeldingClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Afbeeldingen|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                _geselecteerdeAfbeeldingPad = dialog.FileName;
                AfbeeldingButton.Content = $"✓ {System.IO.Path.GetFileName(dialog.FileName)}";
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