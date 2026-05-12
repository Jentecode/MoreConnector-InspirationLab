using MoreConnector.Database;
using MoreConnector.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoreConnector.Views
{
    public partial class Feed : Page
    {
        private readonly AppState _state = AppState.Instance;

        public Feed()
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Home);
            SearchBox.Text = "Zoeken...";

            var user = _state.HuidigeGebruiker;
            if (user != null)
                UsernameText.Text = string.IsNullOrWhiteSpace(user.Username) ? user.Voornaam : user.Username;

            LaadFeed();
            _state.FeedPosts.CollectionChanged += (_, _) => LaadFeed();
        }

        // ── Feed laden ───────────────────────────────────────────────────────
        private void LaadFeed(string zoek = "")
        {
            FeedPanel.Children.Clear();

            var posts = string.IsNullOrWhiteSpace(zoek) || zoek == "Zoeken..."
                ? _state.FeedPosts
                : new ObservableCollection<FeedPost>(
                    _state.FeedPosts.Where(p =>
                        p.AuteurNaam.ToLower().Contains(zoek.ToLower()) ||
                        p.Beschrijving.ToLower().Contains(zoek.ToLower())));

            foreach (var post in posts)
                FeedPanel.Children.Add(BouwPostKaart(post));
        }

        // ── Post kaart ───────────────────────────────────────────────────────
        private Border BouwPostKaart(FeedPost post)
        {
            var kaart = new Border
            {
                Background  = new SolidColorBrush(Color.FromRgb(30, 46, 64)),
                CornerRadius = new CornerRadius(12),
                Padding     = new Thickness(16),
                Margin      = new Thickness(0, 0, 0, 20)
            };

            var rootGrid = new Grid();
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(420) });
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // ── LINKS: auteur, afbeelding, caption, likes ──────────────────
            var links = new StackPanel();
            Grid.SetColumn(links, 0);

            // Auteur rij
            var auteurRij = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
            var avatarGrid = new Grid { Width = 36, Height = 36, Margin = new Thickness(0, 0, 10, 0) };
            var avatarEllipse = new Ellipse { Width = 36, Height = 36, Fill = new SolidColorBrush(Color.FromRgb(217, 217, 217)) };

            // Probeer profielfoto laden
            string auteurFoto = "";
            foreach (var g in _state.Gebruikers)
                if (g.DisplayNaam == post.AuteurNaam || g.Username == post.AuteurNaam.TrimStart('@'))
                { auteurFoto = g.ProfielFotoPad; break; }

            var bmpAuteur = ImageHelper.LaadGeschaald(auteurFoto, 72);
            if (bmpAuteur != null)
                avatarEllipse.Fill = new ImageBrush { ImageSource = bmpAuteur, Stretch = Stretch.UniformToFill };

            avatarGrid.Children.Add(avatarEllipse);
            auteurRij.Children.Add(avatarGrid);
            auteurRij.Children.Add(new TextBlock
            {
                Text = post.AuteurNaam, Foreground = new SolidColorBrush(Colors.White),
                FontSize = 15, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center
            });
            links.Children.Add(auteurRij);

            // Afbeelding — hoge resolutie
            var afbBorder = new Border
            {
                Background  = new SolidColorBrush(Color.FromRgb(45, 62, 80)),
                CornerRadius = new CornerRadius(8),
                Height      = 300,
                ClipToBounds = true,
                Margin      = new Thickness(0, 0, 0, 12)
            };
            var bmpPost = ImageHelper.LaadGeschaald(post.AfbeeldingPad, 840);
            if (bmpPost != null)
            {
                var imgPost = new System.Windows.Controls.Image
                {
                    Source  = bmpPost,
                    Stretch = Stretch.UniformToFill
                };
                System.Windows.Media.RenderOptions.SetBitmapScalingMode(
                    imgPost, System.Windows.Media.BitmapScalingMode.HighQuality);
                afbBorder.Child = imgPost;
            }
            else
            {
                afbBorder.Child = new TextBlock
                {
                    Text = "📷", FontSize = 36,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                    Foreground          = new SolidColorBrush(Color.FromRgb(136, 136, 136))
                };
            }
            links.Children.Add(afbBorder);

            // Caption
            var captBorder = new Border
            {
                Background  = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(8),
                Padding     = new Thickness(12, 10, 12, 10),
                Margin      = new Thickness(0, 0, 0, 10)
            };
            captBorder.Child = new TextBlock
            {
                Text = post.Beschrijving, Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                FontSize = 13, TextWrapping = TextWrapping.Wrap
            };
            links.Children.Add(captBorder);

            // Post likes balk
            links.Children.Add(BouwLikesBalk(post));
            rootGrid.Children.Add(links);

            // ── RECHTS: reacties ───────────────────────────────────────────
            var rechts = new StackPanel { Margin = new Thickness(24, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };
            Grid.SetColumn(rechts, 1);

            var reactiesPanel = new StackPanel();
            // Laad reacties uit DB als post een DB-id heeft
            if (post.DbId > 0)
            {
                try
                {
                    var dbComments = CommentRepository.GetVanPost(post.DbId, _state.HuidigeGebruiker?.Id ?? 0);
                    foreach (var dbC in dbComments)
                        reactiesPanel.Children.Add(BouwReactieBlok(new FeedReactie
                        {
                            AuteurNaam = dbC.AuthorName, Tekst = dbC.Content,
                            DbCommentId = dbC.Id
                        }));
                }
                catch { /* DB niet beschikbaar — toon lokale reacties */ }
            }
            foreach (var r in post.Reacties)
                reactiesPanel.Children.Add(BouwReactieBlok(r));
            rechts.Children.Add(reactiesPanel);
            rechts.Children.Add(BouwReactieInvoer(post, reactiesPanel));
            rootGrid.Children.Add(rechts);

            kaart.Child = rootGrid;
            return kaart;
        }

        // ── Post likes ───────────────────────────────────────────────────────
        private StackPanel BouwLikesBalk(FeedPost post)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };

            var likeBtn = new Button
            {
                Background      = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor          = Cursors.Hand,
                Padding         = new Thickness(0)
            };

            var likeTekst = new TextBlock
            {
                FontSize          = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            void Refresh()
            {
                likeTekst.Text       = post.LikedByMe
                    ? $"❤️  {post.LikeCount}"
                    : $"🤍  {post.LikeCount}";
                likeTekst.Foreground = post.LikedByMe
                    ? new SolidColorBrush(Color.FromRgb(255, 80, 80))
                    : new SolidColorBrush(Colors.White);
            }

            Refresh();
            likeBtn.Content = likeTekst;

            likeBtn.Click += (_, _) =>
            {
                var user = _state.HuidigeGebruiker;
                if (user != null && user.Id > 0 && post.DbId > 0)
                {
                    try
                    {
                        bool nowLiked = PostRepository.ToggleLike(post.DbId, user.Id);
                        post.LikedByMe = nowLiked;
                        post.LikeCount = PostRepository.GetLikeCount(post.DbId);
                    }
                    catch
                    {
                        // DB niet beschikbaar — lokaal toggling
                        string naam = user.DisplayNaam ?? "";
                        if (post.LikedDoor.Contains(naam)) { post.LikedDoor.Remove(naam); post.LikedByMe = false; }
                        else { post.LikedDoor.Add(naam); post.LikedByMe = true; }
                        post.LikeCount = post.LikedDoor.Count;
                    }
                }
                Refresh();
            };

            row.Children.Add(likeBtn);
            return row;
        }

        // ── Reactie blok ─────────────────────────────────────────────────────
        private StackPanel BouwReactieBlok(FeedReactie reactie)
        {
            var stack = new StackPanel { Margin = new Thickness(reactie.IsReply ? 24 : 0, 0, 0, 10) };

            // Header: avatar + naam
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };

            var ell = new Ellipse { Width = 30, Height = 30, Margin = new Thickness(0, 0, 8, 0) };
            var bmpR = ImageHelper.LaadGeschaald(reactie.AuteurFotoPad, 60);
            ell.Fill = bmpR != null
                ? new ImageBrush { ImageSource = bmpR, Stretch = Stretch.UniformToFill }
                : new SolidColorBrush(Color.FromRgb(217, 217, 217));

            header.Children.Add(ell);
            header.Children.Add(new TextBlock
            {
                Text = reactie.AuteurNaam, Foreground = new SolidColorBrush(Colors.White),
                FontSize = 13, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center
            });
            stack.Children.Add(header);

            // Tekst
            stack.Children.Add(new TextBlock
            {
                Text = reactie.Tekst, Foreground = new SolidColorBrush(Color.FromRgb(200, 210, 220)),
                FontSize = 13, Margin = new Thickness(38, 0, 0, 4), TextWrapping = TextWrapping.Wrap
            });

            // Reactie likes
            stack.Children.Add(BouwReactieLikes(reactie));
            return stack;
        }

        // ── Reactie likes ─────────────────────────────────────────────────────
        private StackPanel BouwReactieLikes(FeedReactie reactie)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(38, 0, 0, 0) };

            var btn = new Button
            {
                Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand, Padding = new Thickness(0)
            };
            var txt = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };

            void Refresh()
            {
                bool liked = reactie.LikedDoor.Contains(_state.HuidigeGebruiker?.DisplayNaam ?? "");
                txt.Text       = liked ? $"❤️ {reactie.LikedDoor.Count}" : $"🤍 {reactie.LikedDoor.Count}";
                txt.Foreground = liked
                    ? new SolidColorBrush(Color.FromRgb(255, 80, 80))
                    : new SolidColorBrush(Color.FromRgb(150, 160, 170));
            }

            Refresh();
            btn.Content = txt;
            btn.Click += (_, _) =>
            {
                string naam = _state.HuidigeGebruiker?.DisplayNaam ?? "";
                if (reactie.LikedDoor.Contains(naam)) reactie.LikedDoor.Remove(naam);
                else reactie.LikedDoor.Add(naam);
                Refresh();
            };

            row.Children.Add(btn);
            return row;
        }

        // ── Reactie invoer ────────────────────────────────────────────────────
        private Border BouwReactieInvoer(FeedPost post, StackPanel reactiesPanel)
        {
            var border = new Border
            {
                Background  = new SolidColorBrush(Color.FromRgb(13, 27, 42)),
                CornerRadius = new CornerRadius(20),
                Padding     = new Thickness(14, 8, 8, 8),
                Margin      = new Thickness(0, 8, 0, 0)
            };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textBox = new TextBox
            {
                Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Colors.White), FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center, Text = "Schrijf een reactie..."
            };
            textBox.GotFocus  += (_, _) => { if (textBox.Text == "Schrijf een reactie...") textBox.Text = ""; };
            textBox.LostFocus += (_, _) => { if (string.IsNullOrWhiteSpace(textBox.Text)) textBox.Text = "Schrijf een reactie..."; };
            Grid.SetColumn(textBox, 0);
            grid.Children.Add(textBox);

            var btn = new Button
            {
                Content = "→", Background = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0),
                Width = 32, Height = 32, FontSize = 16, Cursor = Cursors.Hand
            };
            btn.Click += (_, _) =>
            {
                string tekst = textBox.Text.Trim();
                if (string.IsNullOrEmpty(tekst) || tekst == "Schrijf een reactie...") return;

                var user    = _state.HuidigeGebruiker;
                string naam = user?.DisplayNaam ?? "Onbekend";
                string foto = user?.ProfielFotoPad ?? "";

                // Sla reactie op in DB
                int commentId = 0;
                if (user != null && user.Id > 0 && post.DbId > 0)
                {
                    try { commentId = CommentRepository.Toevoegen(post.DbId, user.Id, tekst); }
                    catch { /* DB niet beschikbaar */ }
                }

                var r = new FeedReactie { DbCommentId = commentId, AuteurNaam = naam, Tekst = tekst, AuteurFotoPad = foto };
                post.Reacties.Add(r);
                reactiesPanel.Children.Add(BouwReactieBlok(r));
                textBox.Text = "Schrijf een reactie...";
            };
            Grid.SetColumn(btn, 1);
            grid.Children.Add(btn);
            border.Child = grid;
            return border;
        }

        // ── Zoeken ────────────────────────────────────────────────────────────
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string zoek = SearchBox.Text;
            if (zoek == "Zoeken...") return;
            LaadFeed(zoek);
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Zoeken...") SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) { SearchBox.Text = "Zoeken..."; LaadFeed(); }
        }

        // ── Nav ───────────────────────────────────────────────────────────────

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

    // ── Data modellen ─────────────────────────────────────────────────────────



    public class Activiteit
    {
        public int    Id        { get; set; }
        public string Titel     { get; set; } = "";
        public string Locatie   { get; set; } = "";
        public bool   CanDelete { get; set; }
        public Visibility CanDeleteVisibility => CanDelete ? Visibility.Visible : Visibility.Collapsed;
        public Visibility InfoVisibility      => string.IsNullOrWhiteSpace(Titel) ? Visibility.Collapsed : Visibility.Visible;
    }
