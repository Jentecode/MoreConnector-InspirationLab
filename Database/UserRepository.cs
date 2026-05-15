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
        public static string HashPassword(string plain)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
            return Convert.ToHexString(bytes).ToLower();
        }

        public static User? Login(string email, string plainPassword)
        {
            string hash = HashPassword(plainPassword);
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, firstname, lastname, email, study, bio, created_at,
                       COALESCE(username,'')      AS username,
                       COALESCE(is_admin, 0)      AS is_admin,
                       COALESCE(profile_photo,'') AS profile_photo
                FROM   users
                WHERE  email = @email AND PASSWORD = @pw
                LIMIT  1";
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pw",    hash);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return MapUser(reader);
        }

        public static int Registreer(string firstname, string lastname, string email,
                                      string plainPassword, string study = "", string bio = "",
                                      string username = "")
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO users (firstname, lastname, email, PASSWORD, study, bio, username)
                VALUES (@fn, @ln, @email, @pw, @study, @bio, @un);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@fn",    firstname);
            cmd.Parameters.AddWithValue("@ln",    lastname);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pw",    HashPassword(plainPassword));
            cmd.Parameters.AddWithValue("@study", study);
            cmd.Parameters.AddWithValue("@bio",   bio);
            cmd.Parameters.AddWithValue("@un",    username);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static User? GetById(int id)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"SELECT id, firstname, lastname, email, study, bio, created_at,
                                       COALESCE(username,'') AS username,
                                       COALESCE(is_admin,0) AS is_admin,
                                       COALESCE(profile_photo,'') AS profile_photo
                                FROM users WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapUser(r) : null;
        }

        public static List<User> GetAll()
        {
            var list = new List<User>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"SELECT id, firstname, lastname, email, study, bio, created_at,
                                       COALESCE(username,'') AS username,
                                       COALESCE(is_admin,0) AS is_admin,
                                       COALESCE(profile_photo,'') AS profile_photo
                                FROM users ORDER BY firstname";
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapUser(r));
            return list;
        }

        public static void UpdateProfiel(int id, string firstname, string lastname,
                                          string email, string study, string bio,
                                          string username = "", string profilePhoto = "")
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE users
                SET firstname=@fn, lastname=@ln, email=@email, study=@study,
                    bio=@bio, username=@un, profile_photo=@photo
                WHERE id=@id";
            cmd.Parameters.AddWithValue("@fn",    firstname);
            cmd.Parameters.AddWithValue("@ln",    lastname);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@study", study);
            cmd.Parameters.AddWithValue("@bio",   bio);
            cmd.Parameters.AddWithValue("@un",    username);
            cmd.Parameters.AddWithValue("@photo", profilePhoto);
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

        public static bool WijzigEigenWachtwoord(int id, string huidigPlain, string nieuwPlain)
        {
            using var conn = DbConnection.GetConnection();
            using var chk  = conn.CreateCommand();
            chk.CommandText = "SELECT COUNT(*) FROM users WHERE id=@id AND PASSWORD=@pw";
            chk.Parameters.AddWithValue("@id", id);
            chk.Parameters.AddWithValue("@pw", HashPassword(huidigPlain));
            if (Convert.ToInt32(chk.ExecuteScalar()) == 0) return false;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE users SET PASSWORD=@pw WHERE id=@id";
            cmd.Parameters.AddWithValue("@pw", HashPassword(nieuwPlain));
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return true;
        }

        public static void Verwijder(int id)
        {
            using var conn = DbConnection.GetConnection();
            // Verwijder ook gerelateerde data zodat sessie stopt
            foreach (var sql in new[]
            {
                "DELETE FROM likes             WHERE user_id=@id",
                "DELETE FROM comments          WHERE user_id=@id",
                "DELETE FROM event_participants WHERE user_id=@id",
                "DELETE FROM friendships       WHERE sender_id=@id OR receiver_id=@id",
                "DELETE FROM posts             WHERE user_id=@id",
                "DELETE FROM events            WHERE creator_id=@id",
                "DELETE FROM users             WHERE id=@id"
            })
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@id", id);
                try { cmd.ExecuteNonQuery(); } catch { }
            }
        }

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
            using (var del = conn.CreateCommand())
            {
                del.CommandText = "DELETE FROM user_interests WHERE user_id=@uid";
                del.Parameters.AddWithValue("@uid", userId);
                del.ExecuteNonQuery();
            }
            foreach (var naam in namen)
            {
                int interestId;
                using (var sel = conn.CreateCommand())
                {
                    sel.CommandText = "SELECT id FROM interests WHERE name=@n LIMIT 1";
                    sel.Parameters.AddWithValue("@n", naam);
                    var res = sel.ExecuteScalar();
                    if (res != null) { interestId = Convert.ToInt32(res); }
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

        private static User MapUser(MySqlDataReader r)
        {
            bool isAdmin = false;
            try { isAdmin = r.GetInt32("is_admin") == 1; } catch { }
            string username = "";
            try { username = r.IsDBNull(r.GetOrdinal("username")) ? "" : r.GetString("username"); } catch { }
            string photo = "";
            try { photo = r.IsDBNull(r.GetOrdinal("profile_photo")) ? "" : r.GetString("profile_photo"); } catch { }

            return new User
            {
                Id           = r.GetInt32("id"),
                Firstname    = r.GetString("firstname"),
                Lastname     = r.GetString("lastname"),
                Email        = r.GetString("email"),
                Study        = r.IsDBNull(r.GetOrdinal("study")) ? "" : r.GetString("study"),
                Bio          = r.IsDBNull(r.GetOrdinal("bio"))   ? "" : r.GetString("bio"),
                CreatedAt    = r.GetDateTime("created_at"),
                Username     = username,
                Role         = isAdmin ? "Admin" : "User",
                IsAdmin      = isAdmin,
                ProfielFotoPad = photo
            };
        }
    }
}
