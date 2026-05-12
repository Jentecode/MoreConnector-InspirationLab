using MoreConnector.Database;
using MoreConnector.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MoreConnector.Views
{
    public partial class ProfileEditPage : Page
    {
        private readonly AppState _state = AppState.Instance;
        private readonly List<string> _tags = new();

        public ProfileEditPage()
        {
            InitializeComponent();
            SidebarHelper.Init(this, SidebarHelper.ActivePage.Profiel);
            LaadHuidigeData();
        }

        // ── Laad huidige data ────────────────────────────────────────────────
        private void LaadHuidigeData()
        {
            var user = _state.HuidigeGebruiker;
            if (user == null) return;

            VoornaamInput.Text       = user.Voornaam;
            AchternaamInput.Text     = user.Achternaam;
            EmailInput.Text          = user.Email;
            TelefoonnummerInput.Text = user.Telefoonnummer;
            StudierichtingInput.Text = user.Studierichting;
            BioInput.Text            = user.Bio;
            UsernameInput.Text       = user.Username;

            // Profielfoto preview
            LaadFotoPreview(user.ProfielFotoPad);

            // Tags
            _tags.Clear();
            TagsPanel.Children.Clear();
            foreach (var tag in user.Tags) VoegTagChipToe(tag);
        }

        private void LaadFotoPreview(string pad)
        {
            var bmp = ImageHelper.LaadGeschaald(pad, 220);
            if (bmp != null)
            {
                FotoPreview.Source = bmp;
                FotoAchtergrond.Visibility = Visibility.Collapsed;
            }
            else
            {
                FotoPreview.Source = null;
                FotoAchtergrond.Visibility = Visibility.Visible;
            }
        }

        // ── Tags ─────────────────────────────────────────────────────────────
        private void OnTagToevoegen(object sender, RoutedEventArgs e)
        {
            string tag = TagInput.Text.Trim().TrimStart('#');
            if (string.IsNullOrEmpty(tag) || _tags.Contains(tag)) return;
            _tags.Add(tag);
            TagInput.Text = "";
            VoegTagChipToe(tag);
        }

        private void VoegTagChipToe(string tag)
        {
            if (!_tags.Contains(tag)) _tags.Add(tag);

            var chip = new Border
            {
                Background  = new SolidColorBrush(Color.FromRgb(204, 82, 0)),
                CornerRadius = new CornerRadius(12),
                Padding     = new Thickness(10, 4, 10, 4),
                Margin      = new Thickness(0, 0, 6, 6)
            };
            var inner = new StackPanel { Orientation = Orientation.Horizontal };
            inner.Children.Add(new TextBlock
            {
                Text = $"#{tag}", Foreground = new SolidColorBrush(Colors.White),
                FontSize = 13, VerticalAlignment = VerticalAlignment.Center
            });
            var del = new Button
            {
                Content = " ×", Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0), Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14, Cursor = System.Windows.Input.Cursors.Hand
            };
            del.Click += (_, _) => { _tags.Remove(tag); TagsPanel.Children.Remove(chip); };
            inner.Children.Add(del);
            chip.Child = inner;
            TagsPanel.Children.Add(chip);
        }

        // ── Opslaan ──────────────────────────────────────────────────────────
        private void OnOpslaanClick(object sender, RoutedEventArgs e)
        {
            string voornaam       = VoornaamInput.Text.Trim();
            string achternaam     = AchternaamInput.Text.Trim();
            string email          = EmailInput.Text.Trim();
            string telefoon       = TelefoonnummerInput.Text.Trim();
            string studierichting = StudierichtingInput.Text.Trim();
            string bio            = BioInput.Text.Trim();
            string username       = UsernameInput.Text.Trim();

            if (string.IsNullOrEmpty(voornaam) || string.IsNullOrEmpty(achternaam))
            {
                MessageBox.Show("Voornaam en achternaam zijn verplicht.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                MessageBox.Show("Vul een geldig e-mailadres in.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(username)) _state.PasUsernameToe(username);
            _state.PasProfielToe(voornaam, achternaam, email, telefoon, studierichting, bio);

            if (_state.HuidigeGebruiker != null)
            {
                // Sla interesses op in DB
                try { UserRepository.SlaInteressesOp(_state.HuidigeGebruiker.Id, _tags); }
                catch { /* DB niet beschikbaar - lokaal opslaan */ }
            }

            MessageBox.Show("Profiel opgeslagen!", "Opgeslagen", MessageBoxButton.OK, MessageBoxImage.Information);
            Nav().AuthFrame.Navigate(new ProfilePage());
        }

        // ── Foto beheer ───────────────────────────────────────────────────────
        private void OnWijzigFotoClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Afbeeldingen|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp",
                Title  = "Kies profielfoto"
            };
            if (dialog.ShowDialog() == true && _state.HuidigeGebruiker != null)
            {
                _state.HuidigeGebruiker.ProfielFotoPad = dialog.FileName;
                LaadFotoPreview(dialog.FileName);
            }
        }

        private void OnVerwijderFotoClick(object sender, RoutedEventArgs e)
        {
            if (_state.HuidigeGebruiker != null) _state.HuidigeGebruiker.ProfielFotoPad = "";
            LaadFotoPreview("");
        }

        // ── Account verwijderen ───────────────────────────────────────────────
        private void OnAccountVerwijderenClick(object sender, RoutedEventArgs e)
        {
            var bevestig = MessageBox.Show(
                "Weet je zeker dat je je account wil verwijderen?",
                "Account verwijderen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (bevestig == MessageBoxResult.Yes)
            {
                var g = _state.Gebruikers.FirstOrDefault(x => x.Id == _state.HuidigeGebruiker?.Id);
                if (g != null) _state.Gebruikers.Remove(g);
                _state.HuidigeGebruiker = null;
                Nav().NavigateToLogin();
            }
        }

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
