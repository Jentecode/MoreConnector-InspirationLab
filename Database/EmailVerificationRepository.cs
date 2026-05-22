using System;
using System.Security.Cryptography;

namespace MoreConnector.Database
{
    public static class EmailVerificationRepository
    {
        public static string MaakToken(string email)
        {
            using (var conn = DbConnection.GetConnection())
            using (var del = conn.CreateCommand())
            {
                del.CommandText = "DELETE FROM email_verifications WHERE email=@e";
                del.Parameters.AddWithValue("@e", email);
                del.ExecuteNonQuery();
            }

            string token   = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime verloopt = DateTime.Now.AddMinutes(30);

            using var conn2 = DbConnection.GetConnection();
            using var cmd   = conn2.CreateCommand();
            cmd.CommandText = "INSERT INTO email_verifications (email, token, expires_at) VALUES (@e, @t, @exp)";
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
            cmd.CommandText = @"SELECT COUNT(*) FROM email_verifications
                                WHERE email=@e AND token=@t AND expires_at > NOW()";
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@t", token);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public static void MarkeerGeverifieerd(string email)
        {
            using (var conn = DbConnection.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE users SET is_verified=1 WHERE email=@e";
                cmd.Parameters.AddWithValue("@e", email);
                cmd.ExecuteNonQuery();
            }
            using (var conn = DbConnection.GetConnection())
            using (var del  = conn.CreateCommand())
            {
                del.CommandText = "DELETE FROM email_verifications WHERE email=@e";
                del.Parameters.AddWithValue("@e", email);
                del.ExecuteNonQuery();
            }
        }
    }
}
