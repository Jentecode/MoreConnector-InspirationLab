using MoreConnector.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace MoreConnector.Database
{
    public static class MessageRepository
    {
        /// <summary>Geeft het gesprek tussen twee gebruikers, chronologisch.</summary>
        public static List<Message> GetGesprek(int userId1, int userId2)
        {
            var list = new List<Message>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT m.id, m.sender_id, m.receiver_id, m.message, m.sent_at,
                       CONCAT(s.firstname,' ',s.lastname) AS sender_name,
                       CONCAT(r.firstname,' ',r.lastname) AS receiver_name,
                       COALESCE(s.profile_photo,'') AS sender_photo
                FROM   messages m
                JOIN   users s ON s.id = m.sender_id AND COALESCE(s.is_active, 1) = 1
                JOIN   users r ON r.id = m.receiver_id AND COALESCE(r.is_active, 1) = 1
                WHERE  (m.sender_id=@u1 AND m.receiver_id=@u2)
                    OR (m.sender_id=@u2 AND m.receiver_id=@u1)
                ORDER BY m.sent_at ASC";
            cmd.Parameters.AddWithValue("@u1", userId1);
            cmd.Parameters.AddWithValue("@u2", userId2);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Message
                {
                    Id           = r.GetInt32("id"),
                    SenderId     = r.GetInt32("sender_id"),
                    ReceiverId   = r.GetInt32("receiver_id"),
                    Content      = r.IsDBNull(r.GetOrdinal("message")) ? "" : r.GetString("message"),
                    SentAt       = r.GetDateTime("sent_at"),
                    SenderName   = r.GetString("sender_name"),
                    ReceiverName = r.GetString("receiver_name"),
                    IsOwn        = r.GetInt32("sender_id") == userId1,
                    SenderPhoto  = r.IsDBNull(r.GetOrdinal("sender_photo")) ? "" : r.GetString("sender_photo")
                });
            }
            return list;
        }

        /// <summary>Geeft een lijst van contacten met wie de gebruiker berichten heeft gewisseld.</summary>
        public static List<User> GetContacten(int userId)
        {
            var list = new List<User>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT DISTINCT u.id, u.firstname, u.lastname, u.email, u.study, u.bio, u.created_at
                FROM   messages m
                JOIN   users u ON u.id = CASE WHEN m.sender_id=@uid THEN m.receiver_id ELSE m.sender_id END
                           AND COALESCE(u.is_active, 1) = 1
                WHERE  m.sender_id=@uid OR m.receiver_id=@uid
                ORDER BY u.firstname";
            cmd.Parameters.AddWithValue("@uid", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new User
                {
                    Id        = r.GetInt32("id"),
                    Firstname = r.GetString("firstname"),
                    Lastname  = r.GetString("lastname"),
                    Email     = r.GetString("email"),
                    Study     = r.IsDBNull(r.GetOrdinal("study")) ? "" : r.GetString("study"),
                    Bio       = r.IsDBNull(r.GetOrdinal("bio"))   ? "" : r.GetString("bio"),
                    CreatedAt = r.GetDateTime("created_at")
                });
            }
            return list;
        }

        public static int Verstuur(int senderId, int receiverId, string content)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO messages (sender_id, receiver_id, message)
                VALUES (@sid, @rid, @msg);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@sid", senderId);
            cmd.Parameters.AddWithValue("@rid", receiverId);
            cmd.Parameters.AddWithValue("@msg", content);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
