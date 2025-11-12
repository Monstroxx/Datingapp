using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für ProfileView.xaml
    /// </summary>
    public partial class ProfileView : WindowView
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        public ProfileView(string firstName, string lastName, DateTime? birthday, string bio, string imagePath = null)
        {
            InitializeComponent();
            LoadProfile(firstName, lastName, birthday, bio, imagePath);
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

        public void LoadProfile(string firstName, string lastName, DateTime? birthday, string bio, string imagePath = null)
        {
            // Name setzen
            txtName.Text = firstName ?? "Name";
            txtLastName.Text = lastName ?? "Nachname";

            // Alter berechnen
            if (birthday.HasValue)
            {
                var today = DateTime.Today;
                var age = today.Year - birthday.Value.Year;
                if (birthday.Value.Date > today.AddYears(-age)) age--;
                txtAge.Text = age.ToString();
            }
            else
            {
                txtAge.Text = "--";
            }

            // Bio setzen
            txtBio.Text = string.IsNullOrWhiteSpace(bio) ? "Keine Bio vorhanden." : bio;

            // Bild laden (wenn vorhanden)
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgProfile.Source = bitmap;
                }
                catch
                {
                    // Bei Fehler Platzhalter behalten
                }
            }
        }
    }
}

