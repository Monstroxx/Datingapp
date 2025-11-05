using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

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

        public static Login TryLogin(String userEmail, String userPassword)
        {
            string server = Environment.GetEnvironmentVariable("DatabaseServerIP");
            string port = Environment.GetEnvironmentVariable("DatabaseServerPort");
            string user = Environment.GetEnvironmentVariable("DatabaseServerProfilename");
            string password = Environment.GetEnvironmentVariable("DatabaseServerPassword");
            string database = Environment.GetEnvironmentVariable("DatabaseName");

            string connectionString = $"server={server};port={port};userid={user};password={password};database={database};";
            
            try
            {
                var con = new MySqlConnection(​connectionString​);
                con.Open();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new Login(0,0,LoginStates.Error);
            }

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
