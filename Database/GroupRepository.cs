using MoreConnector.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace MoreConnector.Database
{
    public static class GroupRepository
    {
        // ── Groep aanmaken ────────────────────────────────────────────────────
        public static int MaakGroep(int creatorId, string naam, string beschrijving = "")
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO `groups` (creator_id, name, description)
                VALUES (@cid, @naam, @desc);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@cid",  creatorId);
            cmd.Parameters.AddWithValue("@naam", naam);
            cmd.Parameters.AddWithValue("@desc", beschrijving);
            int groupId = Convert.ToInt32(cmd.ExecuteScalar());

            // Creator automatisch toevoegen
            VoegLidToe(groupId, creatorId);
            return groupId;
        }

        // ── Lid toevoegen ─────────────────────────────────────────────────────
        public static void VoegLidToe(int groupId, int userId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT IGNORE INTO group_members (group_id, user_id)
                VALUES (@gid, @uid)";
            cmd.Parameters.AddWithValue("@gid", groupId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.ExecuteNonQuery();
        }

        // ── Groepen van gebruiker ─────────────────────────────────────────────
        public static List<ChatGroep> GetGroepenVanGebruiker(int userId)
        {
            var list = new List<ChatGroep>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT g.id, g.name, g.description, g.creator_id,
                       COUNT(gm.id) AS member_count
                FROM   `groups` g
                JOIN   group_members gm2 ON gm2.group_id = g.id AND gm2.user_id = @uid
                LEFT JOIN group_members gm ON gm.group_id = g.id
                GROUP BY g.id
                ORDER BY g.name";
            cmd.Parameters.AddWithValue("@uid", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new ChatGroep
                {
                    Id          = r.GetInt32("id"),
                    Naam        = r.GetString("name"),
                    Beschrijving = r.IsDBNull(r.GetOrdinal("description")) ? "" : r.GetString("description"),
                    CreatorId   = r.GetInt32("creator_id"),
                    AantalLeden = r.GetInt32("member_count")
                });
            }
            return list;
        }

        // ── Leden ophalen ─────────────────────────────────────────────────────
        public static List<User> GetLeden(int groupId)
        {
            var list = new List<User>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT u.id, u.firstname, u.lastname, u.username, u.email, u.study, u.bio, u.created_at
                FROM   group_members gm
                JOIN   users u ON u.id = gm.user_id
                WHERE  gm.group_id = @gid
                ORDER BY u.firstname";
            cmd.Parameters.AddWithValue("@gid", groupId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new User
                {
                    Id        = r.GetInt32("id"),
                    Firstname = r.GetString("firstname"),
                    Lastname  = r.GetString("lastname"),
                    Username  = r.IsDBNull(r.GetOrdinal("username")) ? "" : r.GetString("username"),
                    Email     = r.GetString("email"),
                    Study     = r.IsDBNull(r.GetOrdinal("study")) ? "" : r.GetString("study"),
                    Bio       = r.IsDBNull(r.GetOrdinal("bio"))   ? "" : r.GetString("bio"),
                    CreatedAt = r.GetDateTime("created_at")
                });
            return list;
        }

        // ── Groepsberichten ───────────────────────────────────────────────────
        public static List<ChatBericht> GetBerichten(int groupId)
        {
            var list = new List<ChatBericht>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT gm.id, gm.sender_id, gm.message, gm.sent_at,
                       CONCAT(u.firstname,' ',u.lastname) AS sender_name,
                       COALESCE(u.username, '') AS sender_username
                FROM   group_messages gm
                JOIN   users u ON u.id = gm.sender_id
                WHERE  gm.group_id = @gid
                ORDER BY gm.sent_at ASC";
            cmd.Parameters.AddWithValue("@gid", groupId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new ChatBericht
                {
                    Id           = r.GetInt32("id"),
                    SenderId     = r.GetInt32("sender_id"),
                    Tekst        = r.GetString("message"),
                    SentAt       = r.GetDateTime("sent_at"),
                    SenderNaam   = !string.IsNullOrEmpty(r.GetString("sender_username"))
                                   ? $"@{r.GetString("sender_username")}"
                                   : r.GetString("sender_name")
                });
            return list;
        }

        public static int StuurBericht(int groupId, int senderId, string message)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO group_messages (group_id, sender_id, message)
                VALUES (@gid, @sid, @msg);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@gid", groupId);
            cmd.Parameters.AddWithValue("@sid", senderId);
            cmd.Parameters.AddWithValue("@msg", message);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
