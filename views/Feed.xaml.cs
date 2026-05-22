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

            var user = _state.HuidigeGebruiker;
            if (user != null)
                UsernameText.Text = string.IsNullOrWhiteSpace(user.Username) ? user.Voornaam : user.Username;

            LaadTopbarAvatar();
            LaadFeed();
            _state.FeedPosts.CollectionChanged += (_, _) => LaadFeed();
        }

        private void LaadTopbarAvatar()
        {
            var user = _state.HuidigeGebruiker;
            if (user == null) return;
            var bmp = ImageHelper.LaadGeschaald(user.ProfielFotoPad, 96);
            if (bmp == null) return;
            TopbarAvatarBtn.Loaded += (_, _) => PasTopbarAvatarToe(bmp);
            if (TopbarAvatarBtn.IsLoaded) PasTopbarAvatarToe(bmp);
        }

        private void PasTopbarAvatarToe(System.Windows.Media.Imaging.BitmapSource bmp)
        {
            var img = FindVisualChild<System.Windows.Controls.Image>(TopbarAvatarBtn);
            if (img != null) img.Source = bmp;
            var ell = FindVisualChild<System.Windows.Shapes.Ellipse>(TopbarAvatarBtn);
            if (ell != null) ell.Visibility = Visibility.Collapsed;
        }

        private static T? FindVisualChild<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void LaadFeed()
        {
            FeedPanel.Children.Clear();
            foreach (var post in _state.FeedPosts)
                FeedPanel.Children.Add(BouwPostKaart(post));
        }

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

            var links = new StackPanel();
            Grid.SetColumn(links, 0);

            // Auteur rij
            var auteurRij = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            // Profielfoto auteur: match op UserId
            var eigenUser = _state.HuidigeGebruiker;
            string auteurFoto = "";
            foreach (var g in _state.Gebruikers)
                if (g.Id == post.UserId) { auteurFoto = g.ProfielFotoPad; break; }
            if (string.IsNullOrEmpty(auteurFoto) && eigenUser?.Id == post.UserId)
                auteurFoto = eigenUser.ProfielFotoPad;

            var avatarGrid = Models.AvatarHelper.Bouw(auteurFoto, post.AuteurNaam, 36);
            avatarGrid.Margin = new Thickness(0, 0, 10, 0);
            auteurRij.Children.Add(avatarGrid);
            auteurRij.Children.Add(new TextBlock
            {
                Text = post.AuteurNaam, Foreground = new SolidColorBrush(Colors.White),
                FontSize = 15, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center
            });

            if (!string.IsNullOrWhiteSpace(post.DatumTekst))
                auteurRij.Children.Add(new TextBlock
                {
                    Text = $"  ·  {post.DatumTekst}",
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)),
                    FontSize = 12, VerticalAlignment = VerticalAlignment.Center
                });

            bool isEigenPost = eigenUser != null && eigenUser.Id == post.UserId;
            if (isEigenPost || _state.IsAdmin)
            {
                var verwijderBtn = new Button
                {
                    Content = "🗑", Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand, FontSize = 14, Margin = new Thickness(8, 0, 0, 0),
                    ToolTip = "Post verwijderen", VerticalAlignment = VerticalAlignment.Center
                };
                verwijderBtn.Click += (_, _) =>
                {
                    if (MessageBox.Show("Post verwijderen?", "Bevestig", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (post.DbId > 0) try { PostRepository.Verwijder(post.DbId); } catch { }
                        _state.FeedPosts.Remove(post);
                        var bp = _state.Berichten.FirstOrDefault(b => b.Id == post.DbId);
                        if (bp != null) _state.Berichten.Remove(bp);
                    }
                };
                auteurRij.Children.Add(verwijderBtn);
            }
            links.Children.Add(auteurRij);

            // Afbeelding
            var afbBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 62, 80)),
                CornerRadius = new CornerRadius(8), Height = 300, ClipToBounds = true, Margin = new Thickness(0, 0, 0, 12)
            };
            var bmpPost = ImageHelper.LaadGeschaald(post.AfbeeldingPad, 840);
            if (bmpPost != null)
            {
                var imgPost = new System.Windows.Controls.Image { Source = bmpPost, Stretch = Stretch.UniformToFill };
                System.Windows.Media.RenderOptions.SetBitmapScalingMode(imgPost, System.Windows.Media.BitmapScalingMode.HighQuality);
                afbBorder.Child = imgPost;
            }
            else
                afbBorder.Child = new TextBlock { Text = "📷", FontSize = 36,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)) };
            links.Children.Add(afbBorder);

            var captBorder = new Border
            {
                Background = new SolidColorBrush(Colors.White), CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10), Margin = new Thickness(0, 0, 0, 10)
            };
            captBorder.Child = new TextBlock
            {
                Text = post.Beschrijving, Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                FontSize = 13, TextWrapping = TextWrapping.Wrap
            };
            links.Children.Add(captBorder);
            links.Children.Add(BouwLikesBalk(post));
            rootGrid.Children.Add(links);
            var rechts = new StackPanel { Margin = new Thickness(24, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };
            Grid.SetColumn(rechts, 1);

            var reactiesPanel = new StackPanel();

            // FIX DUBBELE COMMENTS: laad enkel uit DB, negeer post.Reacties
            // post.Reacties wordt niet meer gebruikt voor weergave
            if (post.DbId > 0)
            {
                try
                {
                    var dbComments = CommentRepository.GetVanPost(post.DbId, _state.HuidigeGebruiker?.Id ?? 0)
                                                      .OrderByDescending(c => c.CreatedAt).ToList();
                    foreach (var dbC in dbComments)
                        reactiesPanel.Children.Add(BouwReactieBlok(new FeedReactie
                        {
                            AuteurNaam  = dbC.AuthorName, Tekst = dbC.Content,
                            DbCommentId = dbC.Id, DatumTekst = dbC.CreatedAt.ToString("d MMM yyyy HH:mm")
                        }));
                }
                catch { /* DB niet beschikbaar */ }
            }

            // Scrollviewer rond reacties (max 400px hoogte)
            var scrollViewer = new ScrollViewer
            {
                MaxHeight = 400,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = reactiesPanel
            };
            rechts.Children.Add(scrollViewer);
            rechts.Children.Add(BouwReactieInvoer(post, reactiesPanel));
            rootGrid.Children.Add(rechts);

            kaart.Child = rootGrid;
            return kaart;
        }

        private StackPanel BouwLikesBalk(FeedPost post)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            var likeBtn = new Button { Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand, Padding = new Thickness(0) };
            var likeTekst = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };

            void Refresh()
            {
                likeTekst.Text = post.LikedByMe ? $"❤️  {post.LikeCount}" : $"🤍  {post.LikeCount}";
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
                    try { post.LikedByMe = PostRepository.ToggleLike(post.DbId, user.Id); post.LikeCount = PostRepository.GetLikeCount(post.DbId); }
                    catch
                    {
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

        private StackPanel BouwReactieBlok(FeedReactie reactie)
        {
            var stack = new StackPanel { Margin = new Thickness(reactie.IsReply ? 24 : 0, 0, 0, 10) };
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 2) };

            string commentFoto = "";
            var eu = _state.HuidigeGebruiker;
            foreach (var g in _state.Gebruikers)
                if (g.VolledigeNaam == reactie.AuteurNaam || g.DisplayNaam == reactie.AuteurNaam || g.Username == reactie.AuteurNaam)
                { commentFoto = g.ProfielFotoPad; break; }
            if (string.IsNullOrEmpty(commentFoto) && !string.IsNullOrEmpty(reactie.AuteurFotoPad))
                commentFoto = reactie.AuteurFotoPad;
            if (string.IsNullOrEmpty(commentFoto) && eu != null &&
                (eu.VolledigeNaam == reactie.AuteurNaam || eu.DisplayNaam == reactie.AuteurNaam))
                commentFoto = eu.ProfielFotoPad;

            var ell = Models.AvatarHelper.Bouw(commentFoto, reactie.AuteurNaam, 30);
            ell.Margin = new Thickness(0, 0, 8, 0);
            header.Children.Add(ell);
            header.Children.Add(new TextBlock
            {
                Text = reactie.AuteurNaam, Foreground = new SolidColorBrush(Colors.White),
                FontSize = 13, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center
            });
            if (!string.IsNullOrWhiteSpace(reactie.DatumTekst))
                header.Children.Add(new TextBlock
                {
                    Text = $"  ·  {reactie.DatumTekst}",
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)),
                    FontSize = 11, VerticalAlignment = VerticalAlignment.Center
                });
            stack.Children.Add(header);
            stack.Children.Add(new TextBlock
            {
                Text = reactie.Tekst, Foreground = new SolidColorBrush(Color.FromRgb(200, 210, 220)),
                FontSize = 13, Margin = new Thickness(38, 0, 0, 4), TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(BouwReactieLikes(reactie));
            return stack;
        }

        private StackPanel BouwReactieLikes(FeedReactie reactie)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(38, 0, 0, 0) };
            var btn = new Button { Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand, Padding = new Thickness(0) };
            var txt = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };

            int userId = _state.HuidigeGebruiker?.Id ?? 0;

            // Laad initiële staat uit DB
            bool isLiked = false;
            int likeCount = 0;
            if (reactie.DbCommentId > 0 && userId > 0)
            {
                try
                {
                    isLiked   = CommentRepository.IsLikedByUser(reactie.DbCommentId, userId);
                    likeCount = CommentRepository.GetLikeCount(reactie.DbCommentId);
                }
                catch { }
            }

            void Refresh(bool liked, int count)
            {
                txt.Text = liked ? $"❤️ {count}" : $"🤍 {count}";
                txt.Foreground = liked
                    ? new SolidColorBrush(Color.FromRgb(255, 80, 80))
                    : new SolidColorBrush(Color.FromRgb(150, 160, 170));
            }

            Refresh(isLiked, likeCount);
            btn.Content = txt;

            btn.Click += (_, _) =>
            {
                if (reactie.DbCommentId <= 0 || userId <= 0) return;
                bool nowLiked = CommentRepository.ToggleLike(reactie.DbCommentId, userId);
                int newCount  = CommentRepository.GetLikeCount(reactie.DbCommentId);
                Refresh(nowLiked, newCount);
            };

            row.Children.Add(btn);
            return row;
        }

        private Border BouwReactieInvoer(FeedPost post, StackPanel reactiesPanel)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(13, 27, 42)),
                CornerRadius = new CornerRadius(20), Padding = new Thickness(14, 8, 8, 8), Margin = new Thickness(0, 8, 0, 0)
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

                var contentFout = Models.UsernameValidator.ValideerContent(tekst);
                if (contentFout != null)
                {
                    System.Windows.MessageBox.Show(contentFout, "Ongepaste inhoud",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var user  = _state.HuidigeGebruiker;
                // Altijd username gebruiken, niet de echte naam
                string naam = (!string.IsNullOrWhiteSpace(user?.Username)) ? user!.Username : (user?.VolledigeNaam ?? "Onbekend");
                string foto = user?.ProfielFotoPad ?? "";
                var nu = System.DateTime.Now;

                int commentId = 0;
                if (user != null && user.Id > 0 && post.DbId > 0)
                    try { commentId = CommentRepository.Toevoegen(post.DbId, user.Id, tekst); } catch { }
                // Dit voorkomt dubbele weergave bij herladen
                var nieuwBlok = BouwReactieBlok(new FeedReactie
                {
                    DbCommentId = commentId, AuteurNaam = naam, Tekst = tekst,
                    AuteurFotoPad = foto, DatumTekst = nu.ToString("d MMM yyyy HH:mm")
                });
                reactiesPanel.Children.Insert(0, nieuwBlok);
                textBox.Text = "Schrijf een reactie...";
            };
            Grid.SetColumn(btn, 1);
            grid.Children.Add(btn);
            border.Child = grid;
            return border;
        }

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

public class Activiteit
{
    public int    Id        { get; set; }
    public string Titel     { get; set; } = "";
    public string Locatie   { get; set; } = "";
    public bool   CanDelete { get; set; }
    public System.Windows.Visibility CanDeleteVisibility => CanDelete ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    public System.Windows.Visibility InfoVisibility      => string.IsNullOrWhiteSpace(Titel) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
}
