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
        Login login;
        public LoginView()
        {
            InitializeComponent();
            EnvHandler.LoadEnvs("../../../.env");
            DBHandler.TestConnection();
        }

        public void openMainWindow()
        {
            MainView mainView = new MainView(login);
            mainView.Show();
            this.Close();
        }
        public void openSignupWindow()
        {
            // PreferenceView öffnen
            PreferenceView preferenceView = new PreferenceView(login);
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
            login = DBHandler.TryLogin(txtMail.Text, txtPass.Password);
            if (!new List<DBHandler.LoginStates> {
                DBHandler.LoginStates.EmailNotFound,
                DBHandler.LoginStates.Error,
                DBHandler.LoginStates.PasswordIncorrect
            }.Contains(login.State))
                openMainWindow();
        }

        private void Signup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            login = DBHandler.SaveLogin(txtMail.Text, txtPass.Password, txtUser.Text);
            if (!new List<DBHandler.LoginStates> { 
                DBHandler.LoginStates.EmailNotFound, 
                DBHandler.LoginStates.Error, 
                DBHandler.LoginStates.PasswordIncorrect 
            }.Contains(login.State))
                    openSignupWindow();
        }
    }
}
