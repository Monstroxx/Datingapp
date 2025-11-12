using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BananaLove.View
{
    /// <summary>
    /// Interaktionslogik für ProfileListView.xaml
    /// </summary>
    public partial class ProfileListView : UserControl
    {
        public class ProfileCard
        {
            public long UserId { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }
            public string Bio { get; set; }
            public string ImagePath { get; set; }
        }

        public ProfileListView()
        {
            InitializeComponent();
        }

        public void LoadProfiles(List<ProfileCard> profiles)
        {
            itemsControlProfiles.ItemsSource = profiles;
        }

        private void ProfileCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProfileCard profile)
            {
                // TODO: Öffne ProfileView mit den Profil-Daten
                // ProfileView profileView = new ProfileView(profile.Name, ...);
                // profileView.Show();
            }
        }
    }
}

