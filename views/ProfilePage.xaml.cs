using System.Windows.Input;
using MoreConnector.Database;
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
            LaadConnecties();
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
                MijnActiviteiten.Add(new ProfielActiviteit { Titel = $"{ev.Naam} – {ev.DatumTekst}", Locatie = ev.Locatie, FotoPad = ev.AfbeeldingPad });
        }

        private void LaadConnecties()
        {
            // Bouw connecties dynamisch (met foto + unfriend knop)
            ConnectiesPanel.ItemsSource = null;
            ConnectiesBouwPanel.Children.Clear();
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            if (eigenId == 0) return;
            try
            {
                var vrienden = FriendshipRepository.GetVrienden(eigenId);
                foreach (var v in vrienden)
                {
                    var kaart = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(30, 46, 64)),
                        CornerRadius = new CornerRadius(10), Padding = new Thickness(12, 10, 12, 10),
                        Margin = new Thickness(0, 0, 0, 8)
                    };
                    var grid = new System.Windows.Controls.Grid();
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = System.Windows.GridLength.Auto });
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = System.Windows.GridLength.Auto });

                    var ell = new System.Windows.Shapes.Ellipse { Width = 40, Height = 40, Margin = new Thickness(0, 0, 12, 0) };
                    var bmpV = ImageHelper.LaadGeschaald(v.ProfielFotoPad, 80);
                    ell.Fill = bmpV != null
                        ? (System.Windows.Media.Brush)new ImageBrush { ImageSource = bmpV, Stretch = Stretch.UniformToFill }
                        : new SolidColorBrush(Color.FromRgb(255, 140, 0));
                    System.Windows.Controls.Grid.SetColumn(ell, 0);
                    grid.Children.Add(ell);

                    var naam = new TextBlock
                    {
                        Text = string.IsNullOrWhiteSpace(v.Username) ? v.VolledigeNaam : v.Username,
                        Foreground = new SolidColorBrush(Colors.White), FontSize = 14,
                        FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center
                    };
                    System.Windows.Controls.Grid.SetColumn(naam, 1);
                    grid.Children.Add(naam);

                    var user = v; // capture voor closure
                    var unfriendBtn = new Button
                    {
                        Content = "Ontvriend", Background = new SolidColorBrush(Color.FromRgb(100, 30, 30)),
                        Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0),
                        Padding = new Thickness(10, 4, 10, 4), Cursor = Cursors.Hand, FontSize = 12
                    };
                    unfriendBtn.Click += (_, _) =>
                    {
                        var r2 = MessageBox.Show($"Vriend {user.VolledigeNaam} verwijderen?", "Ontvrienden",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (r2 != MessageBoxResult.Yes) return;
                        try { FriendshipRepository.VerwijderVriendschap(eigenId, user.Id); } catch { }
                        ConnectiesBouwPanel.Children.Remove(kaart);
                    };
                    System.Windows.Controls.Grid.SetColumn(unfriendBtn, 2);
                    grid.Children.Add(unfriendBtn);

                    kaart.Child = grid;
                    ConnectiesBouwPanel.Children.Add(kaart);
                }
            }
            catch { }
        }

        private void OnProfielBewerkenClick(object sender, RoutedEventArgs e)
            => Nav().AuthFrame.Navigate(new ProfileEditPage());

        private void OnHomeClick(object sender, RoutedEventArgs e)         => Nav().AuthFrame.Navigate(new Feed());

        private void OnBerichtenClick(object sender, RoutedEventArgs e)    => Nav().AuthFrame.Navigate(new MessagePage());
        private void OnAanmakenClick(object sender, RoutedEventArgs e)     => Nav().AuthFrame.Navigate(new AanmakenKeuze());
        private void OnProfielClick(object sender, RoutedEventArgs e)      => Nav().AuthFrame.Navigate(new ProfilePage());

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
        public string FotoPad { get; set; } = "";
        public Visibility InfoVisibility => string.IsNullOrWhiteSpace(Titel) ? Visibility.Collapsed : Visibility.Visible;
        public System.Windows.Media.ImageSource? FotoBron =>
            string.IsNullOrWhiteSpace(FotoPad) ? null : ImageHelper.LaadGeschaald(FotoPad, 400);
        public Visibility FotoVisibility => string.IsNullOrWhiteSpace(FotoPad) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility PlaceholderVisibility => string.IsNullOrWhiteSpace(FotoPad) ? Visibility.Visible : Visibility.Collapsed;
    }

    public class Connectie
    {
        public string Naam   { get; set; } = "";
        public string Status { get; set; } = "Vriend";
    }
}
