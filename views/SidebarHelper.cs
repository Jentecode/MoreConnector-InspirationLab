using MoreConnector.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoreConnector.Views
{
    public static class SidebarHelper
    {
        public enum ActivePage { Home, Activiteiten, Berichten, Aanmaken, Profiel, Admin, Gebruikers, Notificaties, Geen }

        private static readonly SolidColorBrush ActiveBrush =
            new(Color.FromRgb(0x99, 0x3D, 0x00));

        public static void Init(Page page, ActivePage active)
        {
            page.Loaded += (_, _) => Refresh(page, active);
        }

        public static void Refresh(Page page, ActivePage active)
        {
            var user = AppState.Instance.HuidigeGebruiker; // User type

            SetText(page, "SideNaamLabel",
                user == null ? "Profiel" :
                string.IsNullOrWhiteSpace(user.Username) ? user.VolledigeNaam : $"@{user.Username}");


            // FIX: check both IsAdmin flag and Role string
            bool isAdmin = user?.IsAdmin == true || user?.Role == "Admin";
            SetVisibility(page, "BtnAdmin", isAdmin ? Visibility.Visible : Visibility.Collapsed);
            SetText(page, "SideRolLabel", isAdmin ? "Admin" : "");

            // Profielfoto
            var bmp = ImageHelper.LaadGeschaald(user?.ProfielFotoPad ?? "", 64);
            var img = FindName<Image>(page, "SideAvatarImg");
            if (img != null && bmp != null)
            {
                img.Source = bmp;
                var bg = FindName<Ellipse>(page, "SideAvatarBg");
                if (bg != null) bg.Visibility = Visibility.Collapsed;
            }

            // Actieve knop
            string[] btns = { "BtnHome", "BtnActiviteiten", "BtnBerichten", "BtnGebruikers", "BtnNotificaties", "BtnAanmaken", "BtnProfiel", "BtnAdmin" };
            string activeBtn = active switch
            {
                ActivePage.Home         => "BtnHome",
                ActivePage.Activiteiten => "BtnActiviteiten",
                ActivePage.Berichten    => "BtnBerichten",
                ActivePage.Aanmaken     => "BtnAanmaken",
                ActivePage.Profiel      => "BtnProfiel",
                ActivePage.Admin        => "BtnAdmin",
                ActivePage.Gebruikers   => "BtnGebruikers",
                ActivePage.Notificaties => "BtnNotificaties",
                _                       => ""
            };

            foreach (var name in btns)
            {
                var btn = FindName<Button>(page, name);
                if (btn == null) continue;
                btn.Background = name == activeBtn ? ActiveBrush : Brushes.Transparent;
            }
        }

        private static void SetText(Page page, string name, string text)
        {
            var tb = FindName<TextBlock>(page, name);
            if (tb != null) tb.Text = text;
        }

        private static void SetVisibility(Page page, string name, Visibility v)
        {
            var el = FindName<UIElement>(page, name);
            if (el != null) el.Visibility = v;
        }

        private static T? FindName<T>(Page page, string name) where T : class
            => page.FindName(name) as T;
    }
}
