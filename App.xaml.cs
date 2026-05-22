using MoreConnector.Database;
using System.Windows;

namespace MoreConnector
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Test DB verbinding bij opstarten (niet-blokkerend: app werkt ook zonder DB)
            bool dbOk = DbConnection.TestVerbinding();
            if (!dbOk)
            {
                // App gaat door in offline modus — login valt terug op admin-account
                System.Diagnostics.Debug.WriteLine("DB niet bereikbaar — offline modus");
            }
        }
    }
}
