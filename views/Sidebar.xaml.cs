using MoreConnector.Models;
using System.Windows;
using System.Windows.Controls;

namespace MoreConnector.Views
{
    public partial class Sidebar : UserControl
    {
        // Welke pagina is actief — wordt gezet door de host-pagina
        public enum ActivePage { Home, Activiteiten, Berichten, Aanmaken, Profiel, Admin, Geen }

        public Sidebar()
        {
            InitializeComponent();
            // UserControl.Loaded → profielfoto + naam + admin-knop instellen
            Loaded += (_, _) => Refresh();
        }

        /// <summary>Roep aan vanuit de host-pagina zodat de juiste knop actief is.</summary>
        public void SetActive(ActivePage pagina)
        {
            // Reset alles
            BtnHome.Style         = (Style)Resources["NavBtn"];
            BtnActiviteiten.Style = (Style)Resources["NavBtn"];
            BtnBerichten.Style    = (Style)Resources["NavBtn"];
            BtnAanmaken.Style     = (Style)Resources["NavBtn"];
            BtnProfiel.Style      = (Style)Resources["NavBtn"];
            BtnAdmin.Style        = (Style)Resources["NavBtn"];

            // Highlight actieve
            var actief = pagina switch
            {
                ActivePage.Home         => BtnHome,
                ActivePage.Activiteiten => BtnActiviteiten,
                ActivePage.Berichten    => BtnBerichten,
                ActivePage.Aanmaken     => BtnAanmaken,
                ActivePage.Profiel      => BtnProfiel,
                ActivePage.Admin        => BtnAdmin,
                _                       => null
            };
            if (actief != null)
                actief.Style = (Style)Resources["NavBtnActive"];
        }

        public void Refresh()
        {
            var user = AppState.Instance.HuidigeGebruiker;
            if (user == null) return;

            // Naam tonen
            GebruikerNaamLabel.Text = string.IsNullOrWhiteSpace(user.Username)
                ? user.VolledigeNaam : $"@{user.Username}";
            GebruikerRolLabel.Text = user.Role == "Admin" ? "Admin" : "";

            // Admin-knop tonen/verbergen
            BtnAdmin.Visibility = user.Role == "Admin"
                ? Visibility.Visible : Visibility.Collapsed;

            // Profielfoto
            var bmp = ImageHelper.LaadGeschaald(user.ProfielFotoPad, 72);
            if (bmp != null)
            {
                AvatarImage.Source = bmp;
                AvatarAchtergrond.Visibility = Visibility.Collapsed;
            }
            else
            {
                AvatarImage.Source = null;
                AvatarAchtergrond.Visibility = Visibility.Visible;
            }
        }

        // ── Navigatie ────────────────────────────────────────────────────────
        private void OnHome(object sender, RoutedEventArgs e)         => Nav().AuthFrame.Navigate(new Feed());
        private void OnActiviteiten(object sender, RoutedEventArgs e) => Nav().AuthFrame.Navigate(new ActivityPage());
        private void OnBerichten(object sender, RoutedEventArgs e)    => Nav().AuthFrame.Navigate(new MessagePage());
        private void OnAanmaken(object sender, RoutedEventArgs e)     => Nav().AuthFrame.Navigate(new AanmakenKeuze());
        private void OnProfiel(object sender, RoutedEventArgs e)      => Nav().AuthFrame.Navigate(new ProfilePage());
        private void OnAdmin(object sender, RoutedEventArgs e)        => Nav().AuthFrame.Navigate(new AdminPage());

        private void OnUitloggen(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Wil je uitloggen?", "Uitloggen",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                AppState.Instance.HuidigeGebruiker = null;
                Nav().NavigateToLogin();
            }
        }

        private MoreConnector Nav() => (MoreConnector)Window.GetWindow(this);
    }
}
