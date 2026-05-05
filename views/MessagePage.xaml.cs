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
        private bool _berichtIsPlaceholder = true;
        private bool _zoekIsPlaceholder = true;

        private Contact _geselecteerdeContact = null;
        private Groep _geselecteerdeGroep = null;

        public static ObservableCollection<Contact> Contacten { get; } = new ObservableCollection<Contact>
        {
            new Contact { Naam = "Elsa V." },
            new Contact { Naam = "Geert VB." },
            new Contact { Naam = "Jan Sels" }
        };

        public static ObservableCollection<Groep> Groepen { get; } = new ObservableCollection<Groep>
        {
            new Groep { Naam = "BADI" }
        };

        public MessagePage()
        {
            InitializeComponent();

            ZoekBox.Text = "Zoeken...";
            ZoekBox.Foreground = new SolidColorBrush(Colors.Gray);

            BerichtInput.Text = "Schrijf een bericht...";
            BerichtInput.Foreground = new SolidColorBrush(Colors.Gray);

            BerichtenContactenPanel.ItemsSource = Contacten;
            GroepenPanel.ItemsSource = Groepen;
        }

        // ── Contact geselecteerd ──────────────────────────────────
        private void OnContactClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Contact contact)
            {
                _geselecteerdeContact = contact;
                _geselecteerdeGroep = null;
                ChatNaamText.Text = contact.Naam;
                LaadBerichten(contact.Berichten);
            }
        }

        // ── Groep geselecteerd ────────────────────────────────────
        private void OnGroepClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Groep groep)
            {
                _geselecteerdeGroep = groep;
                _geselecteerdeContact = null;
                ChatNaamText.Text = groep.Naam;
                LaadBerichten(groep.Berichten);
            }
        }

        // ── Berichten laden ───────────────────────────────────────
        private void LaadBerichten(ObservableCollection<Bericht> berichten)
        {
            ChatPanel.Children.Clear();
            foreach (var bericht in berichten)
                ChatPanel.Children.Add(BouwBerichtBlok(bericht));

            ChatScrollViewer.ScrollToBottom();
        }

        // ── Bericht UI-blok bouwen ────────────────────────────────
        private FrameworkElement BouwBerichtBlok(Bericht bericht)
        {
            bool isEigen = bericht.IsEigen;

            var bel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(217, 217, 217)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(isEigen ? 80 : 0, 0, isEigen ? 0 : 80, 0),
                MaxWidth = 400
            };

            bel.Child = new TextBlock
            {
                Text = bericht.Tekst,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                TextWrapping = TextWrapping.Wrap
            };

            var rij = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            rij.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            rij.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            if (isEigen)
            {
                Grid.SetColumn(bel, 0);
                rij.HorizontalAlignment = HorizontalAlignment.Right;

                var avatar = new System.Windows.Shapes.Ellipse
                {
                    Width = 36,
                    Height = 36,
                    Fill = new SolidColorBrush(Color.FromRgb(217, 217, 217)),
                    Margin = new Thickness(8, 0, 0, 0)
                };
                Grid.SetColumn(avatar, 1);
                rij.Children.Add(bel);
                rij.Children.Add(avatar);
            }
            else
            {
                var wrapper = new StackPanel { Orientation = Orientation.Horizontal };
                var avatar = new System.Windows.Shapes.Ellipse
                {
                    Width = 36,
                    Height = 36,
                    Fill = new SolidColorBrush(Color.FromRgb(217, 217, 217)),
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                wrapper.Children.Add(avatar);
                wrapper.Children.Add(bel);
                Grid.SetColumn(wrapper, 0);
                rij.Children.Add(wrapper);
            }

            return rij;
        }

        // ── Bericht sturen ────────────────────────────────────────
        private void OnStuurClick(object sender, RoutedEventArgs e) => StuurBericht();

        private void BerichtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) StuurBericht();
        }

        private void StuurBericht()
        {
            if (_berichtIsPlaceholder) return;
            if (_geselecteerdeContact == null && _geselecteerdeGroep == null) return;

            string tekst = BerichtInput.Text.Trim();
            if (string.IsNullOrEmpty(tekst)) return;

            var nieuw = new Bericht { Tekst = tekst, IsEigen = true };

            if (_geselecteerdeContact != null)
                _geselecteerdeContact.Berichten.Add(nieuw);
            else
                _geselecteerdeGroep.Berichten.Add(nieuw);

            ChatPanel.Children.Add(BouwBerichtBlok(nieuw));
            ChatScrollViewer.ScrollToBottom();

            _berichtIsPlaceholder = false;
            BerichtInput.Text = "";
            BerichtInput.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            BerichtInput.Focus();
        }

        // ── ZoekBox placeholder + zoeklogica ─────────────────────
        private void ZoekBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_zoekIsPlaceholder)
            {
                ZoekBox.Text = "";
                ZoekBox.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                _zoekIsPlaceholder = false;
            }
        }

        private void ZoekBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ZoekBox.Text))
            {
                ZoekBox.Text = "Zoeken...";
                ZoekBox.Foreground = new SolidColorBrush(Colors.Gray);
                _zoekIsPlaceholder = true;

                // Reset lijsten naar alle items
                BerichtenContactenPanel.ItemsSource = Contacten;
                GroepenPanel.ItemsSource = Groepen;
            }
        }

        private void ZoekBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_zoekIsPlaceholder) return;

            string zoekterm = ZoekBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(zoekterm))
            {
                BerichtenContactenPanel.ItemsSource = Contacten;
                GroepenPanel.ItemsSource = Groepen;
                return;
            }

            BerichtenContactenPanel.ItemsSource = Contacten
                .Where(c => c.Naam.ToLower().Contains(zoekterm))
                .ToList();

            GroepenPanel.ItemsSource = Groepen
                .Where(g => g.Naam.ToLower().Contains(zoekterm))
                .ToList();
        }

        // ── BerichtInput placeholder ──────────────────────────────
        private void BerichtInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_berichtIsPlaceholder)
            {
                BerichtInput.Text = "";
                BerichtInput.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                _berichtIsPlaceholder = false;
            }
        }

        private void BerichtInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BerichtInput.Text))
            {
                BerichtInput.Text = "Schrijf een bericht...";
                BerichtInput.Foreground = new SolidColorBrush(Colors.Gray);
                _berichtIsPlaceholder = true;
            }
        }

        // ── Nieuw DM / groep aanmaken ─────────────────────────────
        private void OnNieuwBerichtClick(object sender, RoutedEventArgs e)
            => MessageBox.Show("Nieuw bericht — nog niet geïmplementeerd.", "Nieuw");

        private void OnNieuweGroepClick(object sender, RoutedEventArgs e)
            => MessageBox.Show("Nieuwe groep — nog niet geïmplementeerd.", "Nieuw");

        // ── Navigatie ─────────────────────────────────────────────
        private void OnHomeClick(object sender, RoutedEventArgs e)
            => ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new Feed());

        private void OnActiviteitenClick(object sender, RoutedEventArgs e)
            => ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ActivityPage());

        private void OnBerichtenClick(object sender, RoutedEventArgs e)
            => ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new MessagePage());

        private void OnAanmakenClick(object sender, RoutedEventArgs e)
            => ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ActivityMaker());

        private void OnProfielClick(object sender, RoutedEventArgs e)
            => ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ProfilePage());
    }

    // ── Modellen ──────────────────────────────────────────────────

    public class Contact
    {
        public string Naam { get; set; }
        public bool IsOnline { get; set; } = true;
        public ObservableCollection<Bericht> Berichten { get; } = new ObservableCollection<Bericht>();
    }

    public class Groep
    {
        public string Naam { get; set; }
        public ObservableCollection<Bericht> Berichten { get; } = new ObservableCollection<Bericht>();
    }

    public class Bericht
    {
        public string Tekst { get; set; }
        public bool IsEigen { get; set; }
    }
}