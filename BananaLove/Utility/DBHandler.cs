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

        public static bool TestConnection()
        {
            try
            {
                var con = connect();
                con.Open();
                string query = "SHOW TABLES;";
                var cmd = new MySqlCommand(query, con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DebugHandler.Log(reader.GetString(0));
                }
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                DebugHandler.Log("Error while connecting to Database!" + e.Message);
                return false;
            }
        }

        public static Login TryLogin(string userEmail, string userPassword)
        {
            MySqlConnection con = connect();
            con.Open();

            try
            {
                string query = $"SELECT id, user, email, password FROM `Login` WHERE `email` = @userEmail"; // Nur @userEmail, um SQL injection zu vermeiden
                var cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@userEmail", userEmail);

                var reader = cmd.ExecuteReader();
                string output = "";
                int results = 0;
                while (reader.Read())
                {
                    DebugHandler.seperate();

                    int id = reader.GetInt32("id");
                    int user = reader.GetInt32("user");
                    string email = reader.GetString("email");
                    string password = reader.GetString("password");

                    if (password == userPassword)
                    {
                        DebugHandler.Log("Found user bellow! Password is correct. :)");
                    }
                    else
                    {
                        DebugHandler.Log("Found user bellow! Password is not correct! :(");
                    }

                    results++;

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        output += $"{reader.GetName(i)}: {reader[i]}  ";
                    }
                    DebugHandler.Log(output);
                    output = "";
                }
                DebugHandler.Log($"Found {results} results.");
                DebugHandler.seperate();
            }
            catch (Exception e)
            {
                con.Close();
                DebugHandler.Log("Error while Login!" + e.Message);
                return new Login(0, 0, LoginStates.Error);
            }

            con.Close();
            return new Login(0, 0, LoginStates.Error);
        }

        public static bool SaveLogin(string userEmail, string userPassword) // NICHT GETESTET!!!
        {
            DebugHandler.seperate();
            try
            {
                // 4. Login -> 3. User -> 2. Profile -> 1. Address
                var con = connect();

                string insertAddress = "INSERT INTO Address (street, number, city, postal) VALUES (\"Hauptstraße\", 1, \"Berlin\", 10115)";
                MySqlCommand cmd = new MySqlCommand(insertAddress, con);
                cmd.ExecuteNonQuery();
                long addressId = cmd.LastInsertedId;

                DebugHandler.Log($"Wrote new Address in {cmd.LastInsertedId}.");

                string insertProfile = "INSERT INTO Profil (bio, picture, address_id) VALUES (\"No Bio\", \"\", @address_id)";
                cmd = new MySqlCommand(insertProfile, con);
                cmd.Parameters.AddWithValue("@address_id", addressId);
                cmd.ExecuteNonQuery();
                long profileId = cmd.LastInsertedId;

                DebugHandler.Log($"Wrote new Profile in {cmd.LastInsertedId}.");

                string insertUser = "INSERT INTO User (firstname, lastname, birthday, gender, postal, profil_id) VALUES (\"User\", \"Name\", DATE(1-1-2000), \"m\", 10115, @profil_id)";
                cmd = new MySqlCommand(insertUser, con);
                cmd.Parameters.AddWithValue("@profil_id", profileId);
                cmd.ExecuteNonQuery();
                long userId = cmd.LastInsertedId;

                DebugHandler.Log($"Wrote new User in {cmd.LastInsertedId}.");

                string insertLogin = "INSERT INTO Login (user_id, email, password) VALUES (@user_id, @email, @password)";
                cmd = new MySqlCommand(insertLogin, con);
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.Parameters.AddWithValue("@email", userEmail);
                cmd.Parameters.AddWithValue("@password", userPassword); // besser: vorher hashen! (Mache ich sicher später)
                cmd.ExecuteNonQuery();

                /*
                 * using BCrypt.Net;
                 * string hash = BCrypt.HashPassword("meinPasswort");
                 * 
                 * bool isValid = BCrypt.Verify(eingegebenesPasswort, gespeicherterHash);
                 */

                DebugHandler.Log($"Wrote new Login in {cmd.LastInsertedId}.");
                return true;
            }
            catch
            {
                DebugHandler.Log("Error while writing new Login. Please check the Database!!!");
                return false;
            }


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
