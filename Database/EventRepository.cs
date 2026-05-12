using MoreConnector.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace MoreConnector.Database
{
    public static class EventRepository
    {
        // ── Ophalen ───────────────────────────────────────────────────────────
        public static List<Event> GetAll(int huidigeUserId = 0)
        {
            var list = new List<Event>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT e.id, e.creator_id, e.title, e.description, e.location,
                       e.event_date, e.max_participants, e.created_at,
                       CONCAT(u.firstname,' ',u.lastname) AS creator_name,
                       COUNT(DISTINCT ep.id)              AS participant_count,
                       MAX(CASE WHEN ep.user_id=@uid THEN 1 ELSE 0 END) AS joined_by_me
                FROM   events e
                JOIN   users u ON u.id = e.creator_id
                LEFT JOIN event_participants ep ON ep.event_id = e.id
                GROUP BY e.id
                ORDER BY e.event_date ASC";
            cmd.Parameters.AddWithValue("@uid", huidigeUserId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapEvent(r));
            return list;
        }

        public static Event? GetById(int eventId, int huidigeUserId = 0)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT e.id, e.creator_id, e.title, e.description, e.location,
                       e.event_date, e.max_participants, e.created_at,
                       CONCAT(u.firstname,' ',u.lastname) AS creator_name,
                       COUNT(DISTINCT ep.id) AS participant_count,
                       MAX(CASE WHEN ep.user_id=@uid THEN 1 ELSE 0 END) AS joined_by_me
                FROM   events e
                JOIN   users u ON u.id = e.creator_id
                LEFT JOIN event_participants ep ON ep.event_id = e.id
                WHERE  e.id = @eid
                GROUP BY e.id";
            cmd.Parameters.AddWithValue("@uid", huidigeUserId);
            cmd.Parameters.AddWithValue("@eid", eventId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            var ev = MapEvent(r);
            r.Close();
            ev.ParticipantNames = GetDeelnemerNamen(eventId, conn);
            return ev;
        }

        // ── Aanmaken ──────────────────────────────────────────────────────────
        public static int Aanmaken(int creatorId, string title, string description,
                                    string location, DateTime eventDate, int maxParticipants = 0)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO events (creator_id, title, description, location, event_date, max_participants)
                VALUES (@cid, @title, @desc, @loc, @date, @max);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@cid",   creatorId);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@desc",  description);
            cmd.Parameters.AddWithValue("@loc",   location);
            cmd.Parameters.AddWithValue("@date",  eventDate);
            cmd.Parameters.AddWithValue("@max",   maxParticipants);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ── Verwijderen ───────────────────────────────────────────────────────
        public static void Verwijder(int eventId)
        {
            using var conn = DbConnection.GetConnection();
            foreach (var sql in new[]
            {
                "DELETE FROM event_participants WHERE event_id=@id",
                "DELETE FROM events             WHERE id=@id"
            })
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@id", eventId);
                cmd.ExecuteNonQuery();
            }
        }

        // ── Deelnemen ─────────────────────────────────────────────────────────
        public static bool Inschrijven(int eventId, int userId)
        {
            using var conn = DbConnection.GetConnection();

            // Al ingeschreven?
            using (var chk = conn.CreateCommand())
            {
                chk.CommandText = "SELECT COUNT(*) FROM event_participants WHERE event_id=@eid AND user_id=@uid";
                chk.Parameters.AddWithValue("@eid", eventId);
                chk.Parameters.AddWithValue("@uid", userId);
                if (Convert.ToInt32(chk.ExecuteScalar()) > 0) return false;
            }

            // Max bereikt?
            using (var mx = conn.CreateCommand())
            {
                mx.CommandText = @"SELECT e.max_participants, COUNT(ep.id)
                                   FROM events e
                                   LEFT JOIN event_participants ep ON ep.event_id=e.id
                                   WHERE e.id=@eid GROUP BY e.id";
                mx.Parameters.AddWithValue("@eid", eventId);
                using var mr = mx.ExecuteReader();
                if (mr.Read())
                {
                    int max   = mr.GetInt32(0);
                    int count = mr.GetInt32(1);
                    if (max > 0 && count >= max) return false;
                }
            }

            using var ins = conn.CreateCommand();
            ins.CommandText = "INSERT INTO event_participants (event_id, user_id) VALUES (@eid, @uid)";
            ins.Parameters.AddWithValue("@eid", eventId);
            ins.Parameters.AddWithValue("@uid", userId);
            ins.ExecuteNonQuery();
            return true;
        }

        public static void Uitschrijven(int eventId, int userId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM event_participants WHERE event_id=@eid AND user_id=@uid";
            cmd.Parameters.AddWithValue("@eid", eventId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.ExecuteNonQuery();
        }

        // ── Deelnemers namen ──────────────────────────────────────────────────
        public static List<string> GetDeelnemerNamen(int eventId, MySqlConnection? conn = null)
        {
            var list = new List<string>();
            bool owned = conn == null;
            conn ??= DbConnection.GetConnection();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT CONCAT(u.firstname,' ',u.lastname)
                    FROM   event_participants ep
                    JOIN   users u ON u.id = ep.user_id
                    WHERE  ep.event_id = @eid";
                cmd.Parameters.AddWithValue("@eid", eventId);
                using var r = cmd.ExecuteReader();
                while (r.Read()) list.Add(r.GetString(0));
            }
            finally { if (owned) conn.Dispose(); }
            return list;
        }

        // ── Mapper ────────────────────────────────────────────────────────────
        private static Event MapEvent(MySqlDataReader r) => new()
        {
            Id               = r.GetInt32("id"),
            CreatorId        = r.GetInt32("creator_id"),
            Title            = r.IsDBNull(r.GetOrdinal("title"))       ? "" : r.GetString("title"),
            Description      = r.IsDBNull(r.GetOrdinal("description")) ? "" : r.GetString("description"),
            Location         = r.IsDBNull(r.GetOrdinal("location"))    ? "" : r.GetString("location"),
            EventDate        = r.GetDateTime("event_date"),
            MaxParticipants  = r.IsDBNull(r.GetOrdinal("max_participants")) ? 0 : r.GetInt32("max_participants"),
            CreatedAt        = r.GetDateTime("created_at"),
            CreatorName      = r.IsDBNull(r.GetOrdinal("creator_name")) ? "" : r.GetString("creator_name"),
            ParticipantCount = r.GetInt32("participant_count"),
            JoinedByMe       = r.GetInt32("joined_by_me") == 1
        };
    }
}
