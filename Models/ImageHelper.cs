using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace MoreConnector.Models
{
    /// <summary>
    /// Laadt afbeeldingen met maximale kwaliteit en geheugenefficiëntie.
    /// Gebruikt CacheOption.OnLoad zodat het bestand direct losgelaten wordt.
    /// DecodePixelWidth zorgt dat grote foto's niet onnodig veel RAM gebruiken.
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Laad een afbeelding op volledige resolutie (voor grote weergave zoals detail-pagina's).
        /// </summary>
        public static BitmapImage? LaadVolledig(string pad)
        {
            if (string.IsNullOrWhiteSpace(pad) || !File.Exists(pad)) return null;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource            = new Uri(pad, UriKind.Absolute);
                bmp.CacheOption          = BitmapCacheOption.OnLoad;
                bmp.CreateOptions        = BitmapCreateOptions.IgnoreImageCache;
                bmp.EndInit();
                bmp.Freeze(); // thread-safe + minder GC
                return bmp;
            }
            catch { return null; }
        }

        /// <summary>
        /// Laad een afbeelding geschaald naar een maximale breedte (voor thumbnails/avatars).
        /// Behoudt aspect ratio. Gebruikt hardware-scaling zodat het scherp blijft.
        /// </summary>
        public static BitmapImage? LaadGeschaald(string pad, int maxBreedte = 400)
        {
            if (string.IsNullOrWhiteSpace(pad) || !File.Exists(pad)) return null;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource            = new Uri(pad, UriKind.Absolute);
                bmp.CacheOption          = BitmapCacheOption.OnLoad;
                bmp.CreateOptions        = BitmapCreateOptions.IgnoreImageCache;
                bmp.DecodePixelWidth     = maxBreedte;
                // Geen DecodePixelHeight → aspect ratio blijft behouden
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }

        /// <summary>Ronde clip-geometry voor avatar-afbeeldingen (Ellipse-effect op Image).</summary>
        public static System.Windows.Media.EllipseGeometry RondeClip(double straal)
            => new(new System.Windows.Point(straal, straal), straal, straal);
    }
}
