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
        Login login;
        public MainView(Login login_data)
        {
            login = login_data;
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
            PreferenceView preferenceView = new PreferenceView(login);
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
                DBHandler.searchUsers(txtSearch.Text);
            }
        }
    }
}
