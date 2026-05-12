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
    public partial class NotificatiePage : Page
    {
        private readonly AppState _state = AppState.Instance;

        public NotificatiePage()
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Geen);
            LaadVerzoeken();
        }

        private void LaadVerzoeken()
        {
            VerzoekPanel.Children.Clear();
            int userId = _state.HuidigeGebruiker?.Id ?? 0;
            if (userId == 0) { GeenVerzoekTekst.Visibility = Visibility.Visible; return; }

            List<FriendRequest> verzoeken = new();
            try { verzoeken = FriendshipRepository.GetBinnenkomendeVerzoeken(userId); }
            catch { }

            if (verzoeken.Count == 0)
            { GeenVerzoekTekst.Visibility = Visibility.Visible; return; }

            GeenVerzoekTekst.Visibility = Visibility.Collapsed;

            foreach (var verzoek in verzoeken)
                VerzoekPanel.Children.Add(BouwVerzoekKaart(verzoek));
        }

        private Border BouwVerzoekKaart(FriendRequest verzoek)
        {
            var kaart = new Border
            {
                Background  = new SolidColorBrush(Color.FromRgb(30, 46, 64)),
                CornerRadius = new CornerRadius(12),
                Padding     = new Thickness(20),
                Margin      = new Thickness(0, 0, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Avatar
            var ellipse = new Ellipse
            {
                Width  = 48, Height = 48,
                Fill   = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                Margin = new Thickness(0, 0, 16, 0)
            };
            Grid.SetColumn(ellipse, 0);
            grid.Children.Add(ellipse);

            // Info
            var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            info.Children.Add(new TextBlock
            {
                Text       = verzoek.Sender.VolledigeNaam,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize   = 15, FontWeight = FontWeights.SemiBold
            });
            info.Children.Add(new TextBlock
            {
                Text       = $"@{verzoek.Sender.Username}",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                FontSize   = 13
            });
            info.Children.Add(new TextBlock
            {
                Text       = $"Wil je vriend zijn · {verzoek.CreatedAt:d MMM yyyy}",
                Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)),
                FontSize   = 12
            });
            Grid.SetColumn(info, 1);
            grid.Children.Add(info);

            // Knoppen
            var knoppen = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            var accepteer = MaakKnop("✓  Accepteer", Color.FromRgb(34, 139, 34));
            accepteer.Click += (_, _) =>
            {
                try { FriendshipRepository.AccepteerVerzoek(verzoek.Id); }
                catch { }
                VerzoekPanel.Children.Remove(kaart);
                if (VerzoekPanel.Children.Count == 0)
                    GeenVerzoekTekst.Visibility = Visibility.Visible;
                MessageBox.Show($"Je bent nu bevriend met {verzoek.Sender.VolledigeNaam}!", "Vrienden");
            };

            var weiger = MaakKnop("✕  Weiger", Color.FromRgb(192, 57, 43));
            weiger.Margin = new Thickness(8, 0, 0, 0);
            weiger.Click += (_, _) =>
            {
                try { FriendshipRepository.WeigerVerzoek(verzoek.Id); }
                catch { }
                VerzoekPanel.Children.Remove(kaart);
                if (VerzoekPanel.Children.Count == 0)
                    GeenVerzoekTekst.Visibility = Visibility.Visible;
            };

            knoppen.Children.Add(accepteer);
            knoppen.Children.Add(weiger);
            Grid.SetColumn(knoppen, 2);
            grid.Children.Add(knoppen);

            kaart.Child = grid;
            return kaart;
        }

        private Button MaakKnop(string tekst, Color bg)
        {
            var btn = new Button
            {
                Content         = tekst,
                Background      = new SolidColorBrush(bg),
                Foreground      = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Padding         = new Thickness(14, 8, 14, 8),
                FontSize        = 13, Cursor = Cursors.Hand
            };
            var tpl = new ControlTemplate(typeof(Button));
            var bdr = new FrameworkElementFactory(typeof(Border));
            bdr.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            bdr.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            bdr.SetBinding(Border.PaddingProperty, new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            bdr.AppendChild(cp);
            tpl.VisualTree = bdr;
            btn.Template = tpl;
            return btn;
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
        private MoreConnector Nav() => (MoreConnector)Window.GetWindow(this);
    }
}
