using BananaLove.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BananaLove.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : WindowView
    {
        public MainView(Login login_data)
        {
            LoginData = login_data;
            InitializeComponent();
        }

        public void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement menu functionality
            // z.B. PreferenceView öffnen oder Sidebar-Menü anzeigen
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement profile view functionality
            PreferenceView preferenceView = new PreferenceView(LoginData);
            preferenceView.Show();
        }

        private void btnLike_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement like functionality
        }

        private void btnDislike_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement dislike functionality
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var searchResults = DBHandler.searchUsers(txtSearch.Text);
                LoadProfileListView(searchResults);
            }
        }

        private void LoadProfileListView(List<string> searchResults)
        {
            
            Grid grid = contentGrid;
            
            if (grid == null)
            {
                DebugHandler.LogError("contentGrid not found in MainView.");
                return;
            }

            // Lösche vorherige Inhalte
            grid.Children.Clear();

            // Erstelle ProfileListView
            ProfileListView profileListView = new ProfileListView();

            // Konvertiere die Suchergebnisse in ProfileCard-Objekte
            // searchResults enthält: [id1, username1, bio1, id2, username2, bio2, ...]
            List<ProfileListView.ProfileCard> profiles = new List<ProfileListView.ProfileCard>();
            
            for (int i = 0; i < searchResults.Count; i += 3)
            {
                if (i + 2 < searchResults.Count)
                {
                    profiles.Add(new ProfileListView.ProfileCard
                    {
                        UserId = long.Parse(searchResults[i]),
                        Username = searchResults[i + 1],
                        Bio = searchResults[i + 2],
                        Name = searchResults[i + 1], // Verwende Username als Name, bis wir den echten Namen haben
                        ImagePath = "/Images/icons8-männlicher-benutzer-eingekreist-48.png" // Placeholder
                    });
                }
            }

            // Lade Profile in die View
            profileListView.LoadProfiles(profiles);

            // Füge die View zum Grid hinzu
            grid.Children.Add(profileListView);
        }
    }
}
