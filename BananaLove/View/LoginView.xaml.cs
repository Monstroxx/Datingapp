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
    /// Interaktionslogik für LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            EnvHandler.LoadEnvs("../../../.env");
            DBHandler.TestConnection();
        }

        public void openMainWindow()
        {
            MainView mainView = new MainView();
            mainView.Show();
            this.Close();
        }
        public void openSignupWindow()
        {
            // PreferenceView öffnen
            PreferenceView preferenceView = new PreferenceView();
            preferenceView.Show();

            this.Close();
        }
        public void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinnimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var a = DBHandler.TryLogin(txtMail.Text, txtPass.Password);
            openMainWindow();
        }

        private void Signup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var a = DBHandler.SaveLogin(txtMail.Text, txtPass.Password);
            openSignupWindow();
        }
    }
}
