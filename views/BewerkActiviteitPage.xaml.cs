using MoreConnector.Database;
using MoreConnector.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MoreConnector.Views
{
    public partial class BewerkActiviteitPage : Page
    {
        private readonly AdminEvenement _ev;
        private readonly AppState _state = AppState.Instance;

        public BewerkActiviteitPage(AdminEvenement evenement)
        {
            InitializeComponent();
            _ev = evenement;
            LaadData();
        }

        private void LaadData()
        {
            NaamInput.Text          = _ev.Naam;
            LocatieInput.Text       = _ev.Locatie;
            BeschrijvingInput.Text  = _ev.Beschrijving;
            MaxDeelnemersInput.Text = _ev.MaxDeelnemers.ToString();

            if (DateTime.TryParse(_ev.DatumTekst, out var dt))
            {
                DatumPicker.SelectedDate = dt.Date;
                TijdInput.Text = dt.ToString("HH:mm");
            }
        }

        private void OnOpslaanClick(object sender, RoutedEventArgs e)
        {
            string naam  = NaamInput.Text.Trim();
            string loc   = LocatieInput.Text.Trim();
            string beschr = BeschrijvingInput.Text.Trim();

            if (string.IsNullOrEmpty(naam) || string.IsNullOrEmpty(loc))
            {
                MessageBox.Show("Naam en locatie zijn verplicht.", "Validatie");
                return;
            }

            if (DatumPicker.SelectedDate == null)
            {
                MessageBox.Show("Kies een datum.", "Validatie");
                return;
            }

            DateTime datum = DatumPicker.SelectedDate.Value.Date;
            if (TimeSpan.TryParseExact(TijdInput.Text.Trim(), @"hh\:mm", null, out var ts))
                datum = datum.Add(ts);
            else if (TimeSpan.TryParseExact(TijdInput.Text.Trim(), @"h\:mm", null, out var ts2))
                datum = datum.Add(ts2);

            int max = 0;
            int.TryParse(MaxDeelnemersInput.Text.Trim(), out max);

            // Update in DB
            try
            {
                EventRepository.Bijwerken(_ev.Id, naam, beschr, loc, datum, max);
            }
            catch { }

            // Update in memory
            _ev.Naam         = naam;
            _ev.Locatie      = loc;
            _ev.Beschrijving = beschr;
            _ev.DatumTekst   = datum.ToString("d MMMM yyyy HH:mm");
            _ev.MaxDeelnemers = max;

            MessageBox.Show("Activiteit opgeslagen!", "Opgeslagen");
            Nav().AuthFrame.Navigate(new ActivityPage());
        }

        private void OnAnnulerenClick(object sender, RoutedEventArgs e)
            => Nav().AuthFrame.Navigate(new ActivityPage());

        private MoreConnector Nav() => (MoreConnector)Window.GetWindow(this);
    }
}
