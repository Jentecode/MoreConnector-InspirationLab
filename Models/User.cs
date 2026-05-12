using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MoreConnector.Models
{
    /// <summary>
    /// Centrale gebruikersklasse — combineert DB-velden (Engels) met
    /// UI-properties (Nederlands) zodat de rest van de app niet hoeft te veranderen.
    /// </summary>
    public class User : INotifyPropertyChanged
    {
        // ── DB kolommen ──────────────────────────────────────────────────────
        public int      Id        { get; set; }
        public string   Firstname { get; set; } = "";
        public string   Lastname  { get; set; } = "";
        public string   Email     { get; set; } = "";
        public string   Password  { get; set; } = "";
        public string   Study     { get; set; } = "";
        public string   Bio       { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        // ── Nederlandse aliassen (gebruikt door views) ───────────────────────
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

        // ── Extra UI-velden (niet in DB, lokaal bijgehouden) ─────────────────
        private string _username = "";
        public string Username
        {
            get => _username;
            set { _username = value; OnChanged(nameof(Username)); OnChanged(nameof(DisplayNaam)); }
        }

        public string Role            { get; set; } = "User";
        public string ProfielFotoPad  { get; set; } = "";
        public string Telefoonnummer  { get; set; } = "";
        public string WachtwoordHash  { get; set; } = "";
        public bool   IsBanned        { get; set; }
        public List<string> Tags      { get; } = new();

        // ── Berekende properties ─────────────────────────────────────────────
        public string VolledigeNaam => $"{Firstname} {Lastname}".Trim();

        public string DisplayNaam =>
            !string.IsNullOrWhiteSpace(Username) ? $"@{Username}" : VolledigeNaam;

        // ── INotifyPropertyChanged ───────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
