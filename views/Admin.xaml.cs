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
        private readonly ObservableCollection<User>           _gefilterdeGebruikers = new();
        private readonly ObservableCollection<AdminComment>   _gefilterdeComments   = new();

        public AdminPage()
        {
            InitializeComponent();

            PostsList.ItemsSource    = _gefilterdePosts;
            EventsList.ItemsSource   = _gefilterdeEvents;
            UsersList.ItemsSource    = _gefilterdeGebruikers;
            CommentsList.ItemsSource = _gefilterdeComments;

            _state.Berichten.CollectionChanged   += (_, _) => HerlaadPosts();
            _state.Evenementen.CollectionChanged += (_, _) => HerlaadEvents();
            _state.Gebruikers.CollectionChanged  += (_, _) => HerlaadGebruikers();

            HerlaadPosts();
            HerlaadEvents();
            HerlaadGebruikers();
            HerlaadComments();
            RefreshLeegStaten();
            SetActiveTab(TabPostsBtn);
        }
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

        private void HideAllPanels()
        {
            PostsPanel.Visibility    = Visibility.Collapsed;
            EventsPanel.Visibility   = Visibility.Collapsed;
            UsersPanel.Visibility    = Visibility.Collapsed;
            if (CommentsPanel != null) CommentsPanel.Visibility = Visibility.Collapsed;
        }

        private void OnTabPosts(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            PostsPanel.Visibility = Visibility.Visible;
            SetActiveTab(TabPostsBtn);
        }

        private void OnTabEvents(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            EventsPanel.Visibility = Visibility.Visible;
            SetActiveTab(TabEventsBtn);
        }

        private void OnTabComments(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            if (CommentsPanel != null) CommentsPanel.Visibility = Visibility.Visible;
            SetActiveTab(TabCommentsBtn);
        }

        private void OnTabUsers(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            UsersPanel.Visibility = Visibility.Visible;
            SetActiveTab(TabUsersBtn);
        }
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
            // Admin laadt ALLE gebruikers inclusief gebande rechtstreeks uit DB
            try
            {
                var alleUsers = UserRepository.GetAllInclusingBanned();
                foreach (var u in alleUsers.Where(u =>
                    (u.VolledigeNaam ?? "").ToLower().Contains(zoek) ||
                    (u.Username ?? "").ToLower().Contains(zoek)))
                    _gefilterdeGebruikers.Add(u);
            }
            catch
            {
                // Fallback op AppState als DB niet bereikbaar
                foreach (var u in _state.Gebruikers.Where(u =>
                    (u.VolledigeNaam ?? "").ToLower().Contains(zoek) ||
                    (u.Username ?? "").ToLower().Contains(zoek)))
                    _gefilterdeGebruikers.Add(u);
            }
            RefreshLeegStaten();
        }
        private void PostsSearchBox_TextChanged(object sender, TextChangedEventArgs e)  => HerlaadPosts();
        private void EventsSearchBox_TextChanged(object sender, TextChangedEventArgs e) => HerlaadEvents();
        private void UsersSearchBox_TextChanged(object sender, TextChangedEventArgs e)  => HerlaadGebruikers();
        private void OnPostVerwijderenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not AdminPost post) return;
            var res = MessageBox.Show($"Post van '{post.Auteur}' verwijderen?", "Bevestig",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            // Verwijder uit DB
            try { PostRepository.Verwijder(post.Id); } catch { }

            _state.Berichten.Remove(post);
            var fp = _state.FeedPosts.FirstOrDefault(f => f.DbId == post.Id);
            if (fp != null) _state.FeedPosts.Remove(fp);
        }

        private void OnEventVerwijderenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not AdminEvenement ev) return;
            var res = MessageBox.Show($"Activiteit '{ev.Naam}' verwijderen?", "Bevestig",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            try { EventRepository.Verwijder(ev.Id); } catch { }
            _state.Evenementen.Remove(ev);
        }

        private void OnGebruikerVerwijderenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not User user) return;
            var res = MessageBox.Show($"Gebruiker '{user.VolledigeNaam}' verwijderen?", "Bevestig",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            try { UserRepository.Verwijder(user.Id); } catch { }
            _state.Gebruikers.Remove(user);
            // Als verwijderde user = ingelogde user: navigeer naar login
            if (_state.HuidigeGebruiker == null)
            {
                ((MoreConnector)Window.GetWindow(this)).NavigateToLogin();
                return;
            }
        }
        private void OnBanToggleClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not User user) return;

            if (user.IsActive) // momenteel actief → bannen
            {
                var res = MessageBox.Show(
                    $"'{user.VolledigeNaam}' blokkeren?\n\nDeze gebruiker kan niet meer inloggen en hun content wordt verborgen.",
                    "Verban gebruiker", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes) return;

                try { UserRepository.BanGebruiker(user.Id); } catch { }
                user.IsActive = false;
                user.IsBanned = true;

                // Verwijder content uit AppState
                var posts = _state.FeedPosts.Where(p => p.UserId == user.Id).ToList();
                foreach (var p in posts) _state.FeedPosts.Remove(p);
                var berichten = _state.Berichten.Where(b => b.Auteur == user.VolledigeNaam || b.Auteur == user.DisplayNaam).ToList();
                foreach (var b in berichten) _state.Berichten.Remove(b);
                var events = _state.Evenementen.Where(ev => ev.CreatorId == user.Id).ToList();
                foreach (var ev in events) _state.Evenementen.Remove(ev);

                MessageBox.Show($"{user.VolledigeNaam} is geblokkeerd.", "Geblokkeerd", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else // momenteel gebanned → deblokkeren
            {
                var res = MessageBox.Show(
                    $"'{user.VolledigeNaam}' deblokkeren?\n\nDeze gebruiker kan opnieuw inloggen.",
                    "Deblokkeer gebruiker", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;

                try { UserRepository.UnbanGebruiker(user.Id); } catch { }
                user.IsActive = true;
                user.IsBanned = false;

                MessageBox.Show($"{user.VolledigeNaam} is gedeblokkeerd.", "Gedeblokkeerd", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            HerlaadGebruikers();
        }

        private void OnEmailWijzigenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not User user) return;

            var win = new Window
            {
                Title = $"E-mail wijzigen — {user.VolledigeNaam}",
                Width = 460, Height = 260,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this), ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(27, 42, 59))
            };

            var sp = new StackPanel { Margin = new Thickness(28) };
            sp.Children.Add(new TextBlock
            {
                Text = $"Huidig e-mailadres: {user.Email}",
                Foreground = System.Windows.Media.Brushes.Gray, FontSize = 12,
                Margin = new Thickness(0, 0, 0, 12)
            });

            var emailBox = new TextBox
            {
                Text = user.Email,
                Padding = new Thickness(10, 10, 10, 10), FontSize = 14,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(20, 35, 50)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 12)
            };
            sp.Children.Add(new TextBlock { Text = "Nieuw e-mailadres", Foreground = System.Windows.Media.Brushes.Gray, FontSize = 12, Margin = new Thickness(0,0,0,4) });
            sp.Children.Add(emailBox);

            var fout = new TextBlock { Foreground = System.Windows.Media.Brushes.Tomato, FontSize = 12, Visibility = Visibility.Collapsed, Margin = new Thickness(0, 0, 0, 8) };
            sp.Children.Add(fout);

            var opslaanBtn = new Button
            {
                Content = "✉  E-mail opslaan",
                Padding = new Thickness(20, 12, 20, 12),
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(51, 102, 153)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 14, FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Stretch, Height = 48
            };
            opslaanBtn.Click += (_, _) =>
            {
                string nieuw = emailBox.Text.Trim();
                if (!IsGeldigEmail(nieuw))
                {
                    fout.Text = "Vul een geldig e-mailadres in.";
                    fout.Visibility = Visibility.Visible;
                    return;
                }
                try
                {
                    UserRepository.UpdateProfiel(user.Id, user.Firstname, user.Lastname,
                        nieuw, user.Study, user.Bio, user.Username, user.ProfielFotoPad);
                    user.Email = nieuw;
                    MessageBox.Show("E-mailadres opgeslagen.", "Opgeslagen");
                    win.Close();
                }
                catch (System.Exception ex)
                {
                    fout.Text = $"Fout: {ex.Message}";
                    fout.Visibility = Visibility.Visible;
                }
            };
            sp.Children.Add(opslaanBtn);
            win.Content = sp;
            win.ShowDialog();
        }
        private void OnWachtwoordWijzigenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not User user) return;

            var win = new Window
            {
                Title = $"Wachtwoord — {user.DisplayNaam}",
                Width = 520, Height = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(27, 42, 59))
            };

            var sp = new StackPanel { Margin = new Thickness(28) };
            sp.Children.Add(new TextBlock
            {
                Text = $"Nieuw wachtwoord voor {user.DisplayNaam}",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 15, FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 16)
            });

            var pw1 = new PasswordBox { Padding = new Thickness(10, 10, 10, 10), FontSize = 14, Margin = new Thickness(0, 0, 0, 10) };
            sp.Children.Add(new TextBlock { Text = "Nieuw wachtwoord", Foreground = System.Windows.Media.Brushes.Gray, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) });
            sp.Children.Add(pw1);

            var pw2 = new PasswordBox { Padding = new Thickness(10, 10, 10, 10), FontSize = 14, Margin = new Thickness(0, 0, 0, 14) };
            sp.Children.Add(new TextBlock { Text = "Bevestig wachtwoord", Foreground = System.Windows.Media.Brushes.Gray, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) });
            sp.Children.Add(pw2);

            var fout = new TextBlock { Foreground = System.Windows.Media.Brushes.Tomato, FontSize = 12, Visibility = Visibility.Collapsed, Margin = new Thickness(0, 0, 0, 8) };
            sp.Children.Add(fout);

            var opslaanBtn = new Button
            {
                Content = "💾  Wachtwoord opslaan",
                Padding = new Thickness(20, 14, 20, 14),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 140, 0)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 15, FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 52
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
        private static bool IsGeldigEmail(string email)
        {
            try { _ = new System.Net.Mail.MailAddress(email); return true; }
            catch { return false; }
        }

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
        private void HerlaadComments()
        {
            string zoek = CommentsSearchBox?.Text?.ToLower() ?? "";
            _gefilterdeComments.Clear();
            try
            {
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
            if (PostsLeegTekst    != null) PostsLeegTekst.Visibility    = _gefilterdePosts.Count      == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (EventsLeegTekst   != null) EventsLeegTekst.Visibility   = _gefilterdeEvents.Count     == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (UsersLeegTekst    != null) UsersLeegTekst.Visibility    = _gefilterdeGebruikers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (CommentsLeegTekst != null) CommentsLeegTekst.Visibility  = _gefilterdeComments.Count  == 0 ? Visibility.Visible : Visibility.Collapsed;
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
