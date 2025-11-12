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
        public static bool ViewDebug = false; 

        public static void openMainWindow(bool close, WindowView currentView)
        {
            MainView mainView = new MainView(currentView.LoginData);
            mainView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openPreferenceWindow(bool close, WindowView currentView)
        {
            PreferenceView preferenceView = new PreferenceView(currentView.LoginData);
            preferenceView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openProfileView(bool close, WindowView currentView)
        {
            ProfileView profileView = new ProfileView();
            profileView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openLoginView(bool close, WindowView currentView)
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openeverything(WindowView currentView)
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
