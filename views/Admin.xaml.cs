using MoreConnector.Database;
using MoreConnector.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MoreConnector.Views
{
    public partial class AdminPage : Page
    {
        private readonly AppState _state = AppState.Instance;
        private readonly ObservableCollection<AdminPost>      _gefilterdePosts      = new();
        private readonly ObservableCollection<AdminEvenement> _gefilterdeEvents     = new();
        private readonly ObservableCollection<User> _gefilterdeGebruikers = new();
        private readonly ObservableCollection<AdminComment> _gefilterdeComments = new();

        public AdminPage()
        {
            InitializeComponent();

            PostsList.ItemsSource  = _gefilterdePosts;
            EventsList.ItemsSource = _gefilterdeEvents;
            UsersList.ItemsSource  = _gefilterdeGebruikers;
            CommentsList.ItemsSource = _gefilterdeComments;

            _state.Berichten.CollectionChanged  += (_, _) => HerlaadPosts();
            _state.Evenementen.CollectionChanged += (_, _) => HerlaadEvents();
            _state.Gebruikers.CollectionChanged  += (_, _) => HerlaadGebruikers();

            HerlaadPosts();
            HerlaadEvents();
            HerlaadGebruikers();
            HerlaadComments();
            RefreshLeegStaten();
            SetActiveTab(TabPostsBtn);
        }

        // ── Tab active state ─────────────────────────────────────────────────
        private void SetActiveTab(Button actief)
        {
            var normaal = (Style)Resources["TabButtonStyle"];
            var active  = (Style)Resources["TabButtonActiveStyle"];
            TabPostsBtn.Style    = normaal;
            TabEventsBtn.Style   = normaal;
            TabUsersBtn.Style    = normaal;
            if (TabCommentsBtn != null) TabCommentsBtn.Style = normaal;
            actief.Style = active;
        }

        private void OnTabPosts(object sender, RoutedEventArgs e)
        {
            PostsPanel.Visibility  = Visibility.Visible;
            EventsPanel.Visibility = Visibility.Collapsed;
            UsersPanel.Visibility  = Visibility.Collapsed;
            SetActiveTab(TabPostsBtn);
        }

        private void OnTabEvents(object sender, RoutedEventArgs e)
        {
            PostsPanel.Visibility  = Visibility.Collapsed;
            EventsPanel.Visibility = Visibility.Visible;
            UsersPanel.Visibility  = Visibility.Collapsed;
            SetActiveTab(TabEventsBtn);
        }

        private void OnTabComments(object sender, RoutedEventArgs e)
        {
            PostsPanel.Visibility    = Visibility.Collapsed;
            EventsPanel.Visibility   = Visibility.Collapsed;
            UsersPanel.Visibility    = Visibility.Collapsed;
            CommentsPanel.Visibility = Visibility.Visible;
            SetActiveTab(TabCommentsBtn);
        }

        private void OnTabUsers(object sender, RoutedEventArgs e)
        {
            PostsPanel.Visibility  = Visibility.Collapsed;
            EventsPanel.Visibility = Visibility.Collapsed;
            UsersPanel.Visibility  = Visibility.Visible;
            SetActiveTab(TabUsersBtn);
        }

        // ── Herlaad ──────────────────────────────────────────────────────────
        private void HerlaadPosts()
        {
            string zoek = PostsSearchBox?.Text?.ToLower() ?? "";
            _gefilterdePosts.Clear();
            foreach (var p in _state.Berichten.Where(p =>
                (p.Auteur ?? "").ToLower().Contains(zoek) ||
                (p.Beschrijving ?? "").ToLower().Contains(zoek)))
                _gefilterdePosts.Add(p);
            RefreshLeegStaten();
        }

        private void HerlaadEvents()
        {
            string zoek = EventsSearchBox?.Text?.ToLower() ?? "";
            _gefilterdeEvents.Clear();
            foreach (var ev in _state.Evenementen.Where(ev =>
                (ev.Naam ?? "").ToLower().Contains(zoek) ||
                (ev.Locatie ?? "").ToLower().Contains(zoek)))
                _gefilterdeEvents.Add(ev);
            RefreshLeegStaten();
        }

        private void HerlaadGebruikers()
        {
            string zoek = UsersSearchBox?.Text?.ToLower() ?? "";
            _gefilterdeGebruikers.Clear();
            foreach (var u in _state.Gebruikers.Where(u =>
                (u.VolledigeNaam ?? "").ToLower().Contains(zoek) ||
                (u.Username ?? "").ToLower().Contains(zoek)))
                _gefilterdeGebruikers.Add(u);
            RefreshLeegStaten();
        }

        // ── Zoeken ───────────────────────────────────────────────────────────
        private void PostsSearchBox_TextChanged(object sender, TextChangedEventArgs e)  => HerlaadPosts();
        private void EventsSearchBox_TextChanged(object sender, TextChangedEventArgs e) => HerlaadEvents();
        private void UsersSearchBox_TextChanged(object sender, TextChangedEventArgs e)  => HerlaadGebruikers();

        // ── Verwijderen ──────────────────────────────────────────────────────
        private void OnPostVerwijderenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not AdminPost post) return;
            _state.Berichten.Remove(post);
            var fp = _state.FeedPosts.FirstOrDefault(f =>
                f.Beschrijving == post.Beschrijving && f.AuteurNaam == post.Auteur);
            if (fp != null) _state.FeedPosts.Remove(fp);
        }

        private void OnEventVerwijderenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not AdminEvenement ev) return;
            _state.Evenementen.Remove(ev);
        }

        private void OnGebruikerVerwijderenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not User user) return;
            _state.Gebruikers.Remove(user);
        }

        // ── Ban / Unban ──────────────────────────────────────────────────────
        private void OnGebruikerVerbannenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not User user) return;
            user.IsBanned = true;
        }

        private void OnGebruikerDeblokkeerenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not User user) return;
            user.IsBanned = false;
        }

        // ── Wachtwoord wijzigen ───────────────────────────────────────────────
        private void OnWachtwoordWijzigenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not User user) return;

            var win = new Window
            {
                Title = $"Wachtwoord — {user.DisplayNaam}",
                Width = 380, Height = 230,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(27, 42, 59))
            };

            var sp = new StackPanel { Margin = new Thickness(24) };
            sp.Children.Add(new TextBlock
            {
                Text = $"Nieuw wachtwoord voor {user.DisplayNaam}",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 14, FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12)
            });

            var pw1 = new PasswordBox { Padding = new Thickness(10, 8, 10, 8), FontSize = 14, Margin = new Thickness(0, 0, 0, 8) };
            sp.Children.Add(new TextBlock { Text = "Nieuw wachtwoord", Foreground = System.Windows.Media.Brushes.Gray, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) });
            sp.Children.Add(pw1);

            var pw2 = new PasswordBox { Padding = new Thickness(10, 8, 10, 8), FontSize = 14, Margin = new Thickness(0, 0, 0, 12) };
            sp.Children.Add(new TextBlock { Text = "Bevestig wachtwoord", Foreground = System.Windows.Media.Brushes.Gray, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) });
            sp.Children.Add(pw2);

            var fout = new TextBlock { Foreground = System.Windows.Media.Brushes.Tomato, FontSize = 12, Visibility = Visibility.Collapsed };
            sp.Children.Add(fout);

            var opslaanBtn = new Button
            {
                Content = "Opslaan", Padding = new Thickness(16, 8, 16, 8),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 140, 0)),
                Foreground = System.Windows.Media.Brushes.White, BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand, Margin = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            opslaanBtn.Click += (_, _) =>
            {
                if (pw1.Password.Length < 6) { fout.Text = "Min. 6 tekens."; fout.Visibility = Visibility.Visible; return; }
                if (pw1.Password != pw2.Password) { fout.Text = "Wachtwoorden komen niet overeen."; fout.Visibility = Visibility.Visible; return; }
                user.WachtwoordHash = pw1.Password;
                try { UserRepository.UpdateWachtwoord(user.Id, pw1.Password); }
                catch { /* DB niet beschikbaar */ }
                win.Close();
                MessageBox.Show($"Wachtwoord van {user.DisplayNaam} bijgewerkt.", "Opgeslagen");
            };
            sp.Children.Add(opslaanBtn);
            win.Content = sp;
            win.ShowDialog();
        }

        // ── Navigatie ─────────────────────────────────────────────────────────
        private void OnNaarAppClick(object sender, RoutedEventArgs e)
            => ((MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new Feed());

        private void OnUitloggenClick(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Wil je uitloggen?", "Uitloggen",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                AppState.Instance.HuidigeGebruiker = null;
                ((MoreConnector)Window.GetWindow(this)).NavigateToLogin();
            }
        }

        // ── Lege staten ──────────────────────────────────────────────────────
        private void HerlaadComments()
        {
            string zoek = CommentsSearchBox?.Text?.ToLower() ?? "";
            _gefilterdeComments.Clear();
            try
            {
                // Haal alle comments op via alle posts
                var posts = PostRepository.GetAll();
                foreach (var p in posts)
                {
                    var comments = CommentRepository.GetVanPost(p.Id);
                    foreach (var cm in comments)
                    {
                        if (!string.IsNullOrEmpty(zoek) &&
                            !cm.AuthorName.ToLower().Contains(zoek) &&
                            !cm.Content.ToLower().Contains(zoek)) continue;
                        _gefilterdeComments.Add(new AdminComment
                        {
                            Id              = cm.Id,
                            PostId          = cm.PostId,
                            AuthorName      = cm.AuthorName,
                            Content         = cm.Content,
                            CreatedAtTekst  = cm.CreatedAt.ToString("d MMM yyyy HH:mm")
                        });
                    }
                }
            }
            catch { }

            if (CommentsLeegTekst != null)
                CommentsLeegTekst.Visibility = _gefilterdeComments.Count == 0
                    ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CommentsSearchBox_TextChanged(object sender, TextChangedEventArgs e) => HerlaadComments();

        private void OnCommentVerwijderenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not AdminComment comment) return;
            try { CommentRepository.Verwijder(comment.Id); }
            catch { }
            _gefilterdeComments.Remove(comment);
        }

        private void RefreshLeegStaten()
        {
            if (PostsLeegTekst    != null) PostsLeegTekst.Visibility    = _gefilterdePosts.Count     == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (EventsLeegTekst  != null) EventsLeegTekst.Visibility  = _gefilterdeEvents.Count    == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (UsersLeegTekst   != null) UsersLeegTekst.Visibility   = _gefilterdeGebruikers.Count== 0 ? Visibility.Visible : Visibility.Collapsed;
            if (CommentsLeegTekst!= null) CommentsLeegTekst.Visibility = _gefilterdeComments.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class AdminComment
    {
        public int    Id             { get; set; }
        public int    PostId         { get; set; }
        public string AuthorName     { get; set; } = "";
        public string Content        { get; set; } = "";
        public string CreatedAtTekst { get; set; } = "";
    }
}
