using MoreConnector.Database;
using MoreConnector.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MoreConnector.Views
{
    public partial class MessagePage : Page
    {
        private readonly AppState _state = AppState.Instance;
        private int  _activeUserId  = 0;
        private int  _activeGroupId = 0;
        private bool _isGroep       = false;

        public MessagePage(User? openDirectly = null)
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Berichten);

            ZoekBox.Text       = "Zoek vrienden...";
            ZoekBox.Foreground = new SolidColorBrush(Colors.Gray);
            // Placeholder via overlay TextBlock

            LaadContacten();
            LaadGroepen();

            if (openDirectly != null) OpenDM(openDirectly);
        }
        private void LaadContacten(string zoek = "")
        {
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            List<User> vrienden = new();
            try { vrienden = FriendshipRepository.GetVrienden(eigenId); }
            catch { }

            if (!string.IsNullOrWhiteSpace(zoek) && zoek != "Zoek vrienden...")
                vrienden = vrienden.Where(u =>
                    u.VolledigeNaam.ToLower().Contains(zoek.ToLower()) ||
                    u.Username.ToLower().Contains(zoek.ToLower())).ToList();

            var contacts = vrienden.Select(u =>
            {
                string initLetter = u.Firstname.Length > 0 ? u.Firstname[0].ToString().ToUpper() : "?";
                var bmp = ImageHelper.LaadGeschaald(u.ProfielFotoPad, 80);
                return new ContactVM
                {
                    UserId     = u.Id,
                    Naam       = string.IsNullOrWhiteSpace(u.Username) ? u.VolledigeNaam : u.Username,
                    Initiaal   = bmp == null ? initLetter : "",
                    FotoBron   = bmp,
                    InitiaalVisible = bmp == null ? Visibility.Visible : Visibility.Collapsed,
                    LaatsteBerichtPreview = ""
                };
            }).ToList();

            BerichtenContactenPanel.ItemsSource = new ObservableCollection<ContactVM>(contacts);
        }

        private void LaadGroepen()
        {
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            List<ChatGroep> groepen = new();
            try { groepen = GroupRepository.GetGroepenVanGebruiker(eigenId); }
            catch { }

            var items = groepen.Select(g => new GroepVM
            {
                GroupId            = g.Id,
                Naam               = g.Naam,
                EigenaarId         = g.EigenaarId,
                LedenTekst         = $"{g.AantalLeden} leden",
                InstellingenVisible = g.EigenaarId == eigenId ? Visibility.Visible : Visibility.Collapsed
            }).ToList();

            GroepenPanel.ItemsSource = new ObservableCollection<GroepVM>(items);
        }
        private void OpenDM(User user)
        {
            _activeUserId  = user.Id;
            _activeGroupId = 0;
            _isGroep       = false;
            ChatNaamText.Text = string.IsNullOrWhiteSpace(user.Username) ? user.VolledigeNaam : $"@{user.Username}";

            // Header avatar
            LaadChatHeaderAvatar(user.ProfielFotoPad,
                user.Firstname.Length > 0 ? user.Firstname[0].ToString().ToUpper() : "?");

            LaadDMBerichten(user.Id);
            MarkeerActiefContact(user.Id);
        }

        private void MarkeerActiefContact(int userId)
        {
            if (BerichtenContactenPanel.ItemsSource is not System.Collections.ObjectModel.ObservableCollection<ContactVM> items) return;
            foreach (var item in items)
                item.IsActief = item.UserId == userId;
            // Herlaad panel zodat highlight zichtbaar wordt
            var temp = BerichtenContactenPanel.ItemsSource;
            BerichtenContactenPanel.ItemsSource = null;
            BerichtenContactenPanel.ItemsSource = temp;
        }

        private void LaadChatHeaderAvatar(string fotoPad, string initiaal)
        {
            var bmp = ImageHelper.LaadGeschaald(fotoPad, 80);
            if (bmp != null)
            {
                ChatHeaderAvatar.Source      = bmp;
                ChatHeaderInitiaal.Text      = "";
            }
            else
            {
                ChatHeaderAvatar.Source      = null;
                ChatHeaderInitiaal.Text      = initiaal;
            }
        }

        private void LaadDMBerichten(int otherUserId)
        {
            var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch };
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            List<Message> berichten = new();
            try { berichten = MessageRepository.GetGesprek(eigenId, otherUserId); }
            catch { }
            foreach (var b in berichten)
                panel.Children.Add(BouwBerichtBlok(b.Content, b.SenderId == eigenId, b.SenderName, b.SenderPhoto));
            ChatScrollViewer.Content = panel;
            ChatScrollViewer.ScrollToBottom();
        }

        private void LaadGroepBerichten(int groupId)
        {
            var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch };
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            List<ChatBericht> berichten = new();
            try { berichten = GroupRepository.GetBerichten(groupId); }
            catch { }
            foreach (var b in berichten)
                panel.Children.Add(BouwBerichtBlok(b.Tekst, b.SenderId == eigenId, b.SenderNaam, b.SenderPhoto));
            ChatScrollViewer.Content = panel;
            ChatScrollViewer.ScrollToBottom();
        }
        private Border BouwBerichtBlok(string tekst, bool isEigen, string senderNaam, string fotoPad)
        {
            var outer = new Border
            {
                Margin              = new Thickness(0, 0, 0, 14),
                HorizontalAlignment = isEigen ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth            = 480
            };

            var rij = new StackPanel { Orientation = Orientation.Horizontal };

            // Avatar links voor ontvangen berichten
            if (!isEigen)
            {
                var av = Models.AvatarHelper.Bouw(fotoPad, senderNaam, 36);
                av.Margin = new Thickness(0, 0, 8, 0);
                av.VerticalAlignment = VerticalAlignment.Bottom;
                rij.Children.Add(av);
            }

            var sp = new StackPanel { MaxWidth = isEigen ? 400 : 360 };
            if (!isEigen)
                sp.Children.Add(new TextBlock
                {
                    Text = senderNaam, Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                    FontSize = 12, Margin = new Thickness(4, 0, 0, 3)
                });

            var bubble = new Border
            {
                Background   = isEigen
                    ? new SolidColorBrush(Color.FromRgb(255, 140, 0))
                    : new SolidColorBrush(Color.FromRgb(30, 60, 90)),
                CornerRadius  = new CornerRadius(16),
                Padding      = new Thickness(16, 12, 16, 12),
                MinWidth     = 40,
                MaxWidth     = isEigen ? 400 : 360
            };
            bubble.Child = new TextBlock
            {
                Text = tekst,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap
            };
            sp.Children.Add(bubble);
            rij.Children.Add(sp);
            outer.Child = rij;
            return outer;
        }
        private void OnContactClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not ContactVM contact) return;
            _activeUserId  = contact.UserId;
            _activeGroupId = 0;
            _isGroep       = false;
            ChatNaamText.Text = contact.Naam;

            // Zoek user voor avatar
            var user = _state.Gebruikers.FirstOrDefault(u => u.Id == contact.UserId);
            LaadChatHeaderAvatar(user?.ProfielFotoPad ?? "", contact.Initiaal);
            LaadDMBerichten(contact.UserId);
            MarkeerActiefContact(contact.UserId);
        }
        private void OnGroepClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not GroepVM groep) return;
            _activeGroupId = groep.GroupId;
            _activeUserId  = 0;
            _isGroep       = true;
            ChatNaamText.Text = groep.Naam;
            ChatHeaderAvatar.Source = null;
            ChatHeaderInitiaal.Text = "👥";
            LaadGroepBerichten(groep.GroupId);
        }
        private void OnGroepInstellingenClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not GroepVM groep) return;
            e.Handled = true;

            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;

            var win = new Window
            {
                Title = $"Groep instellingen — {groep.Naam}",
                Width = 480, Height = 560,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this), ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(27, 42, 59))
            };

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var sp = new StackPanel { Margin = new Thickness(28) };
            scroll.Content = sp;

            // Groepsnaam
            sp.Children.Add(new TextBlock { Text = "Groepsnaam", Foreground = Brushes.Gray, FontSize = 12, Margin = new Thickness(0,0,0,4) });
            var naamBox = new TextBox { Text = groep.Naam, Padding = new Thickness(10,8,10,8), FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(20,35,50)), Foreground = Brushes.White,
                BorderThickness = new Thickness(0), Margin = new Thickness(0,0,0,20) };
            sp.Children.Add(naamBox);

            // Huidige leden
            sp.Children.Add(new TextBlock { Text = "Huidige leden", Foreground = Brushes.White, FontSize = 13, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,0,0,8) });
            var ledenPanel = new StackPanel();
            sp.Children.Add(ledenPanel);

            void HerlaadLeden()
            {
                ledenPanel.Children.Clear();
                try
                {
                    var leden = GroupRepository.GetLeden(groep.GroupId);
                    foreach (var lid in leden)
                    {
                        var rij = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,0,0,6) };
                        rij.Children.Add(new TextBlock { Text = lid.VolledigeNaam, Foreground = Brushes.White, FontSize = 13, Width = 240, VerticalAlignment = VerticalAlignment.Center });
                        if (lid.Id != eigenId)
                        {
                            var verwijderLid = new Button
                            {
                                Content = "✕ Verwijder", Foreground = Brushes.White, BorderThickness = new Thickness(0),
                                Background = new SolidColorBrush(Color.FromRgb(150,30,30)),
                                Padding = new Thickness(8,4,8,4), Cursor = System.Windows.Input.Cursors.Hand, Tag = lid.Id
                            };
                            verwijderLid.Click += (_, _) =>
                            {
                                try { GroupRepository.VerwijderLid(groep.GroupId, lid.Id); } catch { }
                                HerlaadLeden();
                            };
                            rij.Children.Add(verwijderLid);
                        }
                        ledenPanel.Children.Add(rij);
                    }
                }
                catch { }
            }
            HerlaadLeden();

            // Vrienden toevoegen
            sp.Children.Add(new TextBlock { Text = "Vriend toevoegen", Foreground = Brushes.White, FontSize = 13, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,20,0,8) });

            var vriendenPanel = new StackPanel();
            try
            {
                var vrienden = FriendshipRepository.GetVrienden(eigenId);
                foreach (var v in vrienden)
                {
                    var rij = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,0,0,6) };
                    rij.Children.Add(new TextBlock { Text = v.VolledigeNaam, Foreground = new SolidColorBrush(Color.FromRgb(180,190,200)), FontSize = 13, Width = 240, VerticalAlignment = VerticalAlignment.Center });
                    var addBtn = new Button
                    {
                        Content = "+ Voeg toe", Foreground = Brushes.White, BorderThickness = new Thickness(0),
                        Background = new SolidColorBrush(Color.FromRgb(0,100,60)),
                        Padding = new Thickness(8,4,8,4), Cursor = System.Windows.Input.Cursors.Hand, Tag = v.Id
                    };
                    // Check of al lid
                    try
                    {
                        var huidigeLeden = GroupRepository.GetLeden(groep.GroupId);
                        if (huidigeLeden.Any(l => l.Id == v.Id))
                        { addBtn.Content = "✓ Al lid"; addBtn.IsEnabled = false; }
                    }
                    catch { }

                    addBtn.Click += (_, _) =>
                    {
                        try { GroupRepository.VoegLidToe(groep.GroupId, v.Id); } catch { }
                        addBtn.Content = "✓ Toegevoegd";
                        addBtn.IsEnabled = false;
                        HerlaadLeden();
                    };
                    rij.Children.Add(addBtn);
                    vriendenPanel.Children.Add(rij);
                }
            }
            catch { }
            sp.Children.Add(vriendenPanel);

            var fout = new TextBlock { Foreground = Brushes.Tomato, FontSize = 12, Visibility = Visibility.Collapsed, Margin = new Thickness(0,12,0,4) };
            sp.Children.Add(fout);

            var opslaanBtn = new Button
            {
                Content = "💾  Naam opslaan", Padding = new Thickness(20, 12, 20, 12),
                Background = new SolidColorBrush(Color.FromRgb(255, 140, 0)), Foreground = Brushes.White,
                BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 14, FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Stretch, Height = 48, Margin = new Thickness(0,16,0,4)
            };
            opslaanBtn.Click += (_, _) =>
            {
                string nieuweNaam = naamBox.Text.Trim();
                if (string.IsNullOrEmpty(nieuweNaam)) { fout.Text = "Naam mag niet leeg zijn."; fout.Visibility = Visibility.Visible; return; }
                try { GroupRepository.HernoemenGroep(groep.GroupId, nieuweNaam); } catch { }
                groep.Naam = nieuweNaam;
                LaadGroepen();
                win.Close();
            };
            sp.Children.Add(opslaanBtn);

            // Groep verwijderen (gevaarlijk — rood)
            var verwijderGroepBtn = new Button
            {
                Content = "🗑  Groep verwijderen", Padding = new Thickness(20, 12, 20, 12),
                Background = new SolidColorBrush(Color.FromRgb(140, 30, 30)), Foreground = Brushes.White,
                BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 13, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 44
            };
            verwijderGroepBtn.Click += (_, _) =>
            {
                var r = MessageBox.Show($"Groep '{groep.Naam}' permanent verwijderen?",
                    "Bevestig", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (r != MessageBoxResult.Yes) return;
                try { GroupRepository.VerwijderGroep(groep.GroupId); } catch { }
                win.Close();
                LaadGroepen();
                ChatNaamText.Text = "";
                ChatScrollViewer.Content = new StackPanel();
            };
            sp.Children.Add(verwijderGroepBtn);

            win.Content = scroll;
            win.ShowDialog();
        }
        private void OnStuurClick(object sender, RoutedEventArgs e) => StuurBericht();

        private void BerichtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !Keyboard.IsKeyDown(Key.LeftShift))
            { StuurBericht(); e.Handled = true; }
        }

        private void StuurBericht()
        {
            string tekst = BerichtInput.Text.Trim();
            if (string.IsNullOrEmpty(tekst)) return;
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            if (eigenId == 0) return;

            string senderNaam = _state.HuidigeGebruiker?.DisplayNaam ?? "Jij";

            if (_isGroep && _activeGroupId > 0)
                try { GroupRepository.StuurBericht(_activeGroupId, eigenId, tekst); } catch { }
            else if (!_isGroep && _activeUserId > 0)
                try { MessageRepository.Verstuur(eigenId, _activeUserId, tekst); } catch { }
            else
            {
                MessageBox.Show("Selecteer eerst een contact of groep.", "Geen chat geselecteerd");
                return;
            }

            // Voeg bericht toe aan bestaand panel
            if (ChatScrollViewer.Content is StackPanel panel)
            {
                panel.Children.Add(BouwBerichtBlok(tekst, true, senderNaam, _state.HuidigeGebruiker?.ProfielFotoPad ?? ""));
                ChatScrollViewer.ScrollToBottom();
            }

            BerichtInput.Text = "";
            BerichtInput.Focus();
        }
        private void OnNieuweGroepClick(object sender, RoutedEventArgs e)
        {
            int eigenId = _state.HuidigeGebruiker?.Id ?? 0;
            if (eigenId == 0) return;
            var dialog = new NieuweGroepDialog(_state.HuidigeGebruiker!);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true) LaadGroepen();
        }
        private void ZoekBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ZoekBox.Text == "Zoek vrienden...") { ZoekBox.Text = ""; ZoekBox.Foreground = new SolidColorBrush(Colors.White); }
        }
        private void ZoekBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ZoekBox.Text)) { ZoekBox.Text = "Zoek vrienden..."; ZoekBox.Foreground = new SolidColorBrush(Colors.Gray); LaadContacten(); }
        }
        private void ZoekBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ZoekBox.Text != "Zoek vrienden...") LaadContacten(ZoekBox.Text);
        }

        private void BerichtInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (BerichtPlaceholder != null)
                BerichtPlaceholder.Visibility = string.IsNullOrEmpty(BerichtInput.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
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
    public class ContactVM : System.ComponentModel.INotifyPropertyChanged
    {
        public int       UserId               { get; set; }
        public string    Naam                 { get; set; } = "";
        public string    Initiaal             { get; set; } = "";
        public BitmapSource? FotoBron         { get; set; }
        public Visibility InitiaalVisible     { get; set; } = Visibility.Visible;
        public string    LaatsteBerichtPreview { get; set; } = "";

        private bool _isActief;
        public bool IsActief
        {
            get => _isActief;
            set { _isActief = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsActief))); PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(AchtergrondKleur))); }
        }
        // Bolletje kleur: oranje als actief, transparant anders
        public System.Windows.Media.Brush AchtergrondKleur => IsActief
            ? new SolidColorBrush(Color.FromRgb(255, 140, 0))
            : new SolidColorBrush(Colors.Transparent);

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
    public class GroepVM : System.ComponentModel.INotifyPropertyChanged
    {
        public int       GroupId             { get; set; }
        public int       EigenaarId          { get; set; }
        private string _naam = "";
        public string Naam
        {
            get => _naam;
            set { _naam = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Naam))); }
        }
        public string    LedenTekst          { get; set; } = "";
        public Visibility InstellingenVisible { get; set; } = Visibility.Collapsed;
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
    // backwards compat
    public class Contact  { public int UserId { get; set; } public string Naam { get; set; } = ""; }
    public class Groep    { public int GroupId { get; set; } public string Naam { get; set; } = ""; }
    public class Bericht  { public string Tekst { get; set; } = ""; public bool IsEigen { get; set; } }
    // Converter: bool -> Visibility (voor het bolletje)
    public class BoolToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public static readonly BoolToVisibilityConverter Instance = new();
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value is true ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new System.NotImplementedException();
    }

}
