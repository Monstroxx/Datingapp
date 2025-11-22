using BananaLove.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
    public partial class LoginView : WindowView
    {
        public LoginView()
        {
            InitializeComponent();
            // Im Debug-Build liegt .env typischerweise im Projektordner.
            // im Release (publish) liegt sie neben der EXE.
            // Wir versuchen zuerst die lokale .env im Ausführungsverzeichnis.
            var baseDirEnv = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            if (File.Exists(baseDirEnv))
            {
                EnvHandler.LoadEnvs(baseDirEnv);
            }
            else
            {
                // Fallback: alter relativer Pfad für Debug-Start aus Visual Studio
                EnvHandler.LoadEnvs("../../../.env");
            }
            DBHandler.TestConnection();
            ViewHandler.openeverything(this);
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
            LoginData = DBHandler.TryLogin(txtMail.Text, txtPass.Password);
            if (!new List<DBHandler.LoginStates> {
                DBHandler.LoginStates.EmailNotFound,
                DBHandler.LoginStates.Error,
                DBHandler.LoginStates.PasswordIncorrect
            }.Contains(LoginData.State))
                ViewHandler.openMainWindow(true, this);
        }

        private void Signup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoginData = DBHandler.SaveLogin(txtMail.Text, txtPass.Password, txtUser.Text);
            if (!new List<DBHandler.LoginStates> { 
                DBHandler.LoginStates.EmailNotFound, 
                DBHandler.LoginStates.Error, 
                DBHandler.LoginStates.PasswordIncorrect 
            }.Contains(LoginData.State))
                    ViewHandler.openPreferenceWindow(true, this);
        }
    }
}
