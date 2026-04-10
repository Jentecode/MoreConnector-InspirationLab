using System;

namespace MoreConnector
{
    public class Event
    {
        public int Event_ID { get; set; }
        public int User_ID { get; set; }
        public string Event_Name { get; set; }
        public string Event_IMG { get; set; }
        public DateTime Event_Date { get; set; }
        public string Event_Location { get; set; }
        public string Event_Description { get; set; }
        public DateTime Created_At { get; set; }
        public bool Is_Active { get; set; }

        public Event(int event_ID, int user_ID, string event_Name, string event_IMG,
                     DateTime event_Date, string event_Location, string event_Description,
                     bool is_Active)
        {
            Event_ID = event_ID;
            User_ID = user_ID;
            Event_Name = event_Name;
            Event_IMG = event_IMG;
            Event_Date = event_Date;
            Event_Location = event_Location;
            Event_Description = event_Description;
            Created_At = DateTime.Now;
            Is_Active = is_Active;
        }

        public void Organise_Event()
        {
            // logica voor event organiseren
        }

        public void Join_Event(User user)
        {
            // logica voor gebruiker inschrijven
        }

        public void Leave_Event(User user)
        {
            // logica voor gebruiker uitschrijven
        }

        public override string ToString()
        {
            return $"Event {Event_ID}: {Event_Name} - Locatie: {Event_Location} - Datum: {Event_Date:dd/MM/yyyy} - Actief: {Is_Active}\n";
        }
    }
}