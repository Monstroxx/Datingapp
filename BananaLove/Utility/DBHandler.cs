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
using System.Printing;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

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
            string server = Environment.GetEnvironmentVariable("DatabaseServerIP") + "";
            string port = Environment.GetEnvironmentVariable("DatabaseServerPort") + "";
            string user = Environment.GetEnvironmentVariable("DatabaseServerProfilename") + "";
            string password = Environment.GetEnvironmentVariable("DatabaseServerPassword") + "";
            string database = Environment.GetEnvironmentVariable("DatabaseName") + "";

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

            string query = "SELECT id, user_id, email, password FROM `Login` WHERE `email` = @userEmail";
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

        /// <summary>
        /// Setzt das Passwort für den gegebenen Login-Eintrag zurück, generiert
        /// ein neues zufälliges Passwort, speichert es in der Datenbank und
        /// verschickt es per E-Mail an den Nutzer.
        /// </summary>
        public static bool ResetPassword(string email)
        {
            DebugHandler.seperate();

            if (string.IsNullOrWhiteSpace(email))
            {
                DebugHandler.LogError("ResetPassword() called with empty email.");
                return false;
            }

            if (!Utility.IsValidEmail(email))
            {
                DebugHandler.LogError("ResetPassword() called with invalid email format.");
                return false;
            }

            using (var con = connect())
            {
                con.Open();

                using (var trans = con.BeginTransaction())
                {
                    try
                    {
                        // Prüfen, ob es den Login gibt
                        const string selectLogin = @"SELECT id, user_id, email FROM Login WHERE email = @email";
                        var cmdSelect = new MySqlCommand(selectLogin, con, trans);
                        cmdSelect.Parameters.AddWithValue("@email", email);

                        long loginId = -1;
                        long userId = -1;

                        using (var reader = cmdSelect.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                DebugHandler.LogError($"ResetPassword(): Kein Login mit Email {email} gefunden.");
                                return false;
                            }

                            loginId = reader.GetInt64("id");
                            userId = reader.GetInt64("user_id");
                        }

                        // Neues Passwort generieren
                        string newPassword = GenerateRandomPassword(12);

                        // Passwort in der Datenbank aktualisieren (aktuell noch im Klartext, wie beim normalen Login)
                        const string updateLogin = @"UPDATE Login SET password = @password WHERE id = @loginId";
                        var cmdUpdate = new MySqlCommand(updateLogin, con, trans);
                        cmdUpdate.Parameters.AddWithValue("@password", newPassword);
                        cmdUpdate.Parameters.AddWithValue("@loginId", loginId);
                        cmdUpdate.ExecuteNonQuery();

                        trans.Commit();

                        DebugHandler.Log($"ResetPassword(): Passwort für user_id={userId} (login_id={loginId}) zurückgesetzt.");

                        // E-Mail mit neuem Passwort senden (Fehler beim Senden sollen die DB-Änderung nicht zurückrollen)
                        MailSender.SendPasswordResetMail(email, newPassword);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        try { trans.Rollback(); } catch { }
                        DebugHandler.LogError($"ResetPassword() DB-Error: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Generiert ein zufälliges Passwort mit der angegebenen Länge.
        /// Enthält Groß-/Kleinbuchstaben, Ziffern und einige Sonderzeichen.
        /// </summary>
        private static string GenerateRandomPassword(int length = 12)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@$?_-";
            var data = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }

            var result = new StringBuilder(length);
            foreach (var b in data)
            {
                result.Append(chars[b % chars.Length]);
            }

            return result.ToString();
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

        public static Login SaveLogin(string userEmail, string userPassword, string userName = "User")
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
            INSERT INTO Address (street, number, city, postal, latitude, longitude)
            VALUES (@street, @number, @city, @postal, @latitude, @longitude)";
                var cmdAddress = new MySqlCommand(insertAddress, con, trans);
                cmdAddress.Parameters.AddWithValue("@street", "Hauptstraße");
                cmdAddress.Parameters.AddWithValue("@number", "1");
                cmdAddress.Parameters.AddWithValue("@city", "Berlin");
                cmdAddress.Parameters.AddWithValue("@postal", 10115);
                cmdAddress.Parameters.AddWithValue("@latitude", 52.52);
                cmdAddress.Parameters.AddWithValue("@longitude", 13.3);
                cmdAddress.ExecuteNonQuery();
                long addressId = cmdAddress.LastInsertedId;

                DebugHandler.Log($"Inserted Address (id={addressId}).");
                
                // 1.1 Preferences

                const string insertPreference = @"
            INSERT INTO Preference (prefers, search_radius)
            VALUES (@prefers, @search_radius);";
                var cmdPreferences = new MySqlCommand(insertPreference, con, trans);
                cmdPreferences.Parameters.AddWithValue("@prefers", "d");
                cmdPreferences.Parameters.AddWithValue("@search_radius", 20);
                cmdPreferences.ExecuteNonQuery();
                long pref_id = cmdPreferences.LastInsertedId;

                // 2. Profile
                const string insertProfile = @"
            INSERT INTO Profil (bio, user_name, picture, address_id)
            VALUES (@bio, @user_name, @picture, @address_id)";
                var cmdProfile = new MySqlCommand(insertProfile, con, trans);
                cmdProfile.Parameters.AddWithValue("@user_name", userName);
                cmdProfile.Parameters.AddWithValue("@bio", "No Bio");
                cmdProfile.Parameters.AddWithValue("@picture", "");
                cmdProfile.Parameters.AddWithValue("@address_id", addressId);
                cmdProfile.ExecuteNonQuery();
                long profileId = cmdProfile.LastInsertedId;

                DebugHandler.Log($"Inserted Profile (id={profileId}).");

                // 3. User
                const string insertUser = @"
            INSERT INTO `User` (firstname, lastname, birthday, gender, profil_id, preference_id)
            VALUES (@firstname, @lastname, @birthday, @gender, @profil_id, @preference_id)";
                var cmdUser = new MySqlCommand(insertUser, con, trans);
                cmdUser.Parameters.AddWithValue("@firstname", "User");
                cmdUser.Parameters.AddWithValue("@lastname", "Name");
                cmdUser.Parameters.AddWithValue("@birthday", new DateTime(2000, 1, 1));
                cmdUser.Parameters.AddWithValue("@gender", "m");
                cmdUser.Parameters.AddWithValue("@profil_id", profileId);
                cmdUser.Parameters.AddWithValue("@preference_id", pref_id);
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

        public static async Task<bool?> UpdateUserData(
            long userId,
            string street,
            string houseNumber,
            string city,
            int postal,
            string gender,
            DateTime birthday,
            string prefers,      // "m", "w", "d"
            int searchRadius,
            string firstname,
            string lastname,
            string bio,
            string username = "UpdatedUser"
        )
        {
            using (var con = connect())
            {
                con.Open();
                var trans = con.BeginTransaction();

                try
                {
                    // 1. IDs des Nutzers abrufen (Profil, Adresse, Preference)
                    const string getIdsQuery = @"
                SELECT 
                    p.id AS profil_id,
                    a.id AS address_id,
                    pref.id AS preference_id
                FROM User u
                JOIN Profil p ON u.profil_id = p.id
                JOIN Address a ON p.address_id = a.id
                JOIN Preference pref ON u.preference_id = pref.id
                WHERE u.id = @userId";
                    var cmdGet = new MySqlCommand(getIdsQuery, con, trans);
                    cmdGet.Parameters.AddWithValue("@userId", userId);
                    long profilId = -1, addressId = -1, prefId = -1;

                    using (var reader = cmdGet.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            profilId = reader.GetInt64("profil_id");
                            addressId = reader.GetInt64("address_id");
                            prefId = reader.GetInt64("preference_id");
                        }
                        else
                        {
                            throw new Exception($"Kein Benutzer mit ID {userId} gefunden.");
                        }
                    }

                    // 2. Adresse updaten
                    const string updateAddress = @"
                UPDATE Address 
                SET street=@street, number=@number, city=@city, postal=@postal, latitude=@latitude, longitude=@longitude
                WHERE id=@id";
                    var cmdAddress = new MySqlCommand(updateAddress, con, trans);
                    cmdAddress.Parameters.AddWithValue("@street", street);
                    cmdAddress.Parameters.AddWithValue("@number", houseNumber);
                    cmdAddress.Parameters.AddWithValue("@city", city);
                    cmdAddress.Parameters.AddWithValue("@postal", postal);
                    cmdAddress.Parameters.AddWithValue("@id", addressId);
                    
                    DebugHandler.Log($"Now update address...");
                    var coords = await Utility.GetCoordsFromPlz("10115");
                    // var coords = Utility.GetCoordsFromPlz(postal.ToString()).GetAwaiter().GetResult();
                    DebugHandler.Log($"Got coords: {coords}");
                    if (coords != null)
                    {
                        var (lat, lon) = coords.Value;
                        cmdAddress.Parameters.AddWithValue("@latitude", lat);
                        cmdAddress.Parameters.AddWithValue("@longitude", lon);
                        DebugHandler.Log($"Updated Address (lat={lat}; lon={lon}).");
                    }
                    else
                    {
                        DebugHandler.LogError($"The coordinates provided are invalid. PLZ: {postal}");
                        throw new KeyNotFoundException("lat and lon not found!");
                    }

                    cmdAddress.ExecuteNonQuery();

                    // 3. Profil updaten
                    const string updateProfile = @"
                UPDATE Profil
                SET user_name=@user_name, bio=@bio
                WHERE id=@id";
                    var cmdProfile = new MySqlCommand(updateProfile, con, trans);
                    cmdProfile.Parameters.AddWithValue("@user_name", username);
                    cmdProfile.Parameters.AddWithValue("@bio", bio);
                    cmdProfile.Parameters.AddWithValue("@id", profilId);
                    cmdProfile.ExecuteNonQuery();

                    // 4. Preference updaten
                    const string updatePreference = @"
                UPDATE Preference
                SET prefers=@prefers, search_radius=@search_radius
                WHERE id=@id";
                    var cmdPref = new MySqlCommand(updatePreference, con, trans);
                    cmdPref.Parameters.AddWithValue("@prefers", prefers);
                    cmdPref.Parameters.AddWithValue("@search_radius", searchRadius);
                    cmdPref.Parameters.AddWithValue("@id", prefId);
                    cmdPref.ExecuteNonQuery();

                    // 5. User updaten
                    const string updateUser = @"
                UPDATE User
                SET firstname=@firstname, lastname=@lastname, birthday=@birthday, gender=@gender
                WHERE id=@userId";
                    var cmdUser = new MySqlCommand(updateUser, con, trans);
                    cmdUser.Parameters.AddWithValue("@firstname", firstname);
                    cmdUser.Parameters.AddWithValue("@lastname", lastname);
                    cmdUser.Parameters.AddWithValue("@birthday", birthday);
                    cmdUser.Parameters.AddWithValue("@gender", gender);
                    cmdUser.Parameters.AddWithValue("@userId", userId);
                    cmdUser.ExecuteNonQuery();

                    trans.Commit();
                    DebugHandler.Log($"User data successfully updated for user_id={userId}");
                    return true;
                }
                catch (Exception ex)
                {
                    DebugHandler.LogError($"DB-Error in UpdateUserData(): {ex.Message}");
                    try { trans.Rollback(); } catch { }
                    return false;
                }
            }
        }
        public static List<string> searchUsers(string target)
        {
            List<string> result = new List<string>();
            var con = connect();
            con.Open();
            string query = "SELECT u.id, p.user_name, p.bio FROM `User` u JOIN `Profil` p ON u.profil_id = p.id WHERE p.user_name LIKE @target";
            var cmd = new MySqlCommand(query, con);
            cmd.Parameters.AddWithValue("@target", "%" + target + "%");
            var reader = cmd.ExecuteReader();
            DebugHandler.seperate();
            while (reader.Read())
            {
                DebugHandler.Log($"Found User: id={reader.GetInt64("id")}, user_name={reader.GetString("user_name")}, bio={reader.GetString("bio")}");
                
                result.Add(reader.GetInt64("id").ToString());
                result.Add(reader.GetString("user_name"));
                result.Add(reader.GetString("bio"));
            }
            return (result);
        }

        public static List<string> get_prefference(long user_id)
        {
            // Diese Funktion liefert alle User zurück, die in den Suchpräferenzen
            // (Geschlecht + Distanz-Radius) des Users mit der gegebenen user_id liegen.
            //
            // Rückgabe-Schema (List<string>):
            // [ id1, gender1, distance1, id2, gender2, distance2, ... ]
            //
            // distanceN = Entfernung in Kilometern zwischen suchendem User (u1) und gefundenem User (u2).

            var result = new List<string>();

            using (var con = connect())
            {
                con.Open();

                // SQL-Logik:
                // - u1 ist der suchende User (mit @user_id)
                // - u2 sind alle anderen User, deren Geschlecht zu u1s Preference (p.prefers) passt
                // - a1/a2 sind die Adressen (mit lat/lon) von u1 bzw. u2
                // - die Distanz wird über die Haversine-Formel berechnet (Erde ~ Kugel, Radius ca. 6371 km)
                // - HAVING filtert alle User heraus, deren Distanz über dem Suchradius liegt
                string query = @"
SELECT 
    u2.id AS target_user_id,
    -- Haversine-Formel: Entfernung (in km) zwischen zwei GPS-Punkten
    (6371 *
        acos(
            cos(radians(a1.latitude)) *
            cos(radians(a2.latitude)) *
            cos(radians(a2.longitude) - radians(a1.longitude)) +
            sin(radians(a1.latitude)) *
            sin(radians(a2.latitude))
        )
    ) AS distance,
    p.search_radius,
    u2.gender AS target_gender
FROM `User` u1
JOIN `Preference` p ON u1.preference_id = p.id       -- Suchpräferenzen des suchenden Users
JOIN `User` u2 ON u2.gender = p.prefers              -- nur User, deren Geschlecht zur Preference passt
JOIN `Profil` pr1 ON u1.profil_id = pr1.id
JOIN `Profil` pr2 ON u2.profil_id = pr2.id
JOIN `Address` a1 ON pr1.address_id = a1.id          -- Adresse (Koordinaten) des suchenden Users
JOIN `Address` a2 ON pr2.address_id = a2.id          -- Adresse (Koordinaten) des anderen Users
WHERE u1.id = @user_id
  AND u2.id != @user_id                               -- sich selbst nicht matchen
HAVING distance <= p.search_radius                    -- nur User innerhalb des Suchradius
ORDER BY distance;                                    -- sortiert nach Entfernung (nächster zuerst)";

                using (var cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@user_id", user_id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        DebugHandler.seperate();

                        while (reader.Read())
                        {
                            long otherUserId = reader.GetInt64("target_user_id");
                            double distance = reader.GetDouble("distance");
                            int radius = reader.GetInt32("search_radius");
                            string gender = reader.GetString("target_gender"); // "m", "w", "d"

                            // distance ist bereits das Ergebnis der Haversine-Formel (siehe SQL oben)
                            // und repräsentiert die Luftlinie in Kilometern zwischen den beiden Adressen.
                            var de = new CultureInfo("de-DE");

                            DebugHandler.Log(
                                $"Found matching user: id={otherUserId}, gender={gender}, distance={distance.ToString("F2", de)}, radius={radius}"
                            );

                            // id, Geschlecht und Distanz als Strings zurückgeben (Tripel)
                            result.Add(otherUserId.ToString());
                            result.Add(gender);
                            result.Add(distance.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
            }

            return result;
        }



        public static Dictionary<string, object> GetUserData(long userId)
        {
            var result = new Dictionary<string, object>();

            using (var con = connect())
            {
                con.Open();

                const string query = @"
            SELECT 
                u.firstname, 
                u.lastname, 
                u.birthday, 
                u.gender,
                p.user_name, 
                p.bio,
                p.picture,
                a.street, 
                a.number, 
                a.city, 
                a.postal,
                pref.prefers, 
                pref.search_radius
            FROM User u
            JOIN Profil p ON u.profil_id = p.id
            JOIN Address a ON p.address_id = a.id
            JOIN Preference pref ON u.preference_id = pref.id
            WHERE u.id = @userId";

                var cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@userId", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception($"Kein Benutzer mit ID {userId} gefunden.");

                    result["firstname"] = reader["firstname"]?.ToString() ?? "";
                    result["lastname"] = reader["lastname"]?.ToString() ?? "";
                    result["birthday"] = reader.GetDateTime("birthday");
                    result["gender"] = reader["gender"]?.ToString() ?? "";
                    result["user_name"] = reader["user_name"]?.ToString() ?? "";
                    result["bio"] = reader["bio"]?.ToString() ?? "";
                    result["picture"] = reader["picture"]?.ToString() ?? "";
                    result["street"] = reader["street"]?.ToString() ?? "";
                    result["number"] = reader["number"]?.ToString() ?? "";
                    result["city"] = reader["city"]?.ToString() ?? "";
                    result["postal"] = Convert.ToInt32(reader["postal"]);
                    result["prefers"] = reader["prefers"]?.ToString() ?? "";
                    result["search_radius"] = Convert.ToInt32(reader["search_radius"]);
                }
            }

            return result;
        }

        public static bool DeleteUser(Login login)
        {
            if (login == null || login.UserID <= 0)
                return false;

            long userId = login.UserID;

            var con = connect();
            con.Open();
            var trans = con.BeginTransaction();

            try
            {
                // 1. Login entfernen
                var cmdLogin = new MySqlCommand(
                    "DELETE FROM Login WHERE id = @loginId",
                    con, trans
                );
                cmdLogin.Parameters.AddWithValue("@loginId", login.LoginID);
                cmdLogin.ExecuteNonQuery();

                // 2. Likes / Matches / Suggests / Interests
                void Exec(string sql)
                {
                    var c = new MySqlCommand(sql, con, trans);
                    c.Parameters.AddWithValue("@uid", userId);
                    c.ExecuteNonQuery();
                }

                Exec("DELETE FROM Likes WHERE user_id = @uid OR liked = @uid");
                Exec("DELETE FROM Matches WHERE first = @uid OR second = @uid");
                Exec("DELETE FROM Suggests WHERE user_id = @uid OR suggestion = @uid");
                Exec("DELETE FROM HasInterests WHERE user_id = @uid");

                // 3. Profile + Address ID holen
                long profileId = -1;
                long addressId = -1;

                var cmdProf = new MySqlCommand(
                    "SELECT profil_id FROM User WHERE id = @uid",
                    con, trans
                );
                cmdProf.Parameters.AddWithValue("@uid", userId);

                var r = cmdProf.ExecuteReader();
                if (r.Read())
                    profileId = r.GetInt64("profil_id");
                r.Close();

                // 4. User löschen
                var cmdUser = new MySqlCommand(
                    "DELETE FROM User WHERE id = @uid",
                    con, trans
                );
                cmdUser.Parameters.AddWithValue("@uid", userId);
                cmdUser.ExecuteNonQuery();

                // 5. Address-ID holen
                if (profileId != -1)
                {
                    var cmdAddrId = new MySqlCommand(
                        "SELECT address_id FROM Profil WHERE id = @pid",
                        con, trans
                    );
                    cmdAddrId.Parameters.AddWithValue("@pid", profileId);

                    var r2 = cmdAddrId.ExecuteReader();
                    if (r2.Read())
                        addressId = r2.GetInt64("address_id");
                    r2.Close();

                    // 6. Profil löschen
                    var cmdP = new MySqlCommand(
                        "DELETE FROM Profil WHERE id = @pid",
                        con, trans
                    );
                    cmdP.Parameters.AddWithValue("@pid", profileId);
                    cmdP.ExecuteNonQuery();
                }

                // 7. Adresse löschen
                if (addressId != -1)
                {
                    var cmdA = new MySqlCommand(
                        "DELETE FROM Address WHERE id = @aid",
                        con, trans
                    );
                    cmdA.Parameters.AddWithValue("@aid", addressId);
                    cmdA.ExecuteNonQuery();
                }

                trans.Commit();
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    trans.Rollback();
                }
                catch
                {
                }

                DebugHandler.LogError("DeleteUser(Login) ERROR: " + ex.Message);
                return false;
            }
            finally
            {
                con.Close();
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

        public override string ToString()
        {
            return $"Login<UserID={UserID}, LoginID={LoginID}, State={State}>";
        }
    }



    public static class Utility
    {
        public static bool IsValidEmail(string email) // Einfache try and catch Anwendung
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
        
        public static async Task<(double lat, double lon)?> GetCoordsFromPlz(string plz)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("BananaLove/0.8"); // Sagen, wer man ist. Damit API antwortet

            var url = $"https://nominatim.openstreetmap.org/search?postalcode={plz}&country=Germany&format=json&limit=1";

            var json = await client.GetStringAsync(url);

            var result = JsonSerializer.Deserialize<List<NominatimResult>>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is null || result.Count == 0)
                return null;

            return (
                double.Parse(result[0].Lat, CultureInfo.InvariantCulture),
                double.Parse(result[0].Lon, CultureInfo.InvariantCulture)
            );
        }

        public record NominatimResult(string Lat, string Lon);


    }
}
