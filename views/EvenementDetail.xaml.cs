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

        private void LaadData()
        {
            NaamTekst.Text         = _ev.Naam;
            DatumTekst.Text        = _ev.DatumTekst;
            LocatieTekst.Text      = _ev.Locatie;
            AuteurTekst.Text       = _ev.Auteur;
            BeschrijvingTekst.Text = string.IsNullOrWhiteSpace(_ev.Beschrijving)
                ? "Geen beschrijving opgegeven." : _ev.Beschrijving;

            var bmp = ImageHelper.LaadVolledig(_ev.AfbeeldingPad);
            if (bmp != null)
                EvenementAfbeelding.Source = bmp;
            else
                AfbeeldingBorder.Background = new SolidColorBrush(Color.FromRgb(30, 46, 64));

            var huidigUser = _state.HuidigeGebruiker;
            if (huidigUser != null && huidigUser.Id > 0)
            {
                try
                {
                    var freshEv = EventRepository.GetById(_ev.Id, huidigUser.Id);
                    if (freshEv?.JoinedByMe == true)
                    {
                        InschrijvenButton.Content    = "✗ Uitschrijven";
                        InschrijvenButton.Background = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(150, 40, 40));
                        InschrijvenButton.IsEnabled  = true;
                        InschrijvenButton.Tag        = "ingeschreven";
                    }
                    if (freshEv?.ParticipantNames != null)
                    {
                        _ev.Deelnemers.Clear();
                        foreach (var n in freshEv.ParticipantNames) _ev.Deelnemers.Add(n);
                    }
                    if (_ev.MaxDeelnemers > 0 && _ev.Deelnemers.Count >= _ev.MaxDeelnemers)
                    {
                        InschrijvenButton.Content   = "Vol";
                        InschrijvenButton.IsEnabled = false;
                    }
                }
                catch { }
            }
        }

        private void LaadDeelnemers()
        {
            DeelnemersPanel.Children.Clear();
            int aantal = _ev.Deelnemers.Count;
            DeelnemersTeller.Text = aantal == 0 ? "Nog geen deelnemers"
                                  : aantal == 1 ? "1 deelnemer"
                                  : $"{aantal} deelnemers";

            if (MaxDeelnemersLabel != null)
            {
                MaxDeelnemersLabel.Text = _ev.MaxDeelnemers > 0 ? $"Max. {_ev.MaxDeelnemers} deelnemers" : "";
                MaxDeelnemersLabel.Visibility = _ev.MaxDeelnemers > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

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

            string fotoPad = "";
            foreach (var g in _state.Gebruikers)
                if (g.DisplayNaam == naam || g.VolledigeNaam == naam || g.Username == naam.TrimStart('@'))
                { fotoPad = g.ProfielFotoPad; break; }

            var avatarGrid = Models.AvatarHelper.Bouw(fotoPad, naam, 32);
            avatarGrid.Margin = new Thickness(0, 0, 8, 0);

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(avatarGrid);
            row.Children.Add(new TextBlock
            {
                Text = naam, Foreground = new SolidColorBrush(Colors.White),
                FontSize = 13, VerticalAlignment = VerticalAlignment.Center
            });

            chip.Child = row;
            return chip;
        }

        private void OnInschrijvenClick(object sender, RoutedEventArgs e)
        {
            var user = _state.HuidigeGebruiker;
            if (user == null) return;
            string naam = user.DisplayNaam;

            bool isIngeschreven = InschrijvenButton.Tag?.ToString() == "ingeschreven";

            if (isIngeschreven)
            {
                // Uitschrijven
                if (user.Id > 0)
                    try { EventRepository.Uitschrijven(_ev.Id, user.Id); } catch { }

                _ev.Deelnemers.Remove(naam);
                InschrijvenButton.Content    = "Inschrijven";
                InschrijvenButton.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 140, 0));
                InschrijvenButton.Tag        = null;
            }
            else
            {
                // Inschrijven
                if (user.Id > 0)
                {
                    try
                    {
                        bool gelukt = EventRepository.Inschrijven(_ev.Id, user.Id);
                        if (!gelukt)
                        {
                            MessageBox.Show("Het maximum aantal deelnemers is bereikt.",
                                "Vol", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                    catch { }
                }

                if (!_ev.Deelnemers.Contains(naam)) _ev.Deelnemers.Add(naam);
                InschrijvenButton.Content    = "✗ Uitschrijven";
                InschrijvenButton.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(150, 40, 40));
                InschrijvenButton.Tag        = "ingeschreven";
            }

            LaadDeelnemers();
        }

        private void OnTerugClick(object sender, RoutedEventArgs e)         => Nav().AuthFrame.Navigate(new ActivityPage());
        private void OnHomeClick(object sender, RoutedEventArgs e)          => Nav().AuthFrame.Navigate(new Feed());
        private void OnBerichtenClick(object sender, RoutedEventArgs e)     => Nav().AuthFrame.Navigate(new MessagePage());
        private void OnAanmakenClick(object sender, RoutedEventArgs e)      => Nav().AuthFrame.Navigate(new AanmakenKeuze());
        private void OnProfielClick(object sender, RoutedEventArgs e)       => Nav().AuthFrame.Navigate(new ProfilePage());
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
            if (r == MessageBoxResult.Yes) { AppState.Instance.HuidigeGebruiker = null; Nav().NavigateToLogin(); }
        }
        private void OnProfielAvatarClick(object sender, RoutedEventArgs e) => Nav().AuthFrame.Navigate(new ProfilePage());
        private MoreConnector Nav() => (MoreConnector)Window.GetWindow(this);
    }
}
