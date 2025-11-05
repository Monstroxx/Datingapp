using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

            if (string.IsNullOrWhiteSpace(server) ||
                string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(database))
            {
                throw new InvalidOperationException("Eine oder mehrere Datenbank-Umgebungsvariablen sind nicht gesetzt.");
            }

            string connectionString = $"server={server};port={port};userid={user};password={password};database={database};";

            return new MySqlConnection(connectionString);
        }

        public static Login TryLogin(String userEmail, String userPassword)
        {
            try
            {
                var con = connect();
                con.Open();
                String query = $"SELECT id, user, email, password FROM `Login`";// WHERE email = {userEmail}";
                var cmd = new MySqlCommand(query, con);

                var reader = cmd.ExecuteReader();
                var output = "";
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        output += $"{reader.GetName(i)}: {reader[i]}  ";
                    }
                    DebugHandler.Log(output);
                    output = "";
                }
            }
            catch (Exception e)
            {
                DebugHandler.Log("Error while Login!" + e.Message);
                return new Login(0, 0, LoginStates.Error);
            }

            return new Login(0, 0, LoginStates.Error);
        }

        public static bool SaveLogin()
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
