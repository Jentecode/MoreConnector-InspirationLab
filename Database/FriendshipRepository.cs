using MoreConnector.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace MoreConnector.Database
{
    public static class FriendshipRepository
    {
        // ── Vriendschapsverzoek sturen ────────────────────────────────────────
        public static void StuurVerzoek(int senderId, int receiverId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT IGNORE INTO friendships (sender_id, receiver_id, status)
                VALUES (@sid, @rid, 'pending')";
            cmd.Parameters.AddWithValue("@sid", senderId);
            cmd.Parameters.AddWithValue("@rid", receiverId);
            cmd.ExecuteNonQuery();
        }

        // ── Verzoek accepteren ────────────────────────────────────────────────
        public static void AccepteerVerzoek(int friendshipId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "UPDATE friendships SET status='accepted' WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", friendshipId);
            cmd.ExecuteNonQuery();
        }

        // ── Verzoek weigeren ──────────────────────────────────────────────────
        public static void WeigerVerzoek(int friendshipId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "UPDATE friendships SET status='declined' WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", friendshipId);
            cmd.ExecuteNonQuery();
        }

        // ── Vrienden ophalen ──────────────────────────────────────────────────
        public static List<User> GetVrienden(int userId)
        {
            var list = new List<User>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT u.id, u.firstname, u.lastname, u.username, u.email, u.study, u.bio, u.created_at
                FROM   friendships f
                JOIN   users u ON u.id = CASE WHEN f.sender_id=@uid THEN f.receiver_id ELSE f.sender_id END
                WHERE  (f.sender_id=@uid OR f.receiver_id=@uid)
                AND    f.status = 'accepted'
                ORDER BY u.firstname";
            cmd.Parameters.AddWithValue("@uid", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapUser(r));
            return list;
        }

        // ── Binnenkomende verzoeken ───────────────────────────────────────────
        public static List<FriendRequest> GetBinnenkomendeVerzoeken(int userId)
        {
            var list = new List<FriendRequest>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT f.id, f.sender_id, f.created_at,
                       u.firstname, u.lastname, u.username, u.email, u.study, u.bio
                FROM   friendships f
                JOIN   users u ON u.id = f.sender_id
                WHERE  f.receiver_id = @uid AND f.status = 'pending'
                ORDER BY f.created_at DESC";
            cmd.Parameters.AddWithValue("@uid", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new FriendRequest
                {
                    Id        = r.GetInt32("id"),
                    SenderId  = r.GetInt32("sender_id"),
                    CreatedAt = r.GetDateTime("created_at"),
                    Sender    = new User
                    {
                        Id        = r.GetInt32("sender_id"),
                        Firstname = r.GetString("firstname"),
                        Lastname  = r.GetString("lastname"),
                        Username  = r.IsDBNull(r.GetOrdinal("username")) ? "" : r.GetString("username"),
                        Email     = r.GetString("email"),
                        Study     = r.IsDBNull(r.GetOrdinal("study")) ? "" : r.GetString("study"),
                        Bio       = r.IsDBNull(r.GetOrdinal("bio"))   ? "" : r.GetString("bio")
                    }
                });
            }
            return list;
        }

        // ── Status opvragen ───────────────────────────────────────────────────
        public static string GetStatus(int userId, int otherUserId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT status FROM friendships
                WHERE (sender_id=@u1 AND receiver_id=@u2)
                   OR (sender_id=@u2 AND receiver_id=@u1)
                LIMIT 1";
            cmd.Parameters.AddWithValue("@u1", userId);
            cmd.Parameters.AddWithValue("@u2", otherUserId);
            var res = cmd.ExecuteScalar();
            return res?.ToString() ?? "none";
        }

        // ── Verwijder vriendschap ─────────────────────────────────────────────
        public static void VerwijderVriendschap(int userId, int otherUserId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM friendships
                WHERE (sender_id=@u1 AND receiver_id=@u2)
                   OR (sender_id=@u2 AND receiver_id=@u1)";
            cmd.Parameters.AddWithValue("@u1", userId);
            cmd.Parameters.AddWithValue("@u2", otherUserId);
            cmd.ExecuteNonQuery();
        }

        // ── Alle verzoeken tellen (badge) ─────────────────────────────────────
        public static int GetAantalOpenVerzoeken(int userId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM friendships WHERE receiver_id=@uid AND status='pending'";
            cmd.Parameters.AddWithValue("@uid", userId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static User MapUser(MySqlDataReader r) => new()
        {
            Id        = r.GetInt32("id"),
            Firstname = r.GetString("firstname"),
            Lastname  = r.GetString("lastname"),
            Username  = r.IsDBNull(r.GetOrdinal("username")) ? "" : r.GetString("username"),
            Email     = r.GetString("email"),
            Study     = r.IsDBNull(r.GetOrdinal("study")) ? "" : r.GetString("study"),
            Bio       = r.IsDBNull(r.GetOrdinal("bio"))   ? "" : r.GetString("bio"),
            CreatedAt = r.GetDateTime("created_at")
        };
    }
}
