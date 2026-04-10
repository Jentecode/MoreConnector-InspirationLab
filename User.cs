using System;

namespace MoreConnector
{
    public class User
    {
        public int User_ID { get; set; }
        public string User_Name { get; set; }
        public string User_IMG { get; set; }
        public string Password_Hash { get; set; }
        public string Email { get; set; }
        public string User_Richting { get; set; }
        public bool Is_Admin { get; set; }
        public DateTime Created_At { get; set; }
        public bool Is_Active { get; set; }

        public User(int user_ID, string user_Name, string user_IMG, string password_Hash,
                    string email, string user_Richting, bool is_Admin, bool is_Active)
        {
            User_ID = user_ID;
            User_Name = user_Name;
            User_IMG = user_IMG;
            Password_Hash = password_Hash;
            Email = email;
            User_Richting = user_Richting;
            Is_Admin = is_Admin;
            Created_At = DateTime.Now;
            Is_Active = is_Active;
        }

        public void Edit_Account(string nieuwNaam, string nieuwEmail)
        {
            User_Name = nieuwNaam;
            Email = nieuwEmail;
        }

        public void Delete_Account()
        {
            Is_Active = false;
        }

        public void Add_Like()
        {
            // logica voor like toevoegen
        }

        public void Remove_Like()
        {
            // logica voor like verwijderen
        }

        public void Report()
        {
            // logica voor rapporteren
        }

        public override string ToString()
        {
            return $"User {User_ID}: {User_Name} - Email: {Email} - Richting: {User_Richting} - Admin: {Is_Admin}\n";
        }
    }
}