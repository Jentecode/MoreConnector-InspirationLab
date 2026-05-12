using MoreConnector.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MoreConnector.Views
{
    public partial class ProfilePage : Page
    {
        private readonly AppState _state = AppState.Instance;
        public static ObservableCollection<ProfielActiviteit> MijnActiviteiten { get; } = new();
        public static ObservableCollection<Connectie>         MijnConnecties   { get; } = new();

        public ProfilePage()
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Profiel);
            MijnActiviteitenPanel.ItemsSource = MijnActiviteiten;
            ConnectiesPanel.ItemsSource       = MijnConnecties;

            LaadProfielData();
            LaadMijnActiviteiten();
            LaadInteresses();
        }

        private void LaadProfielData()
        {
            var user = _state.HuidigeGebruiker;
            if (user == null) return;

            NaamTekst.Text     = user.VolledigeNaam;
            UsernameLabel.Text = string.IsNullOrWhiteSpace(user.Username) ? "" : $"@{user.Username}";
            BioTekst.Text      = user.Bio;
            if (RichtingText != null) RichtingText.Text = user.Studierichting;

            // Profielfoto hoge resolutie
            var bmp = ImageHelper.LaadGeschaald(user.ProfielFotoPad, 200);
            if (bmp != null)
            {
                ProfielFotoImage.Source = bmp;
                ProfielFotoAchtergrond.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProfielFotoImage.Source = null;
                ProfielFotoAchtergrond.Visibility = Visibility.Visible;
            }
        }

        private void LaadMijnActiviteiten()
        {
            string auteur = _state.HuidigeGebruiker?.DisplayNaam ?? "";
            MijnActiviteiten.Clear();
            foreach (var ev in _state.Evenementen.Where(e => e.Auteur == auteur))
                MijnActiviteiten.Add(new ProfielActiviteit { Titel = $"{ev.Naam} – {ev.DatumTekst}", Locatie = ev.Locatie });
        }

        private void LaadInteresses()
        {
            InteressesPanel.Children.Clear();
            var user = _state.HuidigeGebruiker;
            var tags = user?.Tags.Count > 0
                ? user.Tags
                : new System.Collections.Generic.List<string> { "Sport", "Muziek", "IT" };

            foreach (var tag in tags)
            {
                InteressesPanel.Children.Add(new Border
                {
                    Background  = new SolidColorBrush(Color.FromRgb(204, 82, 0)),
                    CornerRadius = new CornerRadius(12),
                    Padding     = new Thickness(14, 6, 14, 6),
                    Margin      = new Thickness(0, 0, 8, 8),
                    Child       = new TextBlock
                    {
                        Text       = $"#{tag}",
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize   = 13
                    }
                });
            }
        }

        private void OnProfielBewerkenClick(object sender, RoutedEventArgs e)
            => Nav().AuthFrame.Navigate(new ProfileEditPage());

        private void OnHomeClick(object sender, RoutedEventArgs e)         => Nav().AuthFrame.Navigate(new Feed());

        private void OnBerichtenClick(object sender, RoutedEventArgs e)    => Nav().AuthFrame.Navigate(new MessagePage());
        private void OnAanmakenClick(object sender, RoutedEventArgs e)     => Nav().AuthFrame.Navigate(new AanmakenKeuze());
        private void OnProfielClick(object sender, RoutedEventArgs e)      => Nav().AuthFrame.Navigate(new ProfilePage());


        // ── Sidebar nav handlers ─────────────────────────────────────────────

        private void SideNav_Gebruikers(object sender, RoutedEventArgs e)   => Nav().AuthFrame.Navigate(new GebruikersPage());
        private void SideNav_Notificaties(object sender, RoutedEventArgs e) => Nav().AuthFrame.Navigate(new NotificatiePage());
        private void SideNav_Home(object sender, RoutedEventArgs e)         => Nav().AuthFrame.Navigate(new Feed());
        private void SideNav_Activiteiten(object sender, RoutedEventArgs e) => Nav().AuthFrame.Navigate(new ActivityPage());
        private void SideNav_Berichten(object sender, RoutedEventArgs e)    => Nav().AuthFrame.Navigate(new MessagePage());
        private void SideNav_Aanmaken(object sender, RoutedEventArgs e)     => Nav().AuthFrame.Navigate(new AanmakenKeuze());
        private void SideNav_Profiel(object sender, RoutedEventArgs e)      => Nav().AuthFrame.Navigate(new ProfilePage());
        private void SideNav_Admin(object sender, RoutedEventArgs e)        => Nav().AuthFrame.Navigate(new AdminPage());
        private void SideNav_Uitloggen(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("Wil je uitloggen?", "Uitloggen", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes)
            {
                AppState.Instance.HuidigeGebruiker = null;
                Nav().NavigateToLogin();
            }
        }
        private void OnProfielAvatarClick(object sender, RoutedEventArgs e) => Nav().AuthFrame.Navigate(new ProfilePage());

        private MoreConnector Nav() => (MoreConnector)Window.GetWindow(this);
    }

    public class ProfielActiviteit
    {
        public string Titel   { get; set; } = "";
        public string Locatie { get; set; } = "";
        public Visibility InfoVisibility => string.IsNullOrWhiteSpace(Titel) ? Visibility.Collapsed : Visibility.Visible;
    }

    public class Connectie
    {
        public string Naam   { get; set; } = "";
        public string Status { get; set; } = "Vriend";
    }
}
