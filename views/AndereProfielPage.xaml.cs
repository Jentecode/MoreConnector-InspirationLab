using MoreConnector.Database;
using MoreConnector.Models;
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
            LaadPosts();
        }

        private void LaadProfiel()
        {
            NaamTekst.Text     = _gebruiker.VolledigeNaam;
            UsernameTekst.Text = $"@{_gebruiker.Username}";
            StudieTekst.Text   = _gebruiker.Study;
            BioTekst.Text      = _gebruiker.Bio;
            ProfielInitiaal.Text = _gebruiker.Firstname.Length > 0
                ? _gebruiker.Firstname[0].ToString().ToUpper() : "?";

            // Profielfoto
            var bmp = ImageHelper.LaadGeschaald(_gebruiker.ProfielFotoPad, 200);
            if (bmp != null)
            {
                ProfielAvatarImg.Source = bmp;
                ProfielInitiaal.Visibility = Visibility.Collapsed;
            }

            // Tags
            try
            {
                var interesses = UserRepository.GetInteresses(_gebruiker.Id);
                foreach (var i in interesses)
                {
                    TagsPanel.Children.Add(new Border
                    {
                        Background   = new SolidColorBrush(Color.FromRgb(204, 82, 0)),
                        CornerRadius  = new CornerRadius(12),
                        Padding      = new Thickness(12, 5, 12, 5),
                        Margin       = new Thickness(0, 0, 8, 8),
                        Child        = new TextBlock { Text = $"#{i.Name}", Foreground = new SolidColorBrush(Colors.White), FontSize = 12 }
                    });
                }
            }
            catch { }

            // Vriendschapsstatus
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            if (eigenId == 0 || eigenId == _gebruiker.Id)
            {
                VriendenBtn.Visibility = Visibility.Collapsed;
                BerichtBtn.Visibility  = Visibility.Collapsed;
                return;
            }

            string status = "none";
            try { status = FriendshipRepository.GetStatus(eigenId, _gebruiker.Id); }
            catch { }

            switch (status)
            {
                case "accepted":
                    VriendenBtn.Content    = "✓ Vrienden";
                    VriendenBtn.IsEnabled  = false;
                    break;
                case "pending":
                    VriendenBtn.Content    = "⏳ Verzonden";
                    VriendenBtn.IsEnabled  = false;
                    break;
            }
        }

        private void LaadPosts()
        {
            PostsPanel.Children.Clear();
            try
            {
                var posts = PostRepository.GetVanGebruiker(_gebruiker.Id);
                foreach (var p in posts)
                {
                    var kaart = new Border
                    {
                        Background  = new SolidColorBrush(Color.FromRgb(30, 46, 64)),
                        CornerRadius = new CornerRadius(12),
                        Padding     = new Thickness(20),
                        Margin      = new Thickness(0, 0, 0, 12)
                    };
                    var sp = new StackPanel();
                    sp.Children.Add(new TextBlock
                    {
                        Text = p.Content, Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 14, TextWrapping = TextWrapping.Wrap
                    });
                    sp.Children.Add(new TextBlock
                    {
                        Text = p.CreatedAt.ToString("d MMMM yyyy"),
                        Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)),
                        FontSize = 12, Margin = new Thickness(0, 6, 0, 0)
                    });
                    kaart.Child = sp;
                    PostsPanel.Children.Add(kaart);
                }
            }
            catch { }

            if (PostsPanel.Children.Count == 0)
                PostsPanel.Children.Add(new TextBlock
                {
                    Text = "Geen posts.",
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)),
                    FontSize = 14
                });
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
            => Nav().AuthFrame.Navigate(new MessagePage(_gebruiker));

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
