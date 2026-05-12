using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MoreConnector.Models
{
    // AdminGebruiker = type alias voor User zodat bestaande Admin.xaml bindings blijven werken
    using AdminGebruiker = User;

    public class AdminPost : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private string _auteur = "";
        public string Auteur { get => _auteur; set { _auteur = value; OnChanged(nameof(Auteur)); } }
        private string _beschrijving = "";
        public string Beschrijving { get => _beschrijving; set { _beschrijving = value; OnChanged(nameof(Beschrijving)); } }
        private string _datumTekst = "";
        public string DatumTekst { get => _datumTekst; set { _datumTekst = value; OnChanged(nameof(DatumTekst)); } }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class AdminEvenement : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private string _naam = "";
        public string Naam { get => _naam; set { _naam = value; OnChanged(nameof(Naam)); } }
        private string _locatie = "";
        public string Locatie { get => _locatie; set { _locatie = value; OnChanged(nameof(Locatie)); } }
        private string _datumTekst = "";
        public string DatumTekst { get => _datumTekst; set { _datumTekst = value; OnChanged(nameof(DatumTekst)); } }
        public string Beschrijving  { get; set; } = "";
        public string Auteur        { get; set; } = "";
        public string AfbeeldingPad { get; set; } = "";
        public ObservableCollection<string> Deelnemers { get; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
