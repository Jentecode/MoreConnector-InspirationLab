using System;
using System.Collections.Generic;

namespace MoreConnector.Models
{
    public class Event
    {
        public int      Id              { get; set; }
        public int      CreatorId       { get; set; }
        public string   Title           { get; set; } = "";
        public string   Description     { get; set; } = "";
        public string   Location        { get; set; } = "";
        public DateTime EventDate       { get; set; }
        public int      MaxParticipants { get; set; }
        public DateTime CreatedAt       { get; set; }
        public string   ImagePath       { get; set; } = "";  // FIX: afbeelding pad

        public string       CreatorName       { get; set; } = "";
        public int          ParticipantCount  { get; set; }
        public bool         JoinedByMe        { get; set; }
        public List<string> ParticipantNames  { get; set; } = new();
    }
}
