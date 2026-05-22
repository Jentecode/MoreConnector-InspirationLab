using MoreConnector.Database;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MoreConnector.Models
{
    // AppState houdt alle globale data bij die de app nodig heeft
    // Één instantie voor de hele app (singleton)
    public sealed class AppState : INotifyPropertyChanged
    {
        public static AppState Instance { get; } = new AppState();
        private AppState() { }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Ingelogde gebruiker
        private User? _huidigeGebruiker;
        public User? HuidigeGebruiker
        {
            get => _huidigeGebruiker;
            set { _huidigeGebruiker = value; Notify(nameof(HuidigeGebruiker)); Notify(nameof(IsAdmin)); }
        }

        public bool IsAdmin => HuidigeGebruiker?.IsAdmin == true || HuidigeGebruiker?.Role == "Admin";

        // Lijsten die in de UI worden getoond
        public ObservableCollection<AdminEvenement> Evenementen { get; } = new();
        public ObservableCollection<AdminPost>      Berichten   { get; } = new();
        public ObservableCollection<User>           Gebruikers  { get; } = new();
        public ObservableCollection<FeedPost>       FeedPosts   { get; } = new();

        // Laad alles opnieuw vanuit de database
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
                foreach (var p in PostRepository.GetAll(HuidigeGebruiker?.Id ?? 0))
                {
                    Berichten.Add(new AdminPost
                    {
                        Id          = p.Id,
                        Auteur      = p.AuthorName,
                        Beschrijving = p.Content,
                        DatumTekst  = p.CreatedAt.ToString("d MMMM yyyy")
                    });
                    FeedPosts.Add(new FeedPost
                    {
                        DbId          = p.Id,
                        UserId        = p.UserId,
                        AuteurNaam    = p.AuthorName,
                        Beschrijving  = p.Content,
                        AfbeeldingPad = p.ImagePath,
                        LikeCount     = p.LikeCount,
                        LikedByMe     = p.LikedByMe,
                        DatumTekst    = p.CreatedAt.ToString("d MMMM yyyy HH:mm")
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
                var nu = DateTime.Now;
                foreach (var ev in EventRepository.GetAll(HuidigeGebruiker?.Id ?? 0))
                {
                    // Verwijder evenementen die al voorbij zijn
                    if (ev.EventDate < nu)
                    {
                        try { EventRepository.Verwijder(ev.Id); } catch { }
                        continue;
                    }
                    Evenementen.Add(new AdminEvenement
                    {
                        Id            = ev.Id,
                        Naam          = ev.Title,
                        Locatie       = ev.Location,
                        DatumTekst    = ev.EventDate.ToString("d MMMM yyyy HH:mm"),
                        Beschrijving  = ev.Description,
                        Auteur        = ev.CreatorName,
                        CreatorId     = ev.CreatorId,
                        MaxDeelnemers = ev.MaxParticipants,
                        AfbeeldingPad = ev.ImagePath ?? ""
                    });
                }
            }
            catch { }
        }

        public void LaadGebruikers()
        {
            Gebruikers.Clear();
            try
            {
                foreach (var u in UserRepository.GetAll())
                    Gebruikers.Add(u);
            }
            catch { }
        }

        // Nieuw evenement toevoegen (opslaan in DB + toevoegen aan lijst)
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
                Id            = newId > 0 ? newId : (Evenementen.Count > 0 ? Evenementen[^1].Id + 1 : 1),
                Naam          = naam,
                Locatie       = locatie,
                DatumTekst    = datum.ToString("d MMMM yyyy HH:mm"),
                Beschrijving  = beschrijving,
                Auteur        = auteur,
                AfbeeldingPad = afbeeldingPad,
                MaxDeelnemers = maxDeelnemers,
                CreatorId     = userId
            });
        }

        // Nieuwe post toevoegen
        public void VoegPostToe(string auteur, string beschrijving, string afbeeldingPad = "")
        {
            int userId = HuidigeGebruiker?.Id ?? 0;
            int newId  = 0;

            if (userId > 0)
                try { newId = PostRepository.Aanmaken(userId, beschrijving, afbeeldingPad); } catch { }

            var nu = DateTime.Now;
            Berichten.Add(new AdminPost
            {
                Id           = newId > 0 ? newId : (Berichten.Count + 1),
                Auteur       = auteur,
                Beschrijving = beschrijving,
                DatumTekst   = nu.ToString("d MMMM yyyy")
            });
            FeedPosts.Insert(0, new FeedPost
            {
                DbId          = newId,
                UserId        = userId,
                AuteurNaam    = auteur,
                Beschrijving  = beschrijving,
                AfbeeldingPad = afbeeldingPad,
                DatumTekst    = nu.ToString("d MMMM yyyy HH:mm")
            });
        }

        // Profielgegevens bijwerken (naam, e-mail, foto, ...)
        public void PasProfielToe(string voornaam, string achternaam, string email,
                                   string telefoon, string studierichting, string bio,
                                   string username = "", string profielFoto = "")
        {
            if (HuidigeGebruiker == null) return;

            string oudeNaam = HuidigeGebruiker.DisplayNaam;

            HuidigeGebruiker.Voornaam       = voornaam;
            HuidigeGebruiker.Achternaam     = achternaam;
            HuidigeGebruiker.Email          = email;
            HuidigeGebruiker.Telefoonnummer = telefoon;
            HuidigeGebruiker.Studierichting = studierichting;
            HuidigeGebruiker.Bio            = bio;
            if (!string.IsNullOrWhiteSpace(username))   HuidigeGebruiker.Username       = username;
            if (!string.IsNullOrWhiteSpace(profielFoto)) HuidigeGebruiker.ProfielFotoPad = profielFoto;

            string nieuweNaam = HuidigeGebruiker.DisplayNaam;

            // Als de naam veranderd is, pas dit ook aan in evenementen en posts
            if (oudeNaam != nieuweNaam)
            {
                foreach (var ev in Evenementen.Where(e => e.CreatorId == HuidigeGebruiker.Id))
                    ev.Auteur = string.IsNullOrWhiteSpace(HuidigeGebruiker.Username)
                        ? HuidigeGebruiker.VolledigeNaam
                        : HuidigeGebruiker.Username;

                foreach (var post in FeedPosts.Where(p => p.UserId == HuidigeGebruiker.Id))
                    post.AuteurNaam = nieuweNaam;
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
    }
}
