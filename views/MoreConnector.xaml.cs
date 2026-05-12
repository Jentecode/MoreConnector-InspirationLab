using MoreConnector.Models;
using System.Windows;

namespace MoreConnector.Views
{
    public partial class MoreConnector : Window
    {
        public User? CurrentUser
        {
            get => AppState.Instance.HuidigeGebruiker;
            set => AppState.Instance.HuidigeGebruiker = value;
        }

        public bool IsAdmin => AppState.Instance.IsAdmin;

        public MoreConnector()
        {
            InitializeComponent();
            AuthFrame.Navigate(new Login());
        }

        public void NavigateToCreateAccount() => AuthFrame.Navigate(new AccountCreation());
        public void NavigateToPasswordReset() => AuthFrame.Navigate(new PasswordReset());
        public void NavigateToLogin()         => AuthFrame.Navigate(new Login());
        public void NavigateToFeed()          => AuthFrame.Navigate(new Feed());
        public void NavigateToProfile()       => AuthFrame.Navigate(new ProfilePage());
        public void NavigateToEditProfile()   => AuthFrame.Navigate(new ProfileEditPage());
        public void NavigateToAdmin()         => AuthFrame.Navigate(new AdminPage());
    }
}
