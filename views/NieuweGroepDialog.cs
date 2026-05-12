using MoreConnector.Database;
using MoreConnector.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MoreConnector.Views
{
    /// <summary>Modal dialog om een nieuwe groepschat te maken met geselecteerde vrienden.</summary>
    public class NieuweGroepDialog : Window
    {
        private readonly User _eigenaar;
        private readonly TextBox _naamBox   = new();
        private readonly StackPanel _ledenPanel = new();
        private readonly List<int> _geselecteerd = new();

        public NieuweGroepDialog(User eigenaar)
        {
            _eigenaar = eigenaar;
            Title  = "Nieuwe groep aanmaken";
            Width  = 440;
            Height = 520;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = new SolidColorBrush(Color.FromRgb(27, 42, 59));

            var root = new StackPanel { Margin = new Thickness(24) };

            root.Children.Add(new TextBlock
            {
                Text = "Groepsnaam", Foreground = Brushes.White,
                FontSize = 13, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6)
            });

            _naamBox.Padding = new Thickness(10, 8, 10, 8);
            _naamBox.FontSize = 14;
            _naamBox.Margin = new Thickness(0, 0, 0, 20);
            root.Children.Add(_naamBox);

            root.Children.Add(new TextBlock
            {
                Text = "Voeg vrienden toe", Foreground = Brushes.White,
                FontSize = 13, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 8)
            });

            var scroll = new ScrollViewer { Height = 250, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            scroll.Content = _ledenPanel;
            root.Children.Add(scroll);

            var aanmaken = new Button
            {
                Content = "Groep aanmaken",
                Background = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                Foreground = Brushes.White, BorderThickness = new Thickness(0),
                Padding = new Thickness(16, 10, 16, 10), FontSize = 14, FontWeight = FontWeights.SemiBold,
                Cursor = System.Windows.Input.Cursors.Hand, Margin = new Thickness(0, 16, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            aanmaken.Click += OnAanmaken;
            root.Children.Add(aanmaken);

            Content = root;
            LaadVrienden();
        }

        private void LaadVrienden()
        {
            _ledenPanel.Children.Clear();
            List<User> vrienden = new();
            try { vrienden = FriendshipRepository.GetVrienden(_eigenaar.Id); }
            catch { }

            foreach (var v in vrienden)
            {
                var rij = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
                var cb  = new CheckBox { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), Tag = v.Id };
                cb.Checked   += (_, _) => _geselecteerd.Add(v.Id);
                cb.Unchecked += (_, _) => _geselecteerd.Remove(v.Id);
                rij.Children.Add(cb);
                rij.Children.Add(new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(v.Username) ? v.VolledigeNaam : $"@{v.Username}",
                    Foreground = Brushes.White, FontSize = 14, VerticalAlignment = VerticalAlignment.Center
                });
                _ledenPanel.Children.Add(rij);
            }

            if (vrienden.Count == 0)
                _ledenPanel.Children.Add(new TextBlock
                {
                    Text = "Je hebt nog geen vrienden om toe te voegen.",
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 153, 170)), FontSize = 13
                });
        }

        private void OnAanmaken(object sender, RoutedEventArgs e)
        {
            string naam = _naamBox.Text.Trim();
            if (string.IsNullOrEmpty(naam))
            {
                MessageBox.Show("Vul een groepsnaam in.", "Validatie"); return;
            }

            try
            {
                int groupId = GroupRepository.MaakGroep(_eigenaar.Id, naam);
                foreach (int uid in _geselecteerd)
                    GroupRepository.VoegLidToe(groupId, uid);

                MessageBox.Show($"Groep '{naam}' aangemaakt!", "Succes");
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Fout: {ex.Message}", "Fout");
            }
        }
    }
}
