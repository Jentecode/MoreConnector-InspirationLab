using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MoreConnector.Views
{
    public partial class ProfilePage : Page
    {
        // Vul deze later met echte data vanuit je database
        public static ObservableCollection<ProfielActiviteit> MijnActiviteiten { get; } = new ObservableCollection<ProfielActiviteit>();
        public static ObservableCollection<Connectie> MijnConnecties { get; } = new ObservableCollection<Connectie>();

        public ProfilePage()
        {
            InitializeComponent();
            MijnActiviteitenPanel.ItemsSource = MijnActiviteiten;
            ConnectiesPanel.ItemsSource = MijnConnecties;

            // Interesses tags laden
            LaadInteresses();
        }

        private void LaadInteresses()
        {
            // TODO: haal interesses op uit database/gebruikersprofiel
            var interesses = new[] { "Sport", "Muziek", "IT" };

            foreach (var interesse in interesses)
            {
                var tag = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(204, 82, 0)),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(14, 6, 14, 6),
                    Margin = new Thickness(0, 0, 8, 8),
                    Child = new TextBlock
                    {
                        Text = interesse,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 13
                    }
                };
                InteressesPanel.Children.Add(tag);
            }
        }

        private void OnProfielBewerkenClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ProfileEditPage());
        }

        private void OnHomeClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new Feed());
        }

        private void OnActiviteitenClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ActivityPage());
        }

        private void OnBerichtenClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new MessagePage());
        }

        private void OnAanmakenClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ActivityMaker());
        }

        private void OnProfielClick(object sender, RoutedEventArgs e)
        {
            ((Views.MoreConnector)Window.GetWindow(this)).AuthFrame.Navigate(new ProfilePage());
        }
    }

    // Zet later in Models/ProfielActiviteit.cs
    public class ProfielActiviteit
    {
        public string Titel { get; set; }
        public string Locatie { get; set; }

        public Visibility InfoVisibility => string.IsNullOrWhiteSpace(Titel) ? Visibility.Collapsed : Visibility.Visible;
    }

    // Zet later in Models/Connectie.cs
    public class Connectie
    {
        public string Naam { get; set; }
        public string Status { get; set; } = "Vriend";
    }
}