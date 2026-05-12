using System;

namespace MoreConnector.Models
{
    /// <summary>Komt overeen met de `comments` tabel.</summary>
    public class Comment
    {
        public int      Id        { get; set; }
        public int      PostId    { get; set; }
        public int      UserId    { get; set; }
        public string   Content   { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        // Ingevuld door JOIN
        public string AuthorName { get; set; } = "";
        public int    LikeCount  { get; set; }
        public bool   LikedByMe  { get; set; }
    }
}
