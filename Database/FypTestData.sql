-- Zusätzliche Testdaten für die FYP-Funktion
-- Diese Datei NACH Database.sql ausführen.

USE DatingApp;

-- ----------------------------------------------------------
-- Zusätzliche Adressen (mit Koordinaten rund um Köln / NRW
-- und ein paar weiter entfernte Städte für Radius-Tests)
-- ----------------------------------------------------------

INSERT INTO `Address` (`id`, `street`, `number`, `city`, `postal`, `latitude`, `longitude`) VALUES
    (3, "Domstraße", "10", "Köln",       50667, 50.941300, 6.958300),  -- nahe Köln Zentrum
    (4, "Rheinweg",  "3",  "Düsseldorf", 40210, 51.227700, 6.773500),  -- Düsseldorf
    (5, "Altstadt",  "7",  "Leverkusen", 51373, 51.045900, 7.019200),  -- Leverkusen
    (6, "Markt",     "1",  "Aachen",     52062, 50.775300, 6.083900),  -- Aachen (weiter weg)
    (7, "Unter den Linden", "1", "Berlin", 10117, 52.520000, 13.405000), -- Berlin (sehr weit)
    (8, "Marienplatz",      "1", "München", 80331, 48.137400, 11.575500); -- München (sehr weit)

-- ----------------------------------------------------------
-- Profile zu den neuen Adressen
-- ----------------------------------------------------------

INSERT INTO `Profil` (`id`, `user_name`, `bio`, `picture`, `address_id`) VALUES
    (3, "CologneGirl",   "Mag Kino und Kaffee.",                 "", 3),
    (4, "DdorfGuy",      "Reist gerne durch Europa.",            "", 4),
    (5, "LeverkusenGal", "Sportlich unterwegs und offen.",       "", 5),
    (6, "AachenDude",    "Tech-Nerd und Gamer.",                 "", 6),
    (7, "BerlinGirl",    "Berliner Schnauze & Streetfood-Fan.",  "", 7),
    (8, "MunichGuy",     "Liebt Berge und den FCB.",            "", 8);

-- ----------------------------------------------------------
-- User (achten auf konsistente IDs und Preference-IDs)
-- Wir nutzen hier bestehende Preference-Einträge:
--   1: prefers = "w", search_radius = 50
--   2: prefers = "m", search_radius = 30
-- ----------------------------------------------------------

INSERT INTO `User` (`id`, `firstname`, `lastname`, `birthday`, `gender`, `profil_id`, `preference_id`) VALUES
    (3, "Anna",   "Klein",   "1998-05-10", "w", 3, 1),  -- Frau in Köln, sucht Frauen im Umkreis 50km
    (4, "Ben",    "Müller",  "1997-03-22", "m", 4, 1),  -- Mann in Düsseldorf
    (5, "Clara",  "Schmidt", "1999-11-01", "w", 5, 1),  -- Frau in Leverkusen, nahe Köln
    (6, "Daniel", "Fischer", "1995-07-19", "m", 6, 1),  -- Mann in Aachen (etwas weiter weg)
    (7, "Eva",    "Schulz",  "2000-02-14", "w", 7, 1),  -- Frau in Berlin (weit weg)
    (8, "Felix",  "Maier",   "1996-09-30", "m", 8, 2);  -- Mann in München, sucht Männer im Umkreis 30km

-- ----------------------------------------------------------
-- Login-Daten für die neuen User
-- (Passwörter hier noch im Klartext wie in Database.sql)
-- ----------------------------------------------------------

INSERT INTO `Login` (`id`, `user_id`, `email`, `password`) VALUES
    (3, 3, "anna.klein@example.com",   "Passwort123!"),
    (4, 4, "ben.mueller@example.com",  "Passwort123!"),
    (5, 5, "clara.schmidt@example.com","Passwort123!"),
    (6, 6, "daniel.fischer@example.com","Passwort123!"),
    (7, 7, "eva.schulz@example.com",   "Passwort123!"),
    (8, 8, "felix.maier@example.com",  "Passwort123!");


