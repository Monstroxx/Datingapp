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
        public bool ViewDebug = true;
        public static void openMainWindow(bool close, MainView currentView)
        {
            MainView mainView = new MainView();
            mainView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openPreferenceWindow(bool close, LoginView currentView)
        {
            PreferenceView preferenceView = new PreferenceView();
            preferenceView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openProfileView(bool close, MainView currentView)
        {
            ProfileView profileView = new ProfileView();
            profileView.Show();
            if (close) { currentView.Close(); }
        }
        public static void openLoginView(bool close, MainView currentView)
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            if (close) { currentView.Close(); }
        }
    }
}
