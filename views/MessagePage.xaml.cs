using MoreConnector.Database;
using MoreConnector.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MoreConnector.Views
{
    public partial class MessagePage : Page
    {
        private readonly AppState _state = AppState.Instance;

        // Huidige chat state
        private int    _activeUserId  = 0;   // DM
        private int    _activeGroupId = 0;   // Groep
        private bool   _isGroep       = false;

        public MessagePage(User? openDirectly = null)
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Berichten);

            ZoekBox.Text       = "Zoeken...";
            ZoekBox.Foreground = new SolidColorBrush(Colors.Gray);
            BerichtInput.Text  = "Schrijf een bericht...";
            BerichtInput.Foreground = new SolidColorBrush(Colors.Gray);

            LaadContacten();
            LaadGroepen();

            // Direct een chat openen (vanuit AndereProfielPage)
            if (openDirectly != null)
                OpenDM(openDirectly);
        }

        // ── Laden ────────────────────────────────────────────────────────────
        private void LaadContacten(string zoek = "")
        {
            BerichtenContactenPanel.ItemsSource = null;
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;

            List<User> vrienden = new();
            try { vrienden = FriendshipRepository.GetVrienden(eigenId); }
            catch { }

            if (!string.IsNullOrWhiteSpace(zoek) && zoek != "Zoeken...")
                vrienden = vrienden.Where(u =>
                    u.VolledigeNaam.ToLower().Contains(zoek.ToLower()) ||
                    u.Username.ToLower().Contains(zoek.ToLower())).ToList();

            var contacts = vrienden.Select(u => new Contact
            {
                UserId = u.Id,
                Naam   = string.IsNullOrWhiteSpace(u.Username) ? u.VolledigeNaam : $"@{u.Username}"
            }).ToList();

            BerichtenContactenPanel.ItemsSource = new ObservableCollection<Contact>(contacts);
        }

        private void LaadGroepen()
        {
            GroepenPanel.ItemsSource = null;
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;

            List<ChatGroep> groepen = new();
            try { groepen = GroupRepository.GetGroepenVanGebruiker(eigenId); }
            catch { }

            var items = groepen.Select(g => new Groep { GroupId = g.Id, Naam = g.Naam }).ToList();
            GroepenPanel.ItemsSource = new ObservableCollection<Groep>(items);
        }

        // ── Chat openen ───────────────────────────────────────────────────────
        private void OpenDM(User user)
        {
            _activeUserId  = user.Id;
            _activeGroupId = 0;
            _isGroep       = false;
            ChatNaamText.Text = string.IsNullOrWhiteSpace(user.Username)
                ? user.VolledigeNaam : $"@{user.Username}";
            LaadDMBerichten(user.Id);
        }

        private void LaadDMBerichten(int otherUserId)
        {
            ChatPanel.Children.Clear();
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            if (eigenId == 0) return;

            List<Message> berichten = new();
            try { berichten = MessageRepository.GetGesprek(eigenId, otherUserId); }
            catch { }

            foreach (var b in berichten)
                ChatPanel.Children.Add(BouwBerichtBlok(b.Content, b.SenderId == eigenId, b.SenderName));

            ChatScrollViewer.ScrollToBottom();
        }

        private void LaadGroepBerichten(int groupId)
        {
            ChatPanel.Children.Clear();
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;

            List<ChatBericht> berichten = new();
            try { berichten = GroupRepository.GetBerichten(groupId); }
            catch { }

            foreach (var b in berichten)
                ChatPanel.Children.Add(BouwBerichtBlok(b.Tekst, b.SenderId == eigenId, b.SenderNaam));

            ChatScrollViewer.ScrollToBottom();
        }

        private Border BouwBerichtBlok(string tekst, bool isEigen, string senderNaam)
        {
            var outer = new Border
            {
                Margin              = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = isEigen ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth            = 420
            };

            var sp = new StackPanel();

            if (!isEigen)
                sp.Children.Add(new TextBlock
                {
                    Text       = senderNaam,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                    FontSize   = 11, Margin = new Thickness(0, 0, 0, 2)
                });

            var bubble = new Border
            {
                Background  = isEigen
                    ? new SolidColorBrush(Color.FromRgb(255, 140, 0))
                    : new SolidColorBrush(Color.FromRgb(30, 46, 64)),
                CornerRadius = new CornerRadius(isEigen ? 14 : 14),
                Padding     = new Thickness(14, 10, 14, 10)
            };
            bubble.Child = new TextBlock
            {
                Text         = tekst,
                Foreground   = new SolidColorBrush(Colors.White),
                FontSize     = 14,
                TextWrapping = TextWrapping.Wrap
            };
            sp.Children.Add(bubble);
            outer.Child = sp;
            return outer;
        }

        // ── Contact klik ─────────────────────────────────────────────────────
        private void OnContactClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Contact contact)
            {
                _activeUserId  = contact.UserId;
                _activeGroupId = 0;
                _isGroep       = false;
                ChatNaamText.Text = contact.Naam;
                LaadDMBerichten(contact.UserId);
            }
        }

        // ── Groep klik ────────────────────────────────────────────────────────
        private void OnGroepClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Groep groep)
            {
                _activeGroupId = groep.GroupId;
                _activeUserId  = 0;
                _isGroep       = true;
                ChatNaamText.Text = groep.Naam;
                LaadGroepBerichten(groep.GroupId);
            }
        }

        // ── Bericht sturen ────────────────────────────────────────────────────
        private void OnStuurClick(object sender, RoutedEventArgs e) => StuurBericht();

        private void BerichtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !Keyboard.IsKeyDown(Key.LeftShift))
            { StuurBericht(); e.Handled = true; }
        }

        private void StuurBericht()
        {
            string tekst = BerichtInput.Text.Trim();
            if (string.IsNullOrEmpty(tekst) || tekst == "Schrijf een bericht...") return;
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            if (eigenId == 0) return;

            string senderNaam = _state.HuidigeGebruiker?.DisplayNaam ?? "Jij";

            if (_isGroep && _activeGroupId > 0)
            {
                try { GroupRepository.StuurBericht(_activeGroupId, eigenId, tekst); }
                catch { }
            }
            else if (!_isGroep && _activeUserId > 0)
            {
                try { MessageRepository.Verstuur(eigenId, _activeUserId, tekst); }
                catch { }
            }
            else
            {
                MessageBox.Show("Selecteer eerst een contact of groep.", "Geen chat geselecteerd");
                return;
            }

            ChatPanel.Children.Add(BouwBerichtBlok(tekst, true, senderNaam));
            ChatScrollViewer.ScrollToBottom();
            BerichtInput.Text       = "Schrijf een bericht...";
            BerichtInput.Foreground = new SolidColorBrush(Colors.Gray);
        }

        // ── Nieuw DM ─────────────────────────────────────────────────────────
        private void OnNieuwBerichtClick(object sender, RoutedEventArgs e)
            => Nav().AuthFrame.Navigate(new GebruikersPage());

        // ── Nieuwe groep ──────────────────────────────────────────────────────
        private void OnNieuweGroepClick(object sender, RoutedEventArgs e)
        {
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            if (eigenId == 0) return;

            var dialog = new NieuweGroepDialog(_state.HuidigeGebruiker!);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                LaadGroepen();
            }
        }

        // ── Zoeken ────────────────────────────────────────────────────────────
        private void ZoekBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ZoekBox.Text == "Zoeken...") { ZoekBox.Text = ""; ZoekBox.Foreground = new SolidColorBrush(Colors.Black); }
        }
        private void ZoekBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ZoekBox.Text)) { ZoekBox.Text = "Zoeken..."; ZoekBox.Foreground = new SolidColorBrush(Colors.Gray); LaadContacten(); }
        }
        private void ZoekBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ZoekBox.Text != "Zoeken...") LaadContacten(ZoekBox.Text);
        }

        private void BerichtInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BerichtInput.Text == "Schrijf een bericht...") { BerichtInput.Text = ""; BerichtInput.Foreground = new SolidColorBrush(Colors.Black); }
        }
        private void BerichtInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BerichtInput.Text)) { BerichtInput.Text = "Schrijf een bericht..."; BerichtInput.Foreground = new SolidColorBrush(Colors.Gray); }
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

    // ── Lokale modellen ───────────────────────────────────────────────────────
    public class Contact
    {
        public int    UserId { get; set; }
        public string Naam   { get; set; } = "";
        public bool   IsOnline { get; set; } = false;
    }
    public class Groep
    {
        public int    GroupId { get; set; }
        public string Naam    { get; set; } = "";
    }
    public class Bericht
    {
        public string Tekst   { get; set; } = "";
        public bool   IsEigen { get; set; }
    }
}
