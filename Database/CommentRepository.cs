using MoreConnector.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace MoreConnector.Database
{
    public static class CommentRepository
    {
        public static List<Comment> GetVanPost(int postId, int huidigeUserId = 0)
        {
            var list = new List<Comment>();
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT c.id, c.post_id, c.user_id, c.content, c.created_at,
                       CASE WHEN u.username IS NOT NULL AND u.username != '' THEN u.username ELSE CONCAT(u.firstname, ' ', u.lastname) END AS author
                FROM   comments c
                JOIN   users u ON u.id = c.user_id
                WHERE  c.post_id = @pid
                ORDER BY c.created_at ASC";
            cmd.Parameters.AddWithValue("@pid", postId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Comment
                {
                    Id         = r.GetInt32("id"),
                    PostId     = r.GetInt32("post_id"),
                    UserId     = r.GetInt32("user_id"),
                    Content    = r.IsDBNull(r.GetOrdinal("content")) ? "" : r.GetString("content"),
                    CreatedAt  = r.GetDateTime("created_at"),
                    AuthorName = r.IsDBNull(r.GetOrdinal("author"))  ? "" : r.GetString("author")
                });
            }
            return list;
        }

        public static int Toevoegen(int postId, int userId, string content)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO comments (post_id, user_id, content) VALUES (@pid, @uid, @content)";
            cmd.Parameters.AddWithValue("@pid",     postId);
            cmd.Parameters.AddWithValue("@uid",     userId);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.ExecuteNonQuery();

            using var id  = conn.CreateCommand();
            id.CommandText = "SELECT LAST_INSERT_ID()";
            return Convert.ToInt32(id.ExecuteScalar());
        }

        public static bool ToggleLike(int commentId, int userId)
        {
            // Zorg dat de tabel bestaat
            using (var conn = DbConnection.GetConnection())
            using (var mk = conn.CreateCommand())
            {
                mk.CommandText = @"CREATE TABLE IF NOT EXISTS comment_likes (
                    comment_id INT NOT NULL,
                    user_id    INT NOT NULL,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (comment_id, user_id)
                )";
                mk.ExecuteNonQuery();
            }

            bool alGeliked;
            using (var conn = DbConnection.GetConnection())
            using (var chk  = conn.CreateCommand())
            {
                chk.CommandText = "SELECT COUNT(*) FROM comment_likes WHERE comment_id=@cid AND user_id=@uid";
                chk.Parameters.AddWithValue("@cid", commentId);
                chk.Parameters.AddWithValue("@uid", userId);
                alGeliked = Convert.ToInt32(chk.ExecuteScalar()) > 0;
            }

            using (var conn = DbConnection.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                if (alGeliked)
                {
                    cmd.CommandText = "DELETE FROM comment_likes WHERE comment_id=@cid AND user_id=@uid";
                    cmd.Parameters.AddWithValue("@cid", commentId);
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.ExecuteNonQuery();
                    return false;
                }
                else
                {
                    cmd.CommandText = "INSERT INTO comment_likes (comment_id, user_id) VALUES (@cid, @uid)";
                    cmd.Parameters.AddWithValue("@cid", commentId);
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        public static int GetLikeCount(int commentId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM comment_likes WHERE comment_id=@cid";
            cmd.Parameters.AddWithValue("@cid", commentId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static bool IsLikedByUser(int commentId, int userId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM comment_likes WHERE comment_id=@cid AND user_id=@uid";
            cmd.Parameters.AddWithValue("@cid", commentId);
            cmd.Parameters.AddWithValue("@uid", userId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public static void Verwijder(int commentId)
        {
            using var conn = DbConnection.GetConnection();
            using var cmd  = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM comments WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", commentId);
            cmd.ExecuteNonQuery();
        }
    }
}
