using BananaLove.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace BananaLove.Utility
{
    class ViewHandler
    {
        public static bool ViewDebug = true; 

        public static void openMainWindow(bool close, LoginView currentView)
        {
            MainView mainView = new MainView(currentView.LoginData);
            mainView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openPreferenceWindow(bool close, LoginView currentView)
        {
            PreferenceView preferenceView = new PreferenceView(currentView.LoginData);
            preferenceView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openProfileView(bool close, LoginView currentView)
        {
            ProfileView profileView = new ProfileView();
            profileView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openLoginView(bool close, LoginView currentView)
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openeverything(LoginView currentView)
        {
            if (ViewDebug == true)
            {
                openMainWindow(false, currentView);
                openPreferenceWindow(false, currentView);
                openProfileView(false, currentView);
            }
        }
    }
}
