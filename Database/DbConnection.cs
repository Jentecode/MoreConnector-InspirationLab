using MySqlConnector;
using System;
using System.Windows;

namespace MoreConnector.Database
{
    /// <summary>
    /// Beheert de MySQL verbinding. Pas de constanten aan naar jouw lokale XAMPP-instellingen.
    /// </summary>
    public static class DbConnection
    {
        // ── Verbindingsinstellingen ───────────────────────────────────────────
        private const string Server   = "localhost";
        private const int    Port     = 3306;
        private const string Database = "moreconnector";
        private const string User     = "root";
        private const string Password = "";          // XAMPP standaard: leeg

        private static readonly string _connectionString =
            $"Server={Server};Port={Port};Database={Database};User={User};Password={Password};CharSet=utf8mb4;";

        /// <summary>Geeft een open MySqlConnection terug. Caller is verantwoordelijk voor Dispose.</summary>
        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        /// <summary>Test de verbinding bij opstarten. Geeft false terug als de DB niet bereikbaar is.</summary>
        public static bool TestVerbinding()
        {
            try
            {
                using var conn = GetConnection();
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Kan geen verbinding maken met de database.\n\n{ex.Message}\n\n" +
                    "Controleer of XAMPP/MySQL actief is en de database 'moreconnector' bestaat.",
                    "Databasefout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }
    }
}
