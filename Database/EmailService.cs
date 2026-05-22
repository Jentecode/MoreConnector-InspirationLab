using System;
using System.Net;
using System.Net.Mail;

namespace MoreConnector.Database
{
    public static class EmailService
    {
        private const string SmtpHost     = "smtp.gmail.com";
        private const int    SmtpPort     = 587;
        private const string VanEmail     = "moreconnector@gmail.com";
        private const string AppPassword  = "jnen uvll pjjm ygtj";

        public static void StuurWachtwoordReset(string naarEmail, string token)
        {
            var body = $@"
Hallo,

Je hebt een wachtwoord reset aangevraagd voor je MoreConnector account.

Gebruik de volgende code om je wachtwoord te resetten:

    {token}

Deze code is 15 minuten geldig.

Als je dit niet hebt aangevraagd, kan je deze e-mail negeren.

Met vriendelijke groeten,
MoreConnector
";
            Stuur(naarEmail, "Wachtwoord resetten - MoreConnector", body);
        }

        public static void StuurEmailVerificatie(string naarEmail, string token)
        {
            var body = $@"
Hallo,

Welkom bij MoreConnector! Bevestig je e-mailadres met de onderstaande code:

    {token}

Deze code is 30 minuten geldig.

Met vriendelijke groeten,
MoreConnector
";
            Stuur(naarEmail, "Bevestig je e-mailadres - MoreConnector", body);
        }

        private static void Stuur(string naar, string onderwerp, string body)
        {
            using var client = new SmtpClient(SmtpHost, SmtpPort)
            {
                EnableSsl   = true,
                Credentials = new NetworkCredential(VanEmail, AppPassword)
            };

            using var bericht = new MailMessage(VanEmail, naar, onderwerp, body);
            client.Send(bericht);
        }
    }
}
