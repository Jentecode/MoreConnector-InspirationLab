using MoreConnector.Database;
using MoreConnector.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MoreConnector.Views
{
    public partial class ActivityPage : Page
    {
        private readonly AppState _state = AppState.Instance;

        public ActivityPage()
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Activiteiten);
            SearchBox.Text = "zoek activiteiten...";

            // FIX: Goedemiddag naam instellen
            var user = _state.HuidigeGebruiker;
            if (user != null && UsernameText != null)
                UsernameText.Text = string.IsNullOrWhiteSpace(user.Username) ? user.Voornaam : user.Username;

            _state.Evenementen.CollectionChanged += (_, _) => HerlaadActiviteiten();
            HerlaadActiviteiten();
        }

        private void HerlaadActiviteiten()
        {
            PostPanel.Children.Clear();

            string zoek = SearchBox?.Text?.ToLower() ?? "";
            bool zoekActief = !string.IsNullOrWhiteSpace(zoek) && zoek != "zoek activiteiten...";

            string huidigeAuteur = _state.HuidigeGebruiker?.DisplayNaam ?? "";

            foreach (var ev in _state.Evenementen)
            {
                if (zoekActief &&
                    !ev.Naam.ToLower().Contains(zoek) &&
                    !ev.Locatie.ToLower().Contains(zoek) &&
                    !ev.Beschrijving.ToLower().Contains(zoek))
                    continue;

                // FIX: check via CreatorId (betrouwbaarder dan naam, werkt na logout/login)
                int huidigeId = _state.HuidigeGebruiker?.Id ?? 0;
                bool kanBeheren = (huidigeId > 0 && ev.CreatorId == huidigeId) ||
                                  ev.Auteur == huidigeAuteur || _state.IsAdmin;
                PostPanel.Children.Add(BouwEvenementKaart(ev, kanBeheren));
            }

            if (PostPanel.Children.Count == 0)
            {
                PostPanel.Children.Add(new TextBlock
                {
                    Text = "Geen activiteiten gevonden.",
                    Foreground = new SolidColorBrush(Colors.Gray),
                    FontSize = 14,
                    Margin = new Thickness(8)
                });
            }
        }

        private Border BouwEvenementKaart(AdminEvenement ev, bool kanBeheren)
        {
            var kaart = new Border
            {
                Background   = new SolidColorBrush(Color.FromRgb(30, 46, 64)),
                CornerRadius  = new CornerRadius(12),
                Margin       = new Thickness(8),
                MinHeight    = 200,
                MaxWidth     = 320,
                Cursor       = System.Windows.Input.Cursors.Hand
            };

            kaart.MouseLeftButtonUp += (_, _) =>
                ((MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new EvenementDetail(ev));

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(160) }); // vaste hoogte voor afbeelding
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // ── Afbeelding of placeholder ─────────────────────────────────
            var imgBorder = new Border
            {
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                ClipToBounds = true
            };

            if (!string.IsNullOrEmpty(ev.AfbeeldingPad))
            {
                var bmp = ImageHelper.LaadGeschaald(ev.AfbeeldingPad, 640);
                if (bmp != null)
                {
                    var img = new System.Windows.Controls.Image
                    {
                        Source  = bmp,
                        Stretch = Stretch.UniformToFill
                    };
                    System.Windows.Media.RenderOptions.SetBitmapScalingMode(img, System.Windows.Media.BitmapScalingMode.HighQuality);
                    imgBorder.Child = img;
                }
                else
                {
                    imgBorder.Background = new SolidColorBrush(Color.FromRgb(45, 62, 80));
                    imgBorder.Child = new TextBlock { Text = "🎉", FontSize = 36,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)) };
                }
            }
            else
            {
                imgBorder.Background = new SolidColorBrush(Color.FromRgb(45, 62, 80));
                imgBorder.Child = new TextBlock { Text = "🎉", FontSize = 36,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)) };
            }

            Grid.SetRow(imgBorder, 0);
            grid.Children.Add(imgBorder);

            // ── Beheer knoppen (bewerken + verwijderen) ────────────────────
            if (kanBeheren)
            {
                var btnPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 8, 8, 0)
                };

                var editBtn = new Button
                {
                    Content = "✏", Background = new SolidColorBrush(Color.FromArgb(180, 30, 100, 200)),
                    Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0),
                    Width = 28, Height = 28, FontSize = 12,
                    Cursor = System.Windows.Input.Cursors.Hand, Margin = new Thickness(0, 0, 4, 0),
                    ToolTip = "Activiteit bewerken"
                };
                editBtn.Click += (_, args) =>
                {
                    args.Handled = true;
                    Nav().AuthFrame.Navigate(new BewerkActiviteitPage(ev));
                };
                btnPanel.Children.Add(editBtn);

                var del = new Button
                {
                    Content = "✕", Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                    Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0),
                    Width = 28, Height = 28, FontSize = 12,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    ToolTip = "Activiteit verwijderen"
                };
                del.Click += (_, args) =>
                {
                    args.Handled = true;
                    var r = MessageBox.Show($"Activiteit '{ev.Naam}' verwijderen?", "Bevestig",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r == MessageBoxResult.Yes)
                    {
                        try { EventRepository.Verwijder(ev.Id); } catch { }
                        _state.Evenementen.Remove(ev);
                    }
                };
                btnPanel.Children.Add(del);

                Grid.SetRow(btnPanel, 0);
                grid.Children.Add(btnPanel);
            }

            // ── Info balk ─────────────────────────────────────────────────
            var infoBalk = new Border
            {
                Background   = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                CornerRadius  = new CornerRadius(0, 0, 12, 12),
                Padding      = new Thickness(12, 8, 12, 8)
            };
            var infoStack = new StackPanel();
            infoStack.Children.Add(new TextBlock
            {
                Text = ev.Naam, Foreground = new SolidColorBrush(Colors.White),
                FontSize = 13, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap
            });
            infoStack.Children.Add(new TextBlock
            {
                Text = $"{ev.Locatie}  ·  {ev.DatumTekst}", Foreground = new SolidColorBrush(Colors.White),
                FontSize = 12, TextWrapping = TextWrapping.Wrap
            });
            if (ev.MaxDeelnemers > 0)
                infoStack.Children.Add(new TextBlock
                {
                    Text = $"Max. {ev.MaxDeelnemers} deelnemers",
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 220, 180)),
                    FontSize = 11
                });
            infoBalk.Child = infoStack;
            Grid.SetRow(infoBalk, 1);
            grid.Children.Add(infoBalk);

            kaart.Child = grid;
            return kaart;
        }

        private void OnBekijkAllesClick(object sender, RoutedEventArgs e) { }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "zoek activiteiten...") SearchBox.Text = "";
        }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) SearchBox.Text = "zoek activiteiten...";
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => HerlaadActiviteiten();

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
}
