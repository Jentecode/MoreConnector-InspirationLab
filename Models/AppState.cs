using MoreConnector.Database;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MoreConnector.Models
{
    public sealed class AppState : INotifyPropertyChanged
    {
        private static readonly Lazy<AppState> _instance = new(() => new AppState());
        public static AppState Instance => _instance.Value;
        private AppState() { }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private User? _huidigeGebruiker;
        public User? HuidigeGebruiker
        {
            get => _huidigeGebruiker;
            set { _huidigeGebruiker = value; OnChanged(nameof(HuidigeGebruiker)); OnChanged(nameof(IsAdmin)); }
        }

        public bool IsAdmin => HuidigeGebruiker?.IsAdmin == true || HuidigeGebruiker?.Role == "Admin";

        public ObservableCollection<AdminEvenement> Evenementen { get; } = new();
        public ObservableCollection<AdminPost>      Berichten   { get; } = new();
        public ObservableCollection<User>           Gebruikers  { get; } = new();
        public ObservableCollection<FeedPost>       FeedPosts   { get; } = new();

        public void LaadAlles()
        {
            LaadPosts();
            LaadEvenementen();
            LaadGebruikers();
        }

        public void LaadPosts()
        {
            Berichten.Clear();
            FeedPosts.Clear();
            try
            {
                var posts = PostRepository.GetAll(HuidigeGebruiker?.Id ?? 0);
                foreach (var p in posts)
                {
                    Berichten.Add(new AdminPost { Id = p.Id, Auteur = p.AuthorName, Beschrijving = p.Content, DatumTekst = p.CreatedAt.ToString("d MMMM yyyy") });
                    FeedPosts.Add(new FeedPost
                    {
                        DbId = p.Id, UserId = p.UserId, AuteurNaam = p.AuthorName,
                        Beschrijving = p.Content, AfbeeldingPad = p.ImagePath,
                        LikeCount = p.LikeCount, LikedByMe = p.LikedByMe,
                        DatumTekst = p.CreatedAt.ToString("d MMMM yyyy HH:mm")
                    });
                }
            }
            catch { }
        }

        public void LaadEvenementen()
        {
            Evenementen.Clear();
            try
            {
                var events = EventRepository.GetAll(HuidigeGebruiker?.Id ?? 0);
                foreach (var ev in events)
                    Evenementen.Add(new AdminEvenement
                    {
                        Id = ev.Id, Naam = ev.Title, Locatie = ev.Location,
                        DatumTekst = ev.EventDate.ToString("d MMMM yyyy HH:mm"),
                        Beschrijving = ev.Description,
                        // FIX: sla CreatorId op zodat we na naamswijziging nog steeds weten wie auteur is
                        Auteur = ev.CreatorName,
                        CreatorId = ev.CreatorId,
                        MaxDeelnemers = ev.MaxParticipants,
                        AfbeeldingPad = ev.ImagePath ?? ""
                    });
            }
            catch { }
        }

        public void LaadGebruikers()
        {
            Gebruikers.Clear();
            try
            {
                var users = UserRepository.GetAll();
                foreach (var u in users) Gebruikers.Add(u);
            }
            catch { }
        }

        public void VoegEvenementToe(string naam, string locatie, string datumTijd,
                                     string beschrijving, string auteur,
                                     string afbeeldingPad = "", int maxDeelnemers = 0)
        {
            DateTime datum = DateTime.TryParse(datumTijd, out var d) ? d : DateTime.Now;
            int userId = HuidigeGebruiker?.Id ?? 0;
            int newId  = 0;
            if (userId > 0)
                try { newId = EventRepository.Aanmaken(userId, naam, beschrijving, locatie, datum, maxDeelnemers, afbeeldingPad); }
                catch { }

            Evenementen.Add(new AdminEvenement
            {
                Id = newId > 0 ? newId : (Evenementen.Count > 0 ? Evenementen[^1].Id + 1 : 1),
                Naam = naam, Locatie = locatie,
                DatumTekst = datum.ToString("d MMMM yyyy HH:mm"),
                Beschrijving = beschrijving, Auteur = auteur,
                AfbeeldingPad = afbeeldingPad, MaxDeelnemers = maxDeelnemers,
                CreatorId = userId
            });
        }

        public void VoegPostToe(string auteur, string beschrijving, string afbeeldingPad = "")
        {
            int userId = HuidigeGebruiker?.Id ?? 0;
            int newId  = 0;
            if (userId > 0)
                try { newId = PostRepository.Aanmaken(userId, beschrijving, afbeeldingPad); } catch { }

            var nu = DateTime.Now;
            Berichten.Add(new AdminPost { Id = newId > 0 ? newId : (Berichten.Count + 1), Auteur = auteur, Beschrijving = beschrijving, DatumTekst = nu.ToString("d MMMM yyyy") });
            FeedPosts.Insert(0, new FeedPost { DbId = newId, UserId = userId, AuteurNaam = auteur, Beschrijving = beschrijving, AfbeeldingPad = afbeeldingPad, DatumTekst = nu.ToString("d MMMM yyyy HH:mm") });
        }

        public void PasProfielToe(string voornaam, string achternaam, string email,
                                   string telefoon, string studierichting, string bio,
                                   string username = "", string profielFoto = "")
        {
            if (HuidigeGebruiker == null) return;

            string oudeDisplayNaam = HuidigeGebruiker.DisplayNaam;

            HuidigeGebruiker.Voornaam       = voornaam;
            HuidigeGebruiker.Achternaam     = achternaam;
            HuidigeGebruiker.Email          = email;
            HuidigeGebruiker.Telefoonnummer = telefoon;
            HuidigeGebruiker.Studierichting = studierichting;
            HuidigeGebruiker.Bio            = bio;
            if (!string.IsNullOrWhiteSpace(username))
                HuidigeGebruiker.Username = username;
            if (!string.IsNullOrWhiteSpace(profielFoto))
                HuidigeGebruiker.ProfielFotoPad = profielFoto;

            string nieuweDisplayNaam = HuidigeGebruiker.DisplayNaam;

            // FIX: update auteur in activiteiten (naam gewijzigd)
            if (oudeDisplayNaam != nieuweDisplayNaam)
            {
                foreach (var ev in Evenementen)
                    if (ev.CreatorId == HuidigeGebruiker.Id)
                        ev.Auteur = HuidigeGebruiker.VolledigeNaam;

                foreach (var post in FeedPosts)
                    if (post.UserId == HuidigeGebruiker.Id)
                        post.AuteurNaam = nieuweDisplayNaam;
            }

            try
            {
                UserRepository.UpdateProfiel(
                    HuidigeGebruiker.Id, voornaam, achternaam, email,
                    studierichting, bio, HuidigeGebruiker.Username,
                    HuidigeGebruiker.ProfielFotoPad);
            }
            catch { }
        }

        // Behoud achterwaartse compatibiliteit
        public void PasUsernameToe(string nieuweUsername)
        {
            if (HuidigeGebruiker == null) return;
            HuidigeGebruiker.Username = nieuweUsername;
            try
            {
                UserRepository.UpdateProfiel(HuidigeGebruiker.Id, HuidigeGebruiker.Firstname,
                    HuidigeGebruiker.Lastname, HuidigeGebruiker.Email, HuidigeGebruiker.Study,
                    HuidigeGebruiker.Bio, nieuweUsername, HuidigeGebruiker.ProfielFotoPad);
            }
            catch { }
        }
    }
}
