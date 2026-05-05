using System.Windows;
namespace MoreConnector.Views
{
    public partial class MoreConnector : Window
    {
        public MoreConnector()
        {
            InitializeComponent();
            // Start on Login page
            AuthFrame.Navigate(new Login());
        }

        public void NavigateToCreateAccount()
        {
            AuthFrame.Navigate(new AccountCreation());
        }

        public void NavigateToPasswordReset()
        {
            AuthFrame.Navigate(new PasswordReset());
        }
        public void NavigateToLogin()
        {
            AuthFrame.Navigate(new Login());
        }
        public void NavigateToFeed()
        {
            AuthFrame.Navigate(new Feed());
        }
        public void NavigateToProfile()
        {
            AuthFrame.Navigate(new ProfilePage());
        }
        public void NavigateToEditProfile()
        {
            AuthFrame.Navigate(new ProfileEditPage());
        }
    }
}