using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoreConnector.Views
{
    public partial class Feed : Page
    {
        // Beide collecties bewaard — Posts voor activiteiten, FeedPosts voor sociale posts
        public static ObservableCollection<Activiteits> Posts { get; } = new ObservableCollection<Activiteits>();
        public static ObservableCollection<FeedPost> FeedPosts { get; } = new ObservableCollection<FeedPost>();

        public Feed()
        {
            InitializeComponent();
            SearchBox.Text = "Zoeken...";
            PostPanel.ItemsSource = Posts;
            LaadFeed();

            // Herlaad de feed als er nieuwe posts bijkomen
            FeedPosts.CollectionChanged += (s, e) => LaadFeed();
        }

        private void LaadFeed()
        {
            FeedPanel.Children.Clear();
            foreach (var post in FeedPosts)
                FeedPanel.Children.Add(BouwPostKaart(post));
        }

        // ── Postkaart bouwen ──────────────────────────────────────
        private Border BouwPostKaart(FeedPost post)
        {
            var kaart = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 46, 64)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var rootGrid = new Grid();
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(420) });
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // ── LINKS: auteur, afbeelding, caption ───────────────
            var links = new StackPanel();
            Grid.SetColumn(links, 0);

            var auteurRij = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12)
            };
            auteurRij.Children.Add(new Ellipse
            {
                Width = 36,
                Height = 36,
                Fill = new SolidColorBrush(Color.FromRgb(217, 217, 217)),
                Margin = new Thickness(0, 0, 10, 0)
            });
            auteurRij.Children.Add(new TextBlock
            {
                Text = post.AuteurNaam,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            });
            links.Children.Add(auteurRij);

            var afbeeldingBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(217, 217, 217)),
                CornerRadius = new CornerRadius(8),
                Height = 280,
                Margin = new Thickness(0, 0, 0, 12)
            };

            if (!string.IsNullOrEmpty(post.AfbeeldingPad))
            {
                afbeeldingBorder.Child = new Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(
                                  new System.Uri(post.AfbeeldingPad, System.UriKind.RelativeOrAbsolute)),
                    Stretch = Stretch.UniformToFill
                };
            }
            else
            {
                afbeeldingBorder.Child = new TextBlock
                {
                    Text = "afbeelding post",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                    FontSize = 14
                };
            }
            links.Children.Add(afbeeldingBorder);

            var captionBorder = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10)
            };
            captionBorder.Child = new TextBlock
            {
                Text = post.Beschrijving,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };
            links.Children.Add(captionBorder);
            rootGrid.Children.Add(links);

            // ── RECHTS: reacties + invoerveld ────────────────────
            var rechts = new StackPanel
            {
                Margin = new Thickness(24, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(rechts, 1);

            var reactiesPanel = new StackPanel();
            foreach (var reactie in post.Reacties)
                reactiesPanel.Children.Add(BouwReactieBlok(reactie));
            rechts.Children.Add(reactiesPanel);

            rechts.Children.Add(BouwReactieInvoer(post, reactiesPanel));
            rootGrid.Children.Add(rechts);

            kaart.Child = rootGrid;
            return kaart;
        }

        // ── Reactieblok bouwen ────────────────────────────────────
        private StackPanel BouwReactieBlok(FeedReactie reactie)
        {
            var stack = new StackPanel
            {
                Margin = new Thickness(reactie.IsReply ? 24 : 0, 0, 0, 12)
            };

            var header = new StackPanel { Orientation = Orientation.Horizontal };
            header.Children.Add(new Ellipse
            {
                Width = 32,
                Height = 32,
                Fill = new SolidColorBrush(Color.FromRgb(217, 217, 217)),
                Margin = new Thickness(0, 0, 8, 0)
            });
            header.Children.Add(new TextBlock
            {
                Text = reactie.AuteurNaam,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            });
            stack.Children.Add(header);

            stack.Children.Add(new TextBlock
            {
                Text = reactie.Tekst,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 13,
                Margin = new Thickness(40, 4, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });

            return stack;
        }

        // ── Reactie invoerveld bouwen ─────────────────────────────
        private Border BouwReactieInvoer(FeedPost post, StackPanel reactiesPanel)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(13, 27, 42)),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(14, 8, 8, 8),
                Margin = new Thickness(0, 8, 0, 0)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textBox = new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = "Schrijf een reactie..."
            };
            textBox.GotFocus += (s, e) => { if (textBox.Text == "Schrijf een reactie...") textBox.Text = ""; };
            textBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(textBox.Text)) textBox.Text = "Schrijf een reactie..."; };
            Grid.SetColumn(textBox, 0);
            grid.Children.Add(textBox);

            var stuurBtn = new Button
            {
                Content = "→",
                Background = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Width = 32,
                Height = 32,
                FontSize = 16,
                Cursor = Cursors.Hand
            };

            stuurBtn.Click += (s, e) =>
            {
                string tekst = textBox.Text.Trim();
                if (string.IsNullOrEmpty(tekst) || tekst == "Schrijf een reactie...") return;

                var nieuweReactie = new FeedReactie
                {
                    AuteurNaam = "Jente P.", // TODO: vervang met ingelogde gebruiker
                    Tekst = tekst,
                    IsReply = false
                };

                post.Reacties.Add(nieuweReactie);
                reactiesPanel.Children.Add(BouwReactieBlok(nieuweReactie));
                textBox.Text = "Schrijf een reactie...";

                // TODO: sla reactie op in database
            };

            Grid.SetColumn(stuurBtn, 1);
            grid.Children.Add(stuurBtn);

            border.Child = grid;
            return border;
        }

        // ── Activiteit verwijderen (vanuit XAML DataTemplate) ─────
        private void OnVerwijderClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Activiteits activiteit)
                Posts.Remove(activiteit);
        }

        // ── Zoekbalk placeholder ──────────────────────────────────
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Zoeken...") SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) SearchBox.Text = "Zoeken...";
        }

        // ── Navigatie (via MoreConnector hoofdvenster) ─────────────
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

    public class Activiteit
    {
        public int Id { get; set; }
        public string Titel { get; set; }
        public string Locatie { get; set; }
        public bool CanDelete { get; set; }

        public Visibility CanDeleteVisibility
            => CanDelete ? Visibility.Visible : Visibility.Collapsed;
        public Visibility InfoVisibility
            => string.IsNullOrWhiteSpace(Titel) ? Visibility.Collapsed : Visibility.Visible;
    }

    public class FeedPost
    {
        public string AuteurNaam { get; set; }
        public string Beschrijving { get; set; }
        public string AfbeeldingPad { get; set; }
        public ObservableCollection<FeedReactie> Reacties { get; } = new ObservableCollection<FeedReactie>();
    }

    public class FeedReactie
    {
        public string AuteurNaam { get; set; }
        public string Tekst { get; set; }
        public bool IsReply { get; set; }
    }
}