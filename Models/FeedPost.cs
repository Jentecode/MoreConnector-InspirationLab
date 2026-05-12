using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MoreConnector.Models
{
    public class FeedPost
    {
        public int    DbId          { get; set; }
        public int    UserId        { get; set; }
        public string AuteurNaam    { get; set; } = "";
        public string Beschrijving  { get; set; } = "";
        public string AfbeeldingPad { get; set; } = "";
        public int    LikeCount     { get; set; }
        public bool   LikedByMe     { get; set; }
        public List<string>                  LikedDoor { get; } = new();
        public ObservableCollection<FeedReactie> Reacties { get; } = new();
    }

    public class FeedReactie
    {
        public int    DbCommentId   { get; set; }
        public string AuteurNaam    { get; set; } = "";
        public string AuteurFotoPad { get; set; } = "";
        public string Tekst         { get; set; } = "";
        public bool   IsReply       { get; set; }
        public List<string> LikedDoor { get; } = new();
    }
}
