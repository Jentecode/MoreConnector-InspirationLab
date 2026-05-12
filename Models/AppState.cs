using MoreConnector.Database;
using System;
using System.Collections.ObjectModel;

namespace MoreConnector.Models
{
    public sealed class AppState
    {
        private static readonly Lazy<AppState> _instance = new(() => new AppState());
        public static AppState Instance => _instance.Value;
        private AppState() { }

        // ── Ingelogde gebruiker ──────────────────────────────────────────────
        public User? HuidigeGebruiker { get; set; }
        public bool  IsAdmin => HuidigeGebruiker?.Role == "Admin";

        // ── Observable collections voor UI ───────────────────────────────────
        public ObservableCollection<AdminEvenement> Evenementen { get; } = new();
        public ObservableCollection<AdminPost>      Berichten   { get; } = new();
        public ObservableCollection<User>           Gebruikers  { get; } = new();

        // FeedPosts zitten in Feed.xaml.cs als FeedPost — hier als brug
        public ObservableCollection<FeedPost> FeedPosts   { get; } = new();

        // ── DB laden ─────────────────────────────────────────────────────────
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
                    Berichten.Add(new AdminPost
                    {
                        Id           = p.Id,
                        Auteur       = p.AuthorName,
                        Beschrijving = p.Content,
                        DatumTekst   = p.CreatedAt.ToString("d MMMM yyyy")
                    });
                    FeedPosts.Add(new FeedPost
                    {
                        DbId          = p.Id,
                        UserId        = p.UserId,
                        AuteurNaam    = p.AuthorName,
                        Beschrijving  = p.Content,
                        AfbeeldingPad = p.ImagePath,
                        LikeCount     = p.LikeCount,
                        LikedByMe     = p.LikedByMe
                    });
                }
            }
            catch { /* DB niet beschikbaar */ }
        }

        public void LaadEvenementen()
        {
            Evenementen.Clear();
            try
            {
                var events = EventRepository.GetAll(HuidigeGebruiker?.Id ?? 0);
                foreach (var ev in events)
                {
                    Evenementen.Add(new AdminEvenement
                    {
                        Id           = ev.Id,
                        Naam         = ev.Title,
                        Locatie      = ev.Location,
                        DatumTekst   = ev.EventDate.ToString("d MMMM yyyy HH:mm"),
                        Beschrijving = ev.Description,
                        Auteur       = ev.CreatorName
                    });
                }
            }
            catch { /* DB niet beschikbaar */ }
        }

        public void LaadGebruikers()
        {
            Gebruikers.Clear();
            try
            {
                var users = UserRepository.GetAll();
                foreach (var u in users)
                    Gebruikers.Add(u);
            }
            catch { /* DB niet beschikbaar */ }
        }

        // ── Helpers voor views ────────────────────────────────────────────────
        public void VoegEvenementToe(string naam, string locatie, string datumTijd,
                                     string beschrijving, string auteur, string afbeeldingPad = "")
        {
            DateTime datum = DateTime.TryParse(datumTijd, out var d) ? d : DateTime.Now;
            int userId = HuidigeGebruiker?.Id ?? 0;
            if (userId > 0)
            {
                try { EventRepository.Aanmaken(userId, naam, beschrijving, locatie, datum); }
                catch { }
            }

            Evenementen.Add(new AdminEvenement
            {
                Id           = Evenementen.Count > 0 ? Evenementen[^1].Id + 1 : 1,
                Naam         = naam,
                Locatie      = locatie,
                DatumTekst   = datumTijd,
                Beschrijving = beschrijving,
                Auteur       = auteur,
                AfbeeldingPad = afbeeldingPad
            });
        }

        public void VoegPostToe(string auteur, string beschrijving, string afbeeldingPad = "")
        {
            int userId = HuidigeGebruiker?.Id ?? 0;
            int newId  = 0;
            if (userId > 0)
            {
                try { newId = PostRepository.Aanmaken(userId, beschrijving, afbeeldingPad); }
                catch { }
            }

            Berichten.Add(new AdminPost
            {
                Id           = newId > 0 ? newId : (Berichten.Count + 1),
                Auteur       = auteur,
                Beschrijving = beschrijving,
                DatumTekst   = DateTime.Now.ToString("d MMMM yyyy")
            });

            FeedPosts.Insert(0, new FeedPost
            {
                DbId          = newId,
                UserId        = userId,
                AuteurNaam    = auteur,
                Beschrijving  = beschrijving,
                AfbeeldingPad = afbeeldingPad
            });
        }

        public void PasUsernameToe(string nieuweUsername)
        {
            if (HuidigeGebruiker == null) return;
            HuidigeGebruiker.Username = nieuweUsername;
        }

        public void PasProfielToe(string voornaam, string achternaam, string email,
                                   string telefoon, string studierichting, string bio)
        {
            if (HuidigeGebruiker == null) return;
            HuidigeGebruiker.Voornaam      = voornaam;
            HuidigeGebruiker.Achternaam    = achternaam;
            HuidigeGebruiker.Email         = email;
            HuidigeGebruiker.Telefoonnummer = telefoon;
            HuidigeGebruiker.Studierichting = studierichting;
            HuidigeGebruiker.Bio           = bio;

            try
            {
                UserRepository.UpdateProfiel(HuidigeGebruiker.Id,
                    voornaam, achternaam, email, studierichting, bio);
            }
            catch { }
        }
    }
}
