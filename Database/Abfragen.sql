# Tests
USE imbiss;

SELECT Bezeichnung, Groesse FROM essen WHERE Groesse = "Medium";
SELECT Vorname, Nachname, Abteilung FROM mitarbeiter;


# LH 6-9
USE imbiss;

SELECT Vorname, Nachname FROM kunde; # LH 6
SELECT * FROM kunde; # LH 7
SELECT Vorname, Nachname FROM kunde WHERE Geburtsdatum < "2004-01-01"; # LH 8
SELECT Vorname, Nachname FROM kunde WHERE Vorname LIKE "T%"; # LH 9


# Eigene tests pt 2
SELECT Vorname, Nachname, TIMESTAMPDIFF(YEAR, geburtsdatum, CURDATE()) AS "Alter", 
	CASE WHEN TIMESTAMPDIFF(YEAR, geburtsdatum, CURDATE()) > 18 THEN 'Volljährig' ELSE 'Minderjährig' END AS "Status" FROM kunde;

