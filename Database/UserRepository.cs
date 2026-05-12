using MoreConnector.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MoreConnector.Database
{
    public static class UserRepository
    {
        // ── Hash ─────────────────────────────────────────────────────────────
        public static string HashPassword(string plain)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
            return Convert.ToHexString(bytes).ToLower();
        }

        // ── Login ─────────────────────────────────────────────────────────────
        /// <summary>Zoekt een gebruiker op email + wachtwoord. Geeft null bij mislukking.</summary>
        public static User? Login(string email, string plainPassword)
        {
            string hash = HashPassword(plainPassword);
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, firstname, lastname, email, study, bio, created_at
                FROM   users
                WHERE  email = @email AND PASSWORD = @pw
                LIMIT  1";
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pw",    hash);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return MapUser(reader);
        }

        // ── Registreer ────────────────────────────────────────────────────────
        public static int Registreer(string firstname, string lastname, string email,
                                      string plainPassword, string study = "", string bio = "")
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO users (firstname, lastname, email, PASSWORD, study, bio)
                VALUES (@fn, @ln, @email, @pw, @study, @bio);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@fn",    firstname);
            cmd.Parameters.AddWithValue("@ln",    lastname);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pw",    HashPassword(plainPassword));
            cmd.Parameters.AddWithValue("@study", study);
            cmd.Parameters.AddWithValue("@bio",   bio);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ── Ophalen ───────────────────────────────────────────────────────────
        public static User? GetById(int id)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT id, firstname, lastname, email, study, bio, created_at FROM users WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapUser(r) : null;
        }

        public static List<User> GetAll()
        {
            var list = new List<User>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT id, firstname, lastname, email, study, bio, created_at FROM users ORDER BY firstname";
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapUser(r));
            return list;
        }

        // ── Bijwerken ─────────────────────────────────────────────────────────
        public static void UpdateProfiel(int id, string firstname, string lastname,
                                          string email, string study, string bio)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE users
                SET firstname=@fn, lastname=@ln, email=@email, study=@study, bio=@bio
                WHERE id=@id";
            cmd.Parameters.AddWithValue("@fn",    firstname);
            cmd.Parameters.AddWithValue("@ln",    lastname);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@study", study);
            cmd.Parameters.AddWithValue("@bio",   bio);
            cmd.Parameters.AddWithValue("@id",    id);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateWachtwoord(int id, string plainPassword)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "UPDATE users SET PASSWORD=@pw WHERE id=@id";
            cmd.Parameters.AddWithValue("@pw", HashPassword(plainPassword));
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public static void Verwijder(int id)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM users WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ── Interesses ────────────────────────────────────────────────────────
        public static List<Interest> GetInteresses(int userId)
        {
            var list = new List<Interest>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT i.id, i.name FROM interests i
                JOIN user_interests ui ON ui.interest_id = i.id
                WHERE ui.user_id = @uid";
            cmd.Parameters.AddWithValue("@uid", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(new Interest { Id = r.GetInt32(0), Name = r.GetString(1) });
            return list;
        }

        public static void SlaInteressesOp(int userId, List<string> namen)
        {
            using var conn = DbConnection.GetConnection();

            // Verwijder oude
            using (var del = conn.CreateCommand())
            {
                del.CommandText = "DELETE FROM user_interests WHERE user_id=@uid";
                del.Parameters.AddWithValue("@uid", userId);
                del.ExecuteNonQuery();
            }

            foreach (var naam in namen)
            {
                // Zorg dat interesse bestaat
                int interestId;
                using (var sel = conn.CreateCommand())
                {
                    sel.CommandText = "SELECT id FROM interests WHERE name=@n LIMIT 1";
                    sel.Parameters.AddWithValue("@n", naam);
                    var res = sel.ExecuteScalar();
                    if (res != null)
                    {
                        interestId = Convert.ToInt32(res);
                    }
                    else
                    {
                        using var ins = conn.CreateCommand();
                        ins.CommandText = "INSERT INTO interests (name) VALUES (@n); SELECT LAST_INSERT_ID();";
                        ins.Parameters.AddWithValue("@n", naam);
                        interestId = Convert.ToInt32(ins.ExecuteScalar());
                    }
                }

                using var link = conn.CreateCommand();
                link.CommandText = "INSERT INTO user_interests (user_id, interest_id) VALUES (@uid, @iid)";
                link.Parameters.AddWithValue("@uid", userId);
                link.Parameters.AddWithValue("@iid", interestId);
                link.ExecuteNonQuery();
            }
        }

        // ── Mapper ────────────────────────────────────────────────────────────
        private static User MapUser(MySqlDataReader r) => new()
        {
            Id        = r.GetInt32("id"),
            Firstname = r.GetString("firstname"),
            Lastname  = r.GetString("lastname"),
            Email     = r.GetString("email"),
            Study     = r.IsDBNull(r.GetOrdinal("study")) ? "" : r.GetString("study"),
            Bio       = r.IsDBNull(r.GetOrdinal("bio"))   ? "" : r.GetString("bio"),
            CreatedAt = r.GetDateTime("created_at")
        };
    }
}
