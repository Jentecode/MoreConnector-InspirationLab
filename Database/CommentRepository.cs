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
                       CONCAT(u.firstname, ' ', u.lastname) AS author
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
            cmd.CommandText = @"
                INSERT INTO comments (post_id, user_id, content)
                VALUES (@pid, @uid, @content);
                SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@pid",     postId);
            cmd.Parameters.AddWithValue("@uid",     userId);
            cmd.Parameters.AddWithValue("@content", content);
            return Convert.ToInt32(cmd.ExecuteScalar());
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
