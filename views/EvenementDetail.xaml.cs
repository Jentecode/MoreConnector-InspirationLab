using MoreConnector.Database;
using MoreConnector.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoreConnector.Views
{
    public partial class EvenementDetail : Page
    {
        private readonly AdminEvenement _ev;
        private readonly AppState _state = AppState.Instance;

        public EvenementDetail(AdminEvenement evenement)
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Activiteiten);
            _ev = evenement;
            LaadData();
            LaadDeelnemers();
        }

        // ── Data ────────────────────────────────────────────────────────────
        private void LaadData()
        {
            NaamTekst.Text        = _ev.Naam;
            DatumTekst.Text       = _ev.DatumTekst;
            LocatieTekst.Text     = _ev.Locatie;
            AuteurTekst.Text      = _ev.Auteur;
            BeschrijvingTekst.Text = string.IsNullOrWhiteSpace(_ev.Beschrijving)
                ? "Geen beschrijving opgegeven."
                : _ev.Beschrijving;

            // Hoge resolutie afbeelding laden
            var bmp = ImageHelper.LaadVolledig(_ev.AfbeeldingPad);
            if (bmp != null)
                EvenementAfbeelding.Source = bmp;
            else
                AfbeeldingBorder.Background = new SolidColorBrush(Color.FromRgb(30, 46, 64));

            // Al ingeschreven?
            string huidige = _state.HuidigeGebruiker?.DisplayNaam ?? "";
            if (_ev.Deelnemers.Contains(huidige))
            {
                InschrijvenButton.Content   = "✓ Ingeschreven";
                InschrijvenButton.IsEnabled = false;
            }
        }

        // ── Deelnemers ──────────────────────────────────────────────────────
        private void LaadDeelnemers()
        {
            DeelnemersPanel.Children.Clear();

            int aantal = _ev.Deelnemers.Count;
            DeelnemersTeller.Text      = aantal == 0 ? "Nog geen deelnemers"
                                       : aantal == 1 ? "1 deelnemer"
                                       : $"{aantal} deelnemers";
            GeenDeelnemersTekst.Visibility = aantal == 0 ? Visibility.Visible : Visibility.Collapsed;

            foreach (var naam in _ev.Deelnemers)
                DeelnemersPanel.Children.Add(BouwDeelnemerChip(naam));
        }

        private Border BouwDeelnemerChip(string naam)
        {
            var chip = new Border
            {
                Background   = new SolidColorBrush(Color.FromRgb(27, 42, 59)),
                CornerRadius  = new CornerRadius(20),
                Padding      = new Thickness(8, 6, 14, 6),
                Margin       = new Thickness(0, 0, 10, 10)
            };
            var row = new StackPanel { Orientation = Orientation.Horizontal };

            // Avatar
            var ellipse = new Ellipse
            {
                Width  = 32, Height = 32,
                Margin = new Thickness(0, 0, 8, 0)
            };

            // Zoek profielfoto van deelnemer
            string fotoPad = "";
            foreach (var g in _state.Gebruikers)
                if (g.DisplayNaam == naam || g.Username == naam.TrimStart('@'))
                { fotoPad = g.ProfielFotoPad; break; }

            var bmp = ImageHelper.LaadGeschaald(fotoPad, 64);
            if (bmp != null)
            {
                ellipse.Fill = new ImageBrush
                {
                    ImageSource = bmp,
                    Stretch     = Stretch.UniformToFill
                };
            }
            else
            {
                ellipse.Fill = new SolidColorBrush(Color.FromRgb(255, 140, 0));
                // Initiaal tonen
                row.Children.Add(ellipse);
                // Voeg letter toe over ellipse via Grid
                var grid = new System.Windows.Controls.Grid();
                grid.Children.Add(ellipse);
                grid.Children.Add(new TextBlock
                {
                    Text                = naam.Length > 0 ? naam[0].ToString().ToUpper() : "?",
                    Foreground          = new SolidColorBrush(Colors.White),
                    FontSize            = 13, FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center
                });
                chip.Child = new StackPanel { Orientation = Orientation.Horizontal, Children = { grid, new TextBlock { Text = naam, Foreground = new SolidColorBrush(Colors.White), FontSize = 13, VerticalAlignment = VerticalAlignment.Center } } };
                return chip;
            }

            row.Children.Add(ellipse);
            row.Children.Add(new TextBlock
            {
                Text              = naam,
                Foreground        = new SolidColorBrush(Colors.White),
                FontSize          = 13,
                VerticalAlignment = VerticalAlignment.Center
            });
            chip.Child = row;
            return chip;
        }

        // ── Inschrijven ─────────────────────────────────────────────────────
        private void OnInschrijvenClick(object sender, RoutedEventArgs e)
        {
            var user = _state.HuidigeGebruiker;
            string naam = user?.DisplayNaam ?? "Onbekend";

            // Schrijf in DB in
            if (user != null && user.Id > 0)
            {
                try { EventRepository.Inschrijven(_ev.Id, user.Id); }
                catch { /* DB niet beschikbaar */ }
            }

            if (!_ev.Deelnemers.Contains(naam))
                _ev.Deelnemers.Add(naam);

            InschrijvenButton.Content   = "✓ Ingeschreven";
            InschrijvenButton.IsEnabled = false;
            LaadDeelnemers();
        }

        // ── Nav ─────────────────────────────────────────────────────────────
        private void OnTerugClick(object sender, RoutedEventArgs e)        => Nav().AuthFrame.Navigate(new ActivityPage());
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
}
