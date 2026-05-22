using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoreConnector.Models
{
    /// <summary>
    /// Centrale helper: bouwt altijd een cirkelvormige avatar.
    /// Toont profielfoto als die beschikbaar is, anders de eerste letter van naam/username.
    /// </summary>
    public static class AvatarHelper
    {
        /// <param name="fotoPad">Pad naar de profielfoto (mag leeg zijn).</param>
        /// <param name="naam">Naam of username — eerste letter wordt getoond als er geen foto is.</param>
        /// <param name="grootte">Diameter van de cirkel in pixels.</param>
        public static Grid Bouw(string fotoPad, string naam, double grootte = 40)
        {
            var g = new Grid { Width = grootte, Height = grootte };

            var ell = new Ellipse
            {
                Width  = grootte,
                Height = grootte,
                Fill   = new SolidColorBrush(Color.FromRgb(255, 140, 0))
            };

            var bmp = ImageHelper.LaadGeschaald(fotoPad, (int)(grootte * 2));
            if (bmp != null)
            {
                ell.Fill = new ImageBrush { ImageSource = bmp, Stretch = Stretch.UniformToFill };
                g.Children.Add(ell);
            }
            else
            {
                // Altijd eerste letter tonen — nooit een lege of grijze cirkel
                string letter = "?";
                if (!string.IsNullOrWhiteSpace(naam))
                    letter = naam.TrimStart('@')[0].ToString().ToUpper();

                g.Children.Add(ell);
                g.Children.Add(new TextBlock
                {
                    Text                = letter,
                    Foreground          = new SolidColorBrush(Colors.White),
                    FontSize            = grootte * 0.42,
                    FontWeight          = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center
                });
            }

            return g;
        }
    }
}
