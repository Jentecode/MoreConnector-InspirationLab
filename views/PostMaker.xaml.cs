using MoreConnector.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MoreConnector.Views
{
    public partial class PostMaker : Page
    {
        private string _afbeeldingPad = "";
        private readonly List<string> _tags = new();
        private readonly AppState _state = AppState.Instance;

        public PostMaker() { InitializeComponent(); }

        private void OnAfbeeldingClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Afbeeldingen|*.jpg;*.jpeg;*.png;*.bmp"
            };
            if (dialog.ShowDialog() == true)
            {
                _afbeeldingPad = dialog.FileName;
                AfbeeldingButton.Content = $"✓ {System.IO.Path.GetFileName(dialog.FileName)}";
                var bmp = ImageHelper.LaadGeschaald(_afbeeldingPad, 160);
                if (bmp != null)
                {
                    AfbeeldingPreview.Source = bmp;
                    AfbeeldingPreviewBorder.Visibility = Visibility.Visible;
                }
            }
        }

        private void OnTagToevoegen(object sender, RoutedEventArgs e)
        {
            string tag = TagInput.Text.Trim().TrimStart('#');
            if (string.IsNullOrEmpty(tag) || _tags.Contains(tag)) return;

            _tags.Add(tag);
            TagInput.Text = "";

            var chip = new Border
            {
                Background   = new SolidColorBrush(Color.FromRgb(204, 82, 0)),
                CornerRadius  = new CornerRadius(12),
                Padding      = new Thickness(10, 4, 10, 4),
                Margin       = new Thickness(0, 0, 6, 6)
            };
            var inner = new StackPanel { Orientation = Orientation.Horizontal };
            inner.Children.Add(new TextBlock
            {
                Text       = $"#{tag}",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize   = 13,
                VerticalAlignment = VerticalAlignment.Center
            });
            var verwijder = new Button
            {
                Content         = " ×",
                Background      = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground      = new SolidColorBrush(Colors.White),
                FontSize        = 14,
                Cursor          = System.Windows.Input.Cursors.Hand,
                Tag             = tag
            };
            verwijder.Click += (_, _) =>
            {
                _tags.Remove(tag);
                TagsPanel.Children.Remove(chip);
            };
            inner.Children.Add(verwijder);
            chip.Child = inner;
            TagsPanel.Children.Add(chip);
        }

        private void OnPostDelenClick(object sender, RoutedEventArgs e)
        {
            string beschrijving = BeschrijvingInput.Text.Trim();
            if (string.IsNullOrEmpty(beschrijving))
            {
                MessageBox.Show("Voer een beschrijving in.", "Validatie");
                return;
            }

            string auteur = _state.HuidigeGebruiker?.DisplayNaam ?? "Onbekend";

            // Tags toevoegen aan beschrijving
            string tagStr = _tags.Count > 0 ? "\n" + string.Join(" ", _tags.ConvertAll(t => $"#{t}")) : "";

            _state.VoegPostToe(auteur, beschrijving + tagStr, _afbeeldingPad);

            Nav().AuthFrame.Navigate(new Feed());
        }

        private void OnAnnulerenClick(object sender, RoutedEventArgs e) => Nav().AuthFrame.Navigate(new AanmakenKeuze());

        private void OnHomeClick(object sender, RoutedEventArgs e)         => Nav().AuthFrame.Navigate(new Feed());

        private void OnBerichtenClick(object sender, RoutedEventArgs e)    => Nav().AuthFrame.Navigate(new MessagePage());
        private void OnAanmakenClick(object sender, RoutedEventArgs e)     => Nav().AuthFrame.Navigate(new AanmakenKeuze());
        private void OnProfielClick(object sender, RoutedEventArgs e)      => Nav().AuthFrame.Navigate(new ProfilePage());

        private void OnBeschrijvingChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int len = BeschrijvingInput.Text.Length;
            BeschrijvingTeller.Text = $"{len} / 500";
            BeschrijvingTeller.Foreground = len > 450
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Tomato)
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(170,170,170));
        }


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
