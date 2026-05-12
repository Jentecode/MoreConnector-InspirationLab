using System;

namespace MoreConnector.Models
{
    /// <summary>Komt overeen met de `messages` tabel.</summary>
    public class Message
    {
        public int      Id         { get; set; }
        public int      SenderId   { get; set; }
        public int      ReceiverId { get; set; }
        public string   Content    { get; set; } = "";
        public DateTime SentAt     { get; set; }

        // Ingevuld door JOIN
        public string SenderName   { get; set; } = "";
        public string ReceiverName { get; set; } = "";
        public bool   IsOwn        { get; set; }   // true als sender == ingelogde gebruiker
    }
}
