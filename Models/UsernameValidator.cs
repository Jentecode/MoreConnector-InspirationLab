namespace MoreConnector.Models
{
    public static class UsernameValidator
    {
        // Blacklist voor content (posts, comments, activiteiten)
        private static readonly string[] ContentBlacklist =
        {
            // Nederlands
            "kut", "lul", "pik", "tiet", "reet", "kak", "pis",
            "hoer", "slet", "teef", "eikel", "klootzak", "idioot", "sukkel",
            "kanker", "tyfus", "tering", "godver", "godverdomme",
            "dom", "debiel", "mongool", "autist", "spast", "zwakzinnig",
            "hoerenzoon", "schijt", "kots", "stront",
            "racist", "fascist", "nazi", "hitler", "heil",
            // Racisme / discriminatie
            "neger", "nikker", "makak", "mof",
            // Engels
            "fuck", "shit", "bitch", "dick", "cunt", "cock", "pussy",
            "nigger", "faggot", "retard", "bastard",
            "whore", "slut", "fag", "rape",
            "porn", "nude", "naked",
            "kkk", "isis", "jihad",
            // Codes
            "88", "1488", "14words"
        };

        /// <summary>
        /// Controleert posts, comments en evenementen op ongepaste inhoud.
        /// </summary>
        public static bool IsGeldigeContent(string tekst)
        {
            if (string.IsNullOrWhiteSpace(tekst)) return true;
            string lower = tekst.ToLower();
            foreach (var woord in ContentBlacklist)
                if (lower.Contains(woord)) return false;
            return true;
        }

        public static string? ValideerContent(string tekst)
        {
            if (!IsGeldigeContent(tekst))
                return "Je bericht bevat ongepaste inhoud. Pas dit aan.";
            return null;
        }

        private static readonly string[] Blacklist =
        {
            // Nederlands
            "kut", "lul", "pik", "tiet", "reet", "kak", "pis",
            "hoer", "slet", "teef", "eikel", "klootzak", "idioot", "sukkel",
            "kanker", "tyfus", "tering", "godver", "godverdomme",
            "dom", "debiel", "mongool", "autist", "spast", "zwakzinnig",
            "hoerenzoon", "schijt", "kots", "stront",
            "racist", "fascist", "nazi", "hitler", "heil",
            // Racisme / discriminatie
            "neger", "nikker", "makak", "mof", "jood",
            // Engels
            "fuck", "shit", "bitch", "dick", "cunt", "cock", "pussy",
            "nigger", "faggot", "retard", "bastard",
            "whore", "slut", "fag", "rape", "kill", "murder",
            "porn", "sex", "nude", "naked",
            "kkk", "isis", "jihad",
            // Codes
            "88", "1488", "14words",
            // Misleidende namen (alleen voor usernames)
            "official", "real", "fake", "bot", "spam", "hack", "scam",
            // Admin misbruik
            "admin", "administrator", "moderator", "mod", "staff",
            "support", "helpdesk", "root", "superuser", "system"
        };

        /// <summary>
        /// Geeft true als de username toegestaan is, false als hij ongepast is.
        /// </summary>
        public static bool IsGeldig(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return true; // leeg = apart gevalideerd
            string lower = username.ToLower();
            foreach (var woord in Blacklist)
                if (lower.Contains(woord)) return false;
            return true;
        }

        /// <summary>
        /// Geeft een foutmelding terug als de username ongepast is, anders null.
        /// </summary>
        public static string? Valideer(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            if (!IsGeldig(username))
                return "Deze gebruikersnaam is niet toegestaan. Kies een andere naam.";
            return null;
        }
    }
}
