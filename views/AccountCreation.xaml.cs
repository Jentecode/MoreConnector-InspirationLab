using MoreConnector.Database;
using MoreConnector.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MoreConnector
{
    public partial class AccountCreation : Page
    {
        public AccountCreation()
        {
            InitializeComponent();
        }

        private void TxtVoornaam_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtVoornaam.Text == "Voornaam")
            {
                TxtVoornaam.Text = "";
                TxtVoornaam.Foreground = Brushes.Black;
            }
        }

        private void TxtVoornaam_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtVoornaam.Text))
            {
                TxtVoornaam.Text = "Voornaam";
                TxtVoornaam.Foreground = Brushes.Gray;
            }
        }

        private void TxtAchternaam_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtAchternaam.Text == "Achternaam")
            {
                TxtAchternaam.Text = "";
                TxtAchternaam.Foreground = Brushes.Black;
            }
        }

        private void TxtAchternaam_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtAchternaam.Text))
            {
                TxtAchternaam.Text = "Achternaam";
                TxtAchternaam.Foreground = Brushes.Gray;
            }
        }

        private void TxtEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtEmail.Text == "email-adres")
            {
                TxtEmail.Text = "";
                TxtEmail.Foreground = Brushes.Black;
            }
        }

        private void TxtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                TxtEmail.Text = "email-adres";
                TxtEmail.Foreground = Brushes.Gray;
            }
        }

        private void TxtTelefoon_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtTelefoon.Text == "telefoonnummer")
            {
                TxtTelefoon.Text = "";
                TxtTelefoon.Foreground = Brushes.Black;
            }
        }

        private void TxtTelefoon_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTelefoon.Text))
            {
                TxtTelefoon.Text = "telefoonnummer";
                TxtTelefoon.Foreground = Brushes.Gray;
            }
        }

        private void TxtStudierichting_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtStudierichting.Text == "studierichting")
            {
                TxtStudierichting.Text = "";
                TxtStudierichting.Foreground = Brushes.Black;
            }
        }

        private void TxtStudierichting_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtStudierichting.Text))
            {
                TxtStudierichting.Text = "studierichting";
                TxtStudierichting.Foreground = Brushes.Gray;
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            string voornaam = TxtVoornaam.Text.Trim();
            string achternaam = TxtAchternaam.Text.Trim();
            string email = TxtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(voornaam) || voornaam == "Voornaam" ||
                string.IsNullOrWhiteSpace(achternaam) || achternaam == "Achternaam")
            {
                MessageBox.Show("Voornaam en achternaam zijn verplicht.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || email == "email-adres" || !email.Contains("@"))
            {
                MessageBox.Show("Vul een geldig e-mailadres in.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TxtWachtwoord.Password.Length == 0)
            {
                MessageBox.Show("Vul een wachtwoord in.", "Validatiefout",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string studierichting = TxtStudierichting.Text.Trim();
                if (studierichting == "Studierichting") studierichting = "";

                int newId = UserRepository.Registreer(
                    voornaam, achternaam, email,
                    TxtWachtwoord.Password,
                    studierichting);

                MessageBox.Show("Account aangemaakt! Je kunt nu inloggen.", "Succes",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ((Views.MoreConnector)Window.GetWindow(this)).NavigateToLogin();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Fout bij aanmaken account:\n{ex.Message}", "Fout",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtWachtwoord_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (WachtwoordPlaceholder != null)
            {
                WachtwoordPlaceholder.Visibility = TxtWachtwoord.Password.Length > 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }
    }
}
