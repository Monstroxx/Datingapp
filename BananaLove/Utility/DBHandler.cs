using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Net.Mail;

namespace BananaLove.Utility
{
    public static class DBHandler
    {
        public enum LoginStates
        {
            NewUser,
            ExistingUser,
            EmailNotFound,
            PasswordIncorrect,
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

        public static bool EmailExists(string userEmail)
        {
            var con = connect();
            con.Open();
            
            string query = $"SELECT id, user_id, email, password FROM `Login` WHERE `email` = @userEmail";
            var cmd = new MySqlCommand(query, con);
            cmd.Parameters.AddWithValue("@userEmail", userEmail);

            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                con.Close();
                return true;
            }

            con.Close();
            return false;
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
                DebugHandler.LogError("Error while connecting to Database!" + e.Message);
                return false;
            }
        }

        public static Login TryLogin(string userEmail, string userPassword)
        {
            DebugHandler.seperate();
            var con = connect();
            con.Open();

            if (!Utility.IsValidEmail(userEmail))
            {
                DebugHandler.LogError("SaveLogin called with invalid email format.");
                return Login.Error(LoginStates.EmailNotFound);
            }

            try
            {
                string query = $"SELECT id, user_id, email, password FROM `Login` WHERE `email` = @userEmail";
                var cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@userEmail", userEmail);

                var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    DebugHandler.LogError("Login failed: Email not found.");
                    return Login.Error(LoginStates.EmailNotFound);
                }

                long loginId = reader.GetInt64("id");
                long userId = reader.GetInt64("user_id");
                string storedHash = reader.GetString("password");

                // Wenn Plaintext benutzt wird:
                if (storedHash == userPassword)
                {
                    DebugHandler.Log($"Login successful for user_id={userId}.");
                    return new Login(userId, loginId, LoginStates.ExistingUser);
                }

                // Wenn schon bcrypt benutzt wird (Mach ich safe bald):
                /*
                if (BCrypt.Net.BCrypt.Verify(userPassword, storedHash))
                {
                    DebugHandler.Log($"Login successful for user_id={userId}.");
                    return new Login(userId, loginId, LoginStates.Success);
                }
                */

                DebugHandler.LogError("Login failed: Wrong password.");
                return Login.Error(LoginStates.PasswordIncorrect);
            }
            catch (Exception ex)
            {
                DebugHandler.LogError($"DB-Error during login: {ex.Message}");
                return Login.Error();
            }
        }

        public static Login SaveLogin(string userEmail, string userPassword)
        {
            DebugHandler.seperate();

            if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(userPassword))
            {
                DebugHandler.LogError("SaveLogin() called with empty email or password.");
                return Login.Error();
            }

            if (!Utility.IsValidEmail(userEmail))
            {
                DebugHandler.LogError("SaveLogin() called with invalid email format.");
                return Login.Error(LoginStates.EmailNotFound);
            }

            if (EmailExists(userEmail))
            {
                DebugHandler.LogError("Email for SaveLogin() already exists.");
                return Login.Error();
            }

            var con = connect();
            con.Open();
            var trans = con.BeginTransaction(); // Macht, dass Änderungen erst mit Commit() gespeichert werden. Ist bei crashes wichtig, da die Hälfte noch bestehen bleibt.

            try
            {
                // 1. Address
                const string insertAddress = @"
            INSERT INTO Address (street, number, city, postal)
            VALUES (@street, @number, @city, @postal)";
                var cmdAddress = new MySqlCommand(insertAddress, con, trans);
                cmdAddress.Parameters.AddWithValue("@street", "Hauptstraße");
                cmdAddress.Parameters.AddWithValue("@number", "1");
                cmdAddress.Parameters.AddWithValue("@city", "Berlin");
                cmdAddress.Parameters.AddWithValue("@postal", 10115);
                cmdAddress.ExecuteNonQuery();
                long addressId = cmdAddress.LastInsertedId;

                DebugHandler.Log($"Inserted Address (id={addressId}).");

                // 2. Profile
                const string insertProfile = @"
            INSERT INTO Profil (bio, picture, address_id)
            VALUES (@bio, @picture, @address_id)";
                var cmdProfile = new MySqlCommand(insertProfile, con, trans);
                cmdProfile.Parameters.AddWithValue("@bio", "No Bio");
                cmdProfile.Parameters.AddWithValue("@picture", "");
                cmdProfile.Parameters.AddWithValue("@address_id", addressId);
                cmdProfile.ExecuteNonQuery();
                long profileId = cmdProfile.LastInsertedId;

                DebugHandler.Log($"Inserted Profile (id={profileId}).");

                // 3. User
                const string insertUser = @"
            INSERT INTO `User` (firstname, lastname, birthday, gender, profil_id)
            VALUES (@firstname, @lastname, @birthday, @gender, @profil_id)";
                var cmdUser = new MySqlCommand(insertUser, con, trans);
                cmdUser.Parameters.AddWithValue("@firstname", "User");
                cmdUser.Parameters.AddWithValue("@lastname", "Name");
                cmdUser.Parameters.AddWithValue("@birthday", new DateTime(2000, 1, 1));
                cmdUser.Parameters.AddWithValue("@gender", "m");
                cmdUser.Parameters.AddWithValue("@profil_id", profileId);
                cmdUser.ExecuteNonQuery();
                long userId = cmdUser.LastInsertedId;

                DebugHandler.Log($"Inserted User (id={userId}).");

                // 4. Login
                const string insertLogin = @"
            INSERT INTO Login (user_id, email, password)
            VALUES (@user_id, @email, @password)";
                var cmdLogin = new MySqlCommand(insertLogin, con, trans);
                cmdLogin.Parameters.AddWithValue("@user_id", userId);
                cmdLogin.Parameters.AddWithValue("@email", userEmail);

                // TODO: Passwort hashen lassen
                // string hash = BCrypt.Net.BCrypt.HashPassword(userPassword);
                // cmdLogin.Parameters.AddWithValue("@password", hash);
                cmdLogin.Parameters.AddWithValue("@password", userPassword);

                cmdLogin.ExecuteNonQuery();
                long loginId = cmdLogin.LastInsertedId;

                DebugHandler.Log($"Inserted Login (id={loginId}).");

                // Commit Transaction
                trans.Commit();

                return new Login(userId, loginId, LoginStates.NewUser);
            }
            catch (Exception ex)
            {
                try { trans.Rollback(); } catch { /* ignore rollback errors */ } // Damit der alte Fehler nicht überschrieben wird.
                DebugHandler.LogError($"DB-Error in SaveLogin(): {ex.Message}");
                return Login.Error();
            }
        }
    }

    public class Login
    {
        public long UserID, LoginID;
        public DBHandler.LoginStates State;

        public Login(long userID, long loginID, DBHandler.LoginStates state)
        {
            UserID = userID;
            LoginID = loginID;
            State = state;
        }

        public static Login Error(DBHandler.LoginStates error = DBHandler.LoginStates.Error)
        {
            return new Login(-1, -1, error);
        }
    }

    public static class Utility
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

}
