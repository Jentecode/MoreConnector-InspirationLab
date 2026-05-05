using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MoreConnector.Views
{
    public partial class ActivityPage : Page
    {
        public static ObservableCollection<Activiteit> Activiteits { get; } = new ObservableCollection<Activiteit>();

        public ActivityPage()
        {
            InitializeComponent();
            SearchBox.Text = "zoeken";
            PostPanel.ItemsSource = Activiteits;
        }

        private void OnVerwijderClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Activiteit activiteit)
                Activiteits.Remove(activiteit);
        }

        private void OnBekijkAllesClick(object sender, RoutedEventArgs e)
            => MessageBox.Show("Nog niet geïmplementeerd.", "Navigatie");

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "zoek posts...")
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = "zoek posts...";
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

    public class Activiteits
    {
        public int Id { get; set; }
        public string Titel { get; set; }
        public string Locatie { get; set; }
        public bool CanDelete { get; set; }

        public Visibility CanDeleteVisibility => CanDelete ? Visibility.Visible : Visibility.Collapsed;
        public Visibility InfoVisibility => string.IsNullOrWhiteSpace(Titel) ? Visibility.Collapsed : Visibility.Visible;
    }
}