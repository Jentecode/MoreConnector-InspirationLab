using MoreConnector.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace MoreConnector.Database
{
    public static class PostRepository
    {
        // ── Ophalen ───────────────────────────────────────────────────────────
        public static List<Post> GetAll(int huidigeUserId = 0)
        {
            var list = new List<Post>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT p.id, p.user_id, p.content, p.image_path, p.created_at,
                       CONCAT(u.firstname, ' ', u.lastname) AS author,
                       COUNT(DISTINCT l.id)                 AS like_count,
                       MAX(CASE WHEN l.user_id=@uid THEN 1 ELSE 0 END) AS liked_by_me
                FROM   posts p
                JOIN   users u ON u.id = p.user_id
                LEFT JOIN likes l ON l.post_id = p.id
                GROUP BY p.id
                ORDER BY p.created_at DESC";
            cmd.Parameters.AddWithValue("@uid", huidigeUserId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapPost(r));
            return list;
        }

        public static List<Post> GetVanGebruiker(int userId)
        {
            var list = new List<Post>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT p.id, p.user_id, p.content, p.image_path, p.created_at,
                       CONCAT(u.firstname, ' ', u.lastname) AS author,
                       COUNT(DISTINCT l.id) AS like_count, 0 AS liked_by_me
                FROM   posts p
                JOIN   users u ON u.id = p.user_id
                LEFT JOIN likes l ON l.post_id = p.id
                WHERE  p.user_id = @uid
                GROUP BY p.id
                ORDER BY p.created_at DESC";
            cmd.Parameters.AddWithValue("@uid", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapPost(r));
            return list;
        }

        // ── Aanmaken ──────────────────────────────────────────────────────────
        public static int Aanmaken(int userId, string content, string imagePath = "")
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO posts (user_id, content, image_path)
                VALUES (@uid, @content, @img);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@uid",     userId);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@img",     imagePath);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ── Verwijderen ───────────────────────────────────────────────────────
        public static void Verwijder(int postId)
        {
            using var conn = DbConnection.GetConnection();
            // Verwijder first comments en likes (FK constraints)
            foreach (var sql in new[]
            {
                "DELETE FROM likes    WHERE post_id=@id",
                "DELETE FROM comments WHERE post_id=@id",
                "DELETE FROM posts    WHERE id=@id"
            })
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@id", postId);
                cmd.ExecuteNonQuery();
            }
        }

        // ── Likes ─────────────────────────────────────────────────────────────
        public static bool ToggleLike(int postId, int userId)
        {
            using var conn = DbConnection.GetConnection();

            // Kijk of al geliked
            using (var chk = conn.CreateCommand())
            {
                chk.CommandText = "SELECT COUNT(*) FROM likes WHERE post_id=@pid AND user_id=@uid";
                chk.Parameters.AddWithValue("@pid", postId);
                chk.Parameters.AddWithValue("@uid", userId);
                bool alGeliked = Convert.ToInt32(chk.ExecuteScalar()) > 0;

                using var tog = conn.CreateCommand();
                if (alGeliked)
                {
                    tog.CommandText = "DELETE FROM likes WHERE post_id=@pid AND user_id=@uid";
                    tog.Parameters.AddWithValue("@pid", postId);
                    tog.Parameters.AddWithValue("@uid", userId);
                    tog.ExecuteNonQuery();
                    return false; // unliked
                }
                else
                {
                    tog.CommandText = "INSERT INTO likes (post_id, user_id) VALUES (@pid, @uid)";
                    tog.Parameters.AddWithValue("@pid", postId);
                    tog.Parameters.AddWithValue("@uid", userId);
                    tog.ExecuteNonQuery();
                    return true;  // liked
                }
            }
        }

        public static int GetLikeCount(int postId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM likes WHERE post_id=@pid";
            cmd.Parameters.AddWithValue("@pid", postId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ── Mapper ────────────────────────────────────────────────────────────
        private static Post MapPost(MySqlDataReader r) => new()
        {
            Id         = r.GetInt32("id"),
            UserId     = r.GetInt32("user_id"),
            Content    = r.IsDBNull(r.GetOrdinal("content"))    ? "" : r.GetString("content"),
            ImagePath  = r.IsDBNull(r.GetOrdinal("image_path")) ? "" : r.GetString("image_path"),
            CreatedAt  = r.GetDateTime("created_at"),
            AuthorName = r.IsDBNull(r.GetOrdinal("author"))     ? "" : r.GetString("author"),
            LikeCount  = r.GetInt32("like_count"),
            LikedByMe  = r.GetInt32("liked_by_me") == 1
        };
    }
}
