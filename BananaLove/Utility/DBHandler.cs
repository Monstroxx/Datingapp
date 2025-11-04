using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace BananaLove.Utility
{
    public static class DBHandler
    {
        

        public enum LoginStates
        {
            NewUser,
            ExistingUser,
            Error,
        }

        public static Login TryLogin(String email, String password)
        {
            return new Login(0,0,LoginStates.Error);
        }

        public static bool SaveLogin(  )
        {
            return false;
        }

    }

    public class Login
    {
        int UserID, LoginID;
        DBHandler.LoginStates State;

        public Login(int userID, int loginID, DBHandler.LoginStates state)
        {
            UserID = userID;
            LoginID = loginID;
            State = state;
        }
    }
}
