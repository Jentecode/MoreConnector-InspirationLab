using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MoreConnector.Models
{
    public class User : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Firstname { get; set; } = "";
        public string Lastname { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Study { get; set; } = "";
        public string Bio { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsAdmin { get; set; }  // is_admin kolom in DB
        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnChanged(nameof(IsActive)); }
        }
        public string Voornaam
        {
            get => Firstname;
            set { Firstname = value; OnChanged(nameof(Voornaam)); OnChanged(nameof(VolledigeNaam)); OnChanged(nameof(DisplayNaam)); }
        }
        public string Achternaam
        {
            get => Lastname;
            set { Lastname = value; OnChanged(nameof(Achternaam)); OnChanged(nameof(VolledigeNaam)); OnChanged(nameof(DisplayNaam)); }
        }
        public string Studierichting
        {
            get => Study;
            set { Study = value; OnChanged(nameof(Studierichting)); }
        }
        private string _username = "";
        public string Username
        {
            get => _username;
            set { _username = value; OnChanged(nameof(Username)); OnChanged(nameof(DisplayNaam)); }
        }

        private string _role = "User";
        public string Role
        {
            get => _role;
            set { _role = value; OnChanged(nameof(Role)); }
        }

        public string ProfielFotoPad { get; set; } = "";
        public string Telefoonnummer { get; set; } = "";
        public string WachtwoordHash { get; set; } = "";
        public bool IsBanned { get; set; }
        public bool IsVerified { get; set; } = true;
        public List<string> Tags { get; } = new();
        public string VolledigeNaam => $"{Firstname} {Lastname}".Trim();

        // DisplayNaam: altijd username als die beschikbaar is, anders volledige naam
        public string DisplayNaam =>
            !string.IsNullOrWhiteSpace(Username) ? Username : VolledigeNaam;
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}