using System.Windows.Media.Imaging;
using MoreConnector.Database;
using MoreConnector.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoreConnector.Views
{
    public partial class GebruikersPage : Page
    {
        private readonly AppState _state = AppState.Instance;
        private List<User> _alleGebruikers = new();
        private readonly System.Collections.Generic.HashSet<int> _verzondenRequests = new();

        public GebruikersPage()
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Gebruikers);
            ZoekBox.Text = "Zoek op naam of gebruikersnaam...";
            ZoekBox.Foreground = new SolidColorBrush(Colors.Gray);
            LaadGebruikers();
        }

        private void LaadGebruikers(string zoek = "")
        {
            try
            {
                _alleGebruikers = UserRepository.GetAll();
            }
            catch { _alleGebruikers = new List<User>(_state.Gebruikers); }

            var gefilterd = string.IsNullOrWhiteSpace(zoek)
                ? _alleGebruikers
                : _alleGebruikers.Where(u =>
                    u.VolledigeNaam.ToLower().Contains(zoek.ToLower()) ||
                    u.Username.ToLower().Contains(zoek.ToLower())).ToList();

            // Verwijder ingelogde gebruiker uit lijst
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            gefilterd = gefilterd.Where(u => u.Id != eigenId).ToList();

            GebruikersPanel.Children.Clear();
            foreach (var user in gefilterd)
                GebruikersPanel.Children.Add(BouwGebruikerKaart(user));

            if (gefilterd.Count == 0)
                GebruikersPanel.Children.Add(new TextBlock
                {
                    Text = "Geen gebruikers gevonden.",
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)),
                    FontSize = 14, Margin = new Thickness(0, 16, 0, 0)
                });
        }

        private Border BouwGebruikerKaart(User user)
        {
            // Vriendschapsstatus ophalen
            string status = "none";
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            if (eigenId > 0 && user.Id > 0)
            {
                try { status = FriendshipRepository.GetStatus(eigenId, user.Id); }
                catch { }
            }

            var kaart = new Border
            {
                Background   = new SolidColorBrush(Color.FromRgb(30, 46, 64)),
                CornerRadius  = new CornerRadius(12),
                Padding      = new Thickness(20),
                Margin       = new Thickness(0, 0, 0, 12),
                Cursor       = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Avatar
            var avatarGrid = Models.AvatarHelper.Bouw(user.ProfielFotoPad, user.Firstname, 52);
            avatarGrid.Margin = new Thickness(0, 0, 16, 0);
            // dummy initiaal voor code below (nodig voor grid.Children.Add)
            var initiaal = new TextBlock
            {
                Text                = "",
                Foreground          = new SolidColorBrush(Colors.White),
                FontSize            = 22, FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center
            };
            avatarGrid.Children.Add(initiaal);
            Grid.SetColumn(avatarGrid, 0);
            grid.Children.Add(avatarGrid);

            // Info
            var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            info.Children.Add(new TextBlock
            {
                Text       = user.VolledigeNaam,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize   = 15, FontWeight = FontWeights.SemiBold
            });
            info.Children.Add(new TextBlock
            {
                Text       = $"@{user.Username}",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                FontSize   = 13
            });
            if (!string.IsNullOrWhiteSpace(user.Study))
                info.Children.Add(new TextBlock
                {
                    Text       = user.Study,
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)),
                    FontSize   = 12
                });
            Grid.SetColumn(info, 1);
            grid.Children.Add(info);

            // Actie knop
            var actieStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            // Bekijk profiel knop
            var bekijkBtn = MaakKnop("👁  Profiel", Color.FromRgb(30, 64, 100));
            bekijkBtn.Click += (_, _) => Nav().AuthFrame.Navigate(new AndereProfielPage(user));
            actieStack.Children.Add(bekijkBtn);

            // Vriendknop
            if (eigenId > 0)
            {
                if (status == "none" && _verzondenRequests.Contains(user.Id))
                    status = "pending";

                Button? vriendenBtn = null;
                switch (status)
                {
                    case "none":
                        vriendenBtn = MaakKnop("➕  Bevriend", Color.FromRgb(255, 140, 0));
                        vriendenBtn.Click += (_, _) =>
                        {
                            try { FriendshipRepository.StuurVerzoek(eigenId, user.Id); }
                            catch { }
                            _verzondenRequests.Add(user.Id);
                            vriendenBtn.Content = "✓ Verzonden";
                            vriendenBtn.IsEnabled = false;
                            vriendenBtn.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                        };
                        break;
                    case "pending":
                        vriendenBtn = MaakKnop("⏳ Verzonden", Color.FromRgb(80, 80, 80));
                        vriendenBtn.IsEnabled = false;
                        break;
                    case "accepted":
                        vriendenBtn = MaakKnop("✓ Vrienden", Color.FromRgb(34, 139, 34));
                        vriendenBtn.IsEnabled = false;
                        break;
                }
                if (vriendenBtn != null)
                {
                    vriendenBtn.Margin = new Thickness(8, 0, 0, 0);
                    actieStack.Children.Add(vriendenBtn);
                }
            }

            Grid.SetColumn(actieStack, 2);
            grid.Children.Add(actieStack);
            kaart.Child = grid;

            return kaart;
        }

        private Button MaakKnop(string tekst, Color achtergrond)
        {
            var btn = new Button
            {
                Content         = tekst,
                Background      = new SolidColorBrush(achtergrond),
                Foreground      = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Padding         = new Thickness(14, 8, 14, 8),
                FontSize        = 13,
                Cursor          = Cursors.Hand
            };
            btn.Template = MaakRondeTemplate();
            return btn;
        }

        private ControlTemplate MaakRondeTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            var factory  = new FrameworkElementFactory(typeof(Border));
            factory.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            factory.SetBinding(Border.PaddingProperty,
                new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            var content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AppendChild(content);
            template.VisualTree = factory;
            return template;
        }

        private void ZoekBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ZoekBox.Text == "Zoek op naam of gebruikersnaam...")
            { ZoekBox.Text = ""; ZoekBox.Foreground = new SolidColorBrush(Colors.Black); }
        }

        private void ZoekBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ZoekBox.Text))
            { ZoekBox.Text = "Zoek op naam of gebruikersnaam..."; ZoekBox.Foreground = new SolidColorBrush(Colors.Gray); LaadGebruikers(); }
        }

        private void ZoekBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string t = ZoekBox.Text;
            if (t == "Zoek op naam of gebruikersnaam...") return;
            LaadGebruikers(t);
        }

        private void SideNav_Home(object sender, RoutedEventArgs e)         => Nav().AuthFrame.Navigate(new Feed());
        private void SideNav_Activiteiten(object sender, RoutedEventArgs e) => Nav().AuthFrame.Navigate(new ActivityPage());
        private void SideNav_Berichten(object sender, RoutedEventArgs e)    => Nav().AuthFrame.Navigate(new MessagePage());
        private void SideNav_Gebruikers(object sender, RoutedEventArgs e)   => Nav().AuthFrame.Navigate(new GebruikersPage());
        private void SideNav_Notificaties(object sender, RoutedEventArgs e)  => Nav().AuthFrame.Navigate(new NotificatiePage());
        private void SideNav_Aanmaken(object sender, RoutedEventArgs e)     => Nav().AuthFrame.Navigate(new AanmakenKeuze());
        private void SideNav_Profiel(object sender, RoutedEventArgs e)      => Nav().AuthFrame.Navigate(new ProfilePage());
        private void SideNav_Admin(object sender, RoutedEventArgs e)        => Nav().AuthFrame.Navigate(new AdminPage());
        private void SideNav_Uitloggen(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("Wil je uitloggen?", "Uitloggen", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes) { AppState.Instance.HuidigeGebruiker = null; Nav().NavigateToLogin(); }
        }

        private MoreConnector Nav() => (MoreConnector)Window.GetWindow(this);
    }
}
