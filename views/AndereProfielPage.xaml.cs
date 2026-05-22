using MoreConnector.Database;
using MoreConnector.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoreConnector.Views
{
    public partial class AndereProfielPage : Page
    {
        private readonly User _gebruiker;
        private readonly AppState _state = AppState.Instance;

        public AndereProfielPage(User gebruiker)
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Geen);
            _gebruiker = gebruiker;
            LaadProfiel();
            LaadActiviteiten();
            LaadConnecties();
        }

        private void LaadProfiel()
        {
            NaamTekst.Text     = string.IsNullOrWhiteSpace(_gebruiker.Username) ? _gebruiker.VolledigeNaam : _gebruiker.Username;
            UsernameTekst.Text = _gebruiker.VolledigeNaam;
            StudieTekst.Text   = _gebruiker.Study;
            BioTekst.Text      = _gebruiker.Bio;
            ProfielInitiaal.Text = _gebruiker.Firstname.Length > 0
                ? _gebruiker.Firstname[0].ToString().ToUpper() : "?";

            var bmp = ImageHelper.LaadGeschaald(_gebruiker.ProfielFotoPad, 200);
            if (bmp != null)
            {
                ProfielAvatarImg.Source = bmp;
                ProfielInitiaal.Visibility = Visibility.Collapsed;
            }

            // Vriendschapsstatus
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            if (eigenId == 0 || eigenId == _gebruiker.Id)
            {
                VriendenBtn.Visibility = Visibility.Collapsed;
                // BerichtBtn verwijderd
                return;
            }

            string status = "none";
            try { status = FriendshipRepository.GetStatus(eigenId, _gebruiker.Id); }
            catch { }

            switch (status)
            {
                case "accepted":
                    VriendenBtn.Content   = "✓ Vrienden";
                    VriendenBtn.IsEnabled = false;
                    break;
                case "pending":
                    VriendenBtn.Content   = "⏳ Verzonden";
                    VriendenBtn.IsEnabled = false;
                    break;
            }
        }

        private void LaadActiviteiten()
        {
            if (ActiviteitenPanel == null) return;
            ActiviteitenPanel.Children.Clear();

            bool gevonden = false;

            // Gebruik een WrapPanel zodat kaarten naast elkaar komen
            var wrapPanel = new System.Windows.Controls.WrapPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };

            foreach (var ev in _state.Evenementen)
            {
                if (ev.CreatorId != _gebruiker.Id &&
                    ev.Auteur != _gebruiker.VolledigeNaam &&
                    ev.Auteur != _gebruiker.Username) continue;
                gevonden = true;

                var kaart = new Border
                {
                    Width           = 200,
                    Height          = 220,
                    CornerRadius    = new CornerRadius(12),
                    Margin          = new Thickness(0, 0, 12, 12),
                    ClipToBounds    = true,
                    BorderBrush     = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                    BorderThickness = new Thickness(1)
                };

                var grid = new System.Windows.Controls.Grid();
                grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
                grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });

                // Foto of placeholder
                var bmp = ImageHelper.LaadGeschaald(ev.AfbeeldingPad, 400);
                if (bmp != null)
                {
                    var img = new System.Windows.Controls.Image { Source = bmp, Stretch = Stretch.UniformToFill };
                    System.Windows.Controls.Grid.SetRow(img, 0);
                    grid.Children.Add(img);
                }
                else
                {
                    var placeholder = new Border { Background = new SolidColorBrush(Color.FromRgb(20, 36, 52)) };
                    placeholder.Child = new TextBlock { Text = "🎉", FontSize = 36, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
                    System.Windows.Controls.Grid.SetRow(placeholder, 0);
                    grid.Children.Add(placeholder);
                }

                // Info balk
                var infoBalk = new Border { Background = new SolidColorBrush(Color.FromRgb(255, 140, 0)), Padding = new Thickness(10, 7, 10, 7) };
                var sp = new StackPanel();
                sp.Children.Add(new TextBlock { Text = ev.Naam, Foreground = new SolidColorBrush(Colors.White), FontSize = 12, FontWeight = FontWeights.SemiBold, TextWrapping = System.Windows.TextWrapping.Wrap });
                sp.Children.Add(new TextBlock { Text = $"{ev.Locatie}  ·  {ev.DatumTekst}", Foreground = new SolidColorBrush(Colors.White), FontSize = 10, TextWrapping = System.Windows.TextWrapping.Wrap });
                infoBalk.Child = sp;
                System.Windows.Controls.Grid.SetRow(infoBalk, 1);
                grid.Children.Add(infoBalk);

                kaart.Child = grid;
                wrapPanel.Children.Add(kaart);
            }

            if (!gevonden)
            {
                ActiviteitenPanel.Children.Add(new TextBlock { Text = "Geen activiteiten.", Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)), FontSize = 13 });
            }
            else
            {
                ActiviteitenPanel.Children.Add(wrapPanel);
            }
        }

        private void LaadConnecties()
        {
            if (ConnectiesPanel == null) return;
            ConnectiesPanel.Children.Clear();

            try
            {
                var vrienden = FriendshipRepository.GetVrienden(_gebruiker.Id);
                foreach (var v in vrienden)
                {
                    var kaart = new Border
                    {
                        Background      = new SolidColorBrush(Color.FromRgb(30, 46, 64)),
                        CornerRadius    = new CornerRadius(10),
                        Padding         = new Thickness(12, 10, 12, 10),
                        Margin          = new Thickness(0, 0, 0, 8),
                        BorderBrush     = new SolidColorBrush(Color.FromRgb(50, 70, 90)),
                        BorderThickness = new Thickness(1)
                    };
                    var row = new StackPanel { Orientation = Orientation.Horizontal };

                    // Profielfoto
                    var ell = new Ellipse { Width = 40, Height = 40, Margin = new Thickness(0, 0, 12, 0) };
                    var bmpV = ImageHelper.LaadGeschaald(v.ProfielFotoPad, 80);
                    ell.Fill = bmpV != null
                        ? (System.Windows.Media.Brush)new ImageBrush { ImageSource = bmpV, Stretch = Stretch.UniformToFill }
                        : new SolidColorBrush(Color.FromRgb(255, 140, 0));
                    row.Children.Add(ell);

                    var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                    info.Children.Add(new TextBlock
                    {
                        Text = v.VolledigeNaam,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 13, FontWeight = FontWeights.SemiBold
                    });
                    if (!string.IsNullOrWhiteSpace(v.Username))
                        info.Children.Add(new TextBlock
                        {
                            Text = $"@{v.Username}",
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                            FontSize = 11
                        });
                    row.Children.Add(info);
                    kaart.Child = row;
                    ConnectiesPanel.Children.Add(kaart);
                }

                if (vrienden.Count == 0)
                    ConnectiesPanel.Children.Add(new TextBlock
                    {
                        Text = "Geen connecties.",
                        Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)),
                        FontSize = 13
                    });
            }
            catch
            {
                ConnectiesPanel.Children.Add(new TextBlock
                {
                    Text = "Connecties niet beschikbaar.",
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)),
                    FontSize = 13
                });
            }
        }

        private void OnVriendenBtnClick(object sender, RoutedEventArgs e)
        {
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            try { FriendshipRepository.StuurVerzoek(eigenId, _gebruiker.Id); }
            catch { }
            VriendenBtn.Content   = "⏳ Verzonden";
            VriendenBtn.IsEnabled = false;
        }

        private void OnBerichtSturen(object sender, RoutedEventArgs e)
        {
            var msgPage = new MessagePage(_gebruiker);
            Nav().AuthFrame.Navigate(msgPage);
        }

        private void OnTerugClick(object sender, RoutedEventArgs e)
            => Nav().AuthFrame.Navigate(new GebruikersPage());

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
        private MoreConnector Nav() => (MoreConnector)Window.GetWindow(this);
    }
}
