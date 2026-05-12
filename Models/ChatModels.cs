using System;
using System.Collections.ObjectModel;

namespace MoreConnector.Models
{
    public class ChatGroep
    {
        public int    Id           { get; set; }
        public string Naam         { get; set; } = "";
        public string Beschrijving { get; set; } = "";
        public int    CreatorId    { get; set; }
        public int    AantalLeden  { get; set; }
    }

    public class ChatBericht
    {
        public int      Id         { get; set; }
        public int      SenderId   { get; set; }
        public string   Tekst      { get; set; } = "";
        public DateTime SentAt     { get; set; }
        public string   SenderNaam { get; set; } = "";
        public bool     IsEigen    { get; set; }
    }

    public class DmContact
    {
        public int    UserId      { get; set; }
        public string Naam        { get; set; } = "";
        public string LastMessage { get; set; } = "";
    }
}
