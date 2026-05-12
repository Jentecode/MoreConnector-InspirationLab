using System;

namespace MoreConnector.Models
{
    public class FriendRequest
    {
        public int      Id        { get; set; }
        public int      SenderId  { get; set; }
        public User     Sender    { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
