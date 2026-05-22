using System;
using System.Security.Cryptography;

namespace MoreConnector.Database
{
    public static class PasswordResetRepository
    {
        public static string MaakToken(string email)
        {
            // Verwijder oude tokens voor dit e-mailadres
            using (var conn = DbConnection.GetConnection())
            using (var del = conn.CreateCommand())
            {
                del.CommandText = "DELETE FROM password_resets WHERE email=@e";
                del.Parameters.AddWithValue("@e", email);
                del.ExecuteNonQuery();
            }

            // Genereer 6-cijferige code
            string token = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime verloopt = DateTime.Now.AddMinutes(15);

            using var conn2 = DbConnection.GetConnection();
            using var cmd   = conn2.CreateCommand();
            cmd.CommandText = "INSERT INTO password_resets (email, token, expires_at) VALUES (@e, @t, @exp)";
            cmd.Parameters.AddWithValue("@e",   email);
            cmd.Parameters.AddWithValue("@t",   token);
            cmd.Parameters.AddWithValue("@exp", verloopt);
            cmd.ExecuteNonQuery();

            return token;
        }

        public static bool ValideerToken(string email, string token)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) FROM password_resets
                                WHERE email=@e AND token=@t AND expires_at > NOW()";
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@t", token);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public static void Verwijder(string email)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM password_resets WHERE email=@e";
            cmd.Parameters.AddWithValue("@e", email);
            cmd.ExecuteNonQuery();
        }
    }
}
