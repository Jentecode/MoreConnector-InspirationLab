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
        // Zet een wachtwoord om naar een veilige hash
        public static string HashPassword(string plain)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
            return Convert.ToHexString(bytes).ToLower();
        }

        // Inloggen: geeft null als e-mail/wachtwoord fout is of account gebanned is
        public static User? Login(string email, string plainPassword)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, firstname, lastname, email, study, bio, created_at,
                       COALESCE(username,'')      AS username,
                       COALESCE(is_admin,    0)   AS is_admin,
                       COALESCE(profile_photo,'') AS profile_photo,
                       COALESCE(is_active,   1)   AS is_active,
                       COALESCE(is_verified, 1)   AS is_verified
                FROM users
                WHERE email = @email AND PASSWORD = @pw
                LIMIT 1";
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pw",    HashPassword(plainPassword));

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            var user = MapUser(r);
            return user.IsActive ? user : null; // gebande users krijgen null
        }

        // Nieuw account aanmaken, geeft het nieuwe user-id terug
        public static int Registreer(string firstname, string lastname, string email,
                                      string password, string study = "", string bio = "",
                                      string username = "")
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO users (firstname, lastname, email, PASSWORD, study, bio, username, is_verified)
                VALUES (@fn, @ln, @email, @pw, @study, @bio, @un, 0);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@fn",    firstname);
            cmd.Parameters.AddWithValue("@ln",    lastname);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pw",    HashPassword(password));
            cmd.Parameters.AddWithValue("@study", study);
            cmd.Parameters.AddWithValue("@bio",   bio);
            cmd.Parameters.AddWithValue("@un",    username);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Haal één gebruiker op via id
        public static User? GetById(int id)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, firstname, lastname, email, study, bio, created_at,
                       COALESCE(username,'')      AS username,
                       COALESCE(is_admin,    0)   AS is_admin,
                       COALESCE(profile_photo,'') AS profile_photo,
                       COALESCE(is_active,   1)   AS is_active,
                       COALESCE(is_verified, 1)   AS is_verified
                FROM users WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapUser(r) : null;
        }

        // Haal één gebruiker op via e-mail
        public static User? GetByEmail(string email)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, firstname, lastname, email, study, bio, created_at,
                       COALESCE(username,'')      AS username,
                       COALESCE(is_admin,    0)   AS is_admin,
                       COALESCE(profile_photo,'') AS profile_photo,
                       COALESCE(is_active,   1)   AS is_active,
                       COALESCE(is_verified, 1)   AS is_verified
                FROM users WHERE email = @email LIMIT 1";
            cmd.Parameters.AddWithValue("@email", email);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapUser(r) : null;
        }

        // Alle actieve gebruikers (gebande worden niet getoond)
        public static List<User> GetAll()
        {
            var list = new List<User>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, firstname, lastname, email, study, bio, created_at,
                       COALESCE(username,'')      AS username,
                       COALESCE(is_admin,    0)   AS is_admin,
                       COALESCE(profile_photo,'') AS profile_photo,
                       COALESCE(is_active,   1)   AS is_active,
                       COALESCE(is_verified, 1)   AS is_verified
                FROM users
                WHERE COALESCE(is_active, 1) = 1
                ORDER BY firstname";
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapUser(r));
            return list;
        }

        // Alle gebruikers inclusief gebande (alleen voor adminpanel)
        public static List<User> GetAllInclusingBanned()
        {
            var list = new List<User>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, firstname, lastname, email, study, bio, created_at,
                       COALESCE(username,'')      AS username,
                       COALESCE(is_admin,    0)   AS is_admin,
                       COALESCE(profile_photo,'') AS profile_photo,
                       COALESCE(is_active,   1)   AS is_active,
                       COALESCE(is_verified, 1)   AS is_verified
                FROM users ORDER BY firstname";
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapUser(r));
            return list;
        }

        // Profiel bijwerken
        public static void UpdateProfiel(int id, string firstname, string lastname,
                                          string email, string study, string bio,
                                          string username = "", string profilePhoto = "")
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE users
                SET firstname=@fn, lastname=@ln, email=@email,
                    study=@study, bio=@bio, username=@un, profile_photo=@photo
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

        // Wachtwoord wijzigen (admin)
        public static void UpdateWachtwoord(int id, string plainPassword)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "UPDATE users SET PASSWORD=@pw WHERE id=@id";
            cmd.Parameters.AddWithValue("@pw", HashPassword(plainPassword));
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // Wachtwoord wijzigen (eigen profiel): controleert huidig wachtwoord
        public static bool WijzigEigenWachtwoord(int id, string huidig, string nieuw)
        {
            using var conn = DbConnection.GetConnection();
            using var chk  = conn.CreateCommand();
            chk.CommandText = "SELECT COUNT(*) FROM users WHERE id=@id AND PASSWORD=@pw";
            chk.Parameters.AddWithValue("@id", id);
            chk.Parameters.AddWithValue("@pw", HashPassword(huidig));
            if (Convert.ToInt32(chk.ExecuteScalar()) == 0) return false;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE users SET PASSWORD=@pw WHERE id=@id";
            cmd.Parameters.AddWithValue("@pw", HashPassword(nieuw));
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return true;
        }

        // Account verwijderen: verwijdert ook alle gerelateerde data
        public static void Verwijder(int id)
        {
            using var conn = DbConnection.GetConnection();
            var queries = new[]
            {
                "DELETE FROM likes              WHERE user_id=@id",
                "DELETE FROM comments           WHERE user_id=@id",
                "DELETE FROM event_participants WHERE user_id=@id",
                "DELETE FROM friendships        WHERE sender_id=@id OR receiver_id=@id",
                "DELETE FROM group_members      WHERE user_id=@id",
                "DELETE FROM posts              WHERE user_id=@id",
                "DELETE FROM events             WHERE creator_id=@id",
                "DELETE FROM users              WHERE id=@id"
            };
            foreach (var sql in queries)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@id", id);
                try { cmd.ExecuteNonQuery(); } catch { }
            }

            if (Models.AppState.Instance.HuidigeGebruiker?.Id == id)
                Models.AppState.Instance.HuidigeGebruiker = null;
        }

        // Gebruiker bannen: zet is_active=0 en verwijdert hun content
        public static void BanGebruiker(int id)
        {
            using var conn = DbConnection.GetConnection();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE users SET is_active=0 WHERE id=@id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            // Verwijder posts van deze user
            var postIds = new List<int>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id FROM posts WHERE user_id=@id";
                cmd.Parameters.AddWithValue("@id", id);
                using var r = cmd.ExecuteReader();
                while (r.Read()) postIds.Add(r.GetInt32(0));
            }
            foreach (int pid in postIds)
            {
                foreach (var sql in new[] {
                    "DELETE FROM likes    WHERE post_id=@pid",
                    "DELETE FROM comments WHERE post_id=@pid",
                    "DELETE FROM posts    WHERE id=@pid"
                })
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@pid", pid);
                    try { cmd.ExecuteNonQuery(); } catch { }
                }
            }

            // Verwijder evenementen van deze user
            var eventIds = new List<int>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id FROM events WHERE creator_id=@id";
                cmd.Parameters.AddWithValue("@id", id);
                using var r = cmd.ExecuteReader();
                while (r.Read()) eventIds.Add(r.GetInt32(0));
            }
            foreach (int eid in eventIds)
            {
                foreach (var sql in new[] {
                    "DELETE FROM event_participants WHERE event_id=@eid",
                    "DELETE FROM events             WHERE id=@eid"
                })
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@eid", eid);
                    try { cmd.ExecuteNonQuery(); } catch { }
                }
            }
        }

        // Gebruiker deblokkeren
        public static void UnbanGebruiker(int id)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "UPDATE users SET is_active=1 WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // Checkt of een e-mailadres al bestaat
        public static bool EmailBestaat(string email)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM users WHERE email = @email";
            cmd.Parameters.AddWithValue("@email", email);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        // Zet een database-rij om naar een User-object
        private static User MapUser(MySqlDataReader r)
        {
            bool isAdmin    = SafeGetBool(r, "is_admin");
            bool isActive   = SafeGetBool(r, "is_active",   defaultVal: true);
            bool isVerified = SafeGetBool(r, "is_verified", defaultVal: true);
            string username = SafeGetString(r, "username");
            string photo    = SafeGetString(r, "profile_photo");

            return new User
            {
                Id             = r.GetInt32("id"),
                Firstname      = r.GetString("firstname"),
                Lastname       = r.GetString("lastname"),
                Email          = r.GetString("email"),
                Study          = SafeGetString(r, "study"),
                Bio            = SafeGetString(r, "bio"),
                CreatedAt      = r.GetDateTime("created_at"),
                Username       = username,
                Role           = isAdmin ? "Admin" : "User",
                IsAdmin        = isAdmin,
                IsActive       = isActive,
                IsVerified     = isVerified,
                ProfielFotoPad = photo
            };
        }

        private static bool SafeGetBool(MySqlDataReader r, string col, bool defaultVal = false)
        {
            try { return r.GetInt32(col) == 1; } catch { return defaultVal; }
        }

        private static string SafeGetString(MySqlDataReader r, string col)
        {
            try { return r.IsDBNull(r.GetOrdinal(col)) ? "" : r.GetString(col); } catch { return ""; }
        }
    }
}
