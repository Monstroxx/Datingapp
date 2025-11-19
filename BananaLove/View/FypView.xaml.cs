using BananaLove.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BananaLove.View
{
    /// <summary>
    /// Interaktionslogik für FypView.xaml
    /// Zeigt immer genau ein Profil an und erlaubt Navigation mit Links/Rechts.
    /// </summary>
    public partial class FypView : UserControl
    {
        private readonly List<(long UserId, string Gender, double DistanceKm)> _entries = new();
        private int _currentIndex = 0;

        public FypView()
        {
            InitializeComponent();

            // Key-Events empfangen
            this.Focusable = true;
            this.Loaded += (s, e) => this.Focus();
        }

        /// <summary>
        /// Lädt die Match-Ergebnisse aus DBHandler.get_prefference().
        /// Erwartet Tripel: [id1, gender1, distance1, id2, gender2, distance2, ...]
        /// </summary>
        public void LoadMatches(List<string> searchResults)
        {
            _entries.Clear();

            for (int i = 0; i + 2 < searchResults.Count; i += 3)
            {
                if (!long.TryParse(searchResults[i], out var userId))
                    continue;

                string gender = searchResults[i + 1];

                if (!double.TryParse(searchResults[i + 2], NumberStyles.Any, CultureInfo.InvariantCulture, out var distance))
                    distance = 0.0;

                _entries.Add((userId, gender, distance));
            }

            _currentIndex = 0;

            if (_entries.Count > 0)
            {
                ShowCurrentUser();
            }
            else
            {
                ClearDisplay();
            }
        }

        /// <summary>
        /// Zeigt den aktuell ausgewählten User (_currentIndex) im UI an.
        /// </summary>
        private void ShowCurrentUser()
        {
            if (_entries.Count == 0)
            {
                ClearDisplay();
                return;
            }

            var entry = _entries[_currentIndex];

            // Basisdaten zum User aus der DB holen
            var data = DBHandler.GetUserData(entry.UserId);

            string firstname = data.TryGetValue("firstname", out var fn) ? fn?.ToString() ?? "" : "";
            string lastname = data.TryGetValue("lastname", out var ln) ? ln?.ToString() ?? "" : "";
            string username = data.TryGetValue("user_name", out var un) ? un?.ToString() ?? "" : "";
            string bio = data.TryGetValue("bio", out var b) ? b?.ToString() ?? "" : "";

            // Name / Username / Bio
            txtName.Text = string.IsNullOrWhiteSpace(firstname + lastname)
                ? username
                : $"{firstname} {lastname}";

            txtUsername.Text = username;
            txtBio.Text = string.IsNullOrWhiteSpace(bio) ? "Noch keine Bio vorhanden." : bio;

            // Geschlecht (direkt aus get_prefference)
            txtGender.Text = entry.Gender;

            // Distanz-Text
            var de = new CultureInfo("de-DE");
            txtDistance.Text = $"{entry.DistanceKm.ToString("F1", de)} km entfernt";

            // Fallback auf Platzhalter (bild funktion noch net implementiert)
            imgProfile.Source = new BitmapImage(
                new Uri("/Images/icons8-männlicher-benutzer-eingekreist-48.png", UriKind.Relative)
            );


            // Index-Anzeige (z.B. "1 / 5")
            txtIndexInfo.Text = $"{_currentIndex + 1} / {_entries.Count}";
        }

        private void ClearDisplay()
        {
            txtName.Text = "Keine Vorschläge";
            txtUsername.Text = "";
            txtBio.Text = "";
            txtGender.Text = "";
            txtDistance.Text = "";
            imgProfile.Source = null;
            txtIndexInfo.Text = "0 / 0";
        }

        /// <summary>
        /// Navigation mit Pfeiltasten: Links = vorheriger, Rechts = nächster (loopend).
        /// </summary>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (_entries.Count == 0)
                return;

            if (e.Key == Key.Right)
            {
                _currentIndex = (_currentIndex + 1) % _entries.Count; // Loop nach vorne
                ShowCurrentUser();
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                _currentIndex = (_currentIndex - 1 + _entries.Count) % _entries.Count; // Loop nach hinten
                ShowCurrentUser();
                e.Handled = true;
            }
        }
    }
}


