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

        public static MySqlConnection connect()
        {
            string server = Environment.GetEnvironmentVariable("DatabaseServerIP");
            string port = Environment.GetEnvironmentVariable("DatabaseServerPort");
            string user = Environment.GetEnvironmentVariable("DatabaseServerProfilename");
            string password = Environment.GetEnvironmentVariable("DatabaseServerPassword");
            string database = Environment.GetEnvironmentVariable("DatabaseName");

            string connectionString = $"server={server};port={port};userid={user};password={password};database={database};";

            return new MySqlConnection(​connectionString​);
        }

        public static bool TestConnection()
        {
            try
            {
                var con = connect();
                con.Open();
                string query = $"SHOW TABLES;";
                var cmd = new MySqlCommand(query, con);
                DebugHandler.Log(cmd.ToString());
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                DebugHandler.Log("Database connection failed: " + e.Message);
                return false;
            }
        }

        public static Login TryLogin(String userEmail, String userPassword)
        {   
            try
            {
                var con = connect();
                con.Open();
                String query = $"SELECT id FROM users WHERE email = @userEmail";
                var cmd = new MySqlCommand(query, con);
            }
            catch (Exception e)
            {
                DebugHandler.Log("Error while Login!" + e.Message);
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
