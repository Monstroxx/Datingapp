DROP DATABASE IF EXISTS DatingApp;
CREATE DATABASE DatingApp;

USE DatingApp;

-- DATA BASES -------------------------------------------

CREATE TABLE `Profil` (
    `id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `user_name` VARCHAR(45),
    `bio` VARCHAR(150),
    `picture` VARCHAR(255), -- Path to it '/uploads/profilepictures/user123.jpg' or Empty
    `address_id` INT
)

-- User Table
CREATE TABLE `User` (
    `id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `firstname` VARCHAR(45),
    `lastname` VARCHAR(45),
    `birthday` DATE,
    `gender` VARCHAR(1),
    `profil_id` INT,
    `preference_id` INT
)

CREATE TABLE `Address` (
    `id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `street` VARCHAR(45),
    `number` VARCHAR(10),
    `city` VARCHAR(45),
    `postal` INT NOT NULL
)

CREATE TABLE `Login` (
    `id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `user_id` INT,
    `email` VARCHAR(320),
    `password` VARCHAR(32)
)

CREATE TABLE `Interests` (
    `id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `tag` VARCHAR(45)
)

CREATE TABLE `HasInterests` (
    `id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `user_id` INT,
    `interest` INT
)

CREATE TABLE `Matches` (
    `id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `first` INT,
    `second` INT
)

CREATE TABLE `Likes` (
    `id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `user_id` INT,
    `liked` INT
)

CREATE TABLE `Suggests` (
    `id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `user_id` INT,
    `suggestion` INT
)

CREATE TABLE `Preference` (
    id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    `prefers` VARCHAR(1),
    `search_radius` INT
);

-- TEST USERS -------------------------------------------

INSERT INTO `DatingApp`.`Profil` VALUES 
    (1, "User1", "Ein Test!", "", 1),
    (2, "User_2", "Ein Test!", "", 2);

INSERT INTO `DatingApp`.`User` VALUES
    (1, "Test", "Person", "2004-12-04", "M", 1, 1),
    (2, "Testing", "Persona", "2003-07-23", "W", 2, 1);

INSERT INTO `DatingApp`.`Address` VALUES
    (1, "Hauptstraße", "5a", "Köln", 51385),
    (2, "Nebenweg", "12", "Bonn", 52948);

INSERT INTO `DatingApp`.`Login` VALUES
    (1, 1, "test.person@example.com", "Password!"),
    (2, 2, "testing.persona@example.com", "Passwort123!");

INSERT INTO `DatingApp`.`Interests` VALUES
    (1, "Sports"),
    (2, "Nice People")

INSERT INTO `DatingApp`.`HasInterests` VALUES
    (1,1,1),
    (2,1,2),
    (3,2,1)

INSERT INTO `DatingApp`.`Matches` VALUES
    (1, 1, 2);

INSERT INTO `DatingApp`.`Likes` VALUES
    (1, 1, 2),
    (2, 2, 1);

INSERT INTO `DatingApp`.`Suggests` VALUES
    (1, 1, 2),
    (2, 2, 1);

INSERT INTO `DatingApp`.`Preference` VALUES
    (1, "w", 50),
    (2, "m", 30);

-- FOREGIN KEYS -------------------------------------------

ALTER TABLE `User` -- Users to Profile
ADD CONSTRAINT profile_id
FOREIGN KEY (`profil_id`) REFERENCES Profil(`id`);

ALTER TABLE `Login` -- Login to User
ADD CONSTRAINT login_user_id
FOREIGN KEY (`user_id`) REFERENCES User(`id`);

-- Matches
ALTER TABLE `Matches` -- Matches to first User
ADD CONSTRAINT matches_first_user
FOREIGN KEY (`first`) REFERENCES User(`id`);

ALTER TABLE `Matches` -- Matches to second User
ADD CONSTRAINT matches_second_user
FOREIGN KEY (`second`) REFERENCES User(`id`);

-- Likes
ALTER TABLE `Likes` -- Likes to `from` User
ADD CONSTRAINT likes_first_user
FOREIGN KEY (`user_id`) REFERENCES User(`id`);

ALTER TABLE `Likes` -- Likes to `to` User
ADD CONSTRAINT likes_second_user
FOREIGN KEY (`liked`) REFERENCES User(`id`);

-- Suggests
ALTER TABLE `Suggests` -- Suggests to from User
ADD CONSTRAINT suggests_first_user
FOREIGN KEY (`user_id`) REFERENCES User(`id`);

ALTER TABLE `Suggests` -- Suggests to to User
ADD CONSTRAINT suggests_second_user
FOREIGN KEY (`suggestion`) REFERENCES User(`id`);

ALTER TABLE `HasInterests` -- interested user to to User
ADD CONSTRAINT interested_user
FOREIGN KEY (`user_id`) REFERENCES User(`id`);

ALTER TABLE `HasInterests` -- interest to to interest
ADD CONSTRAINT interestion
FOREIGN KEY (`interest`) REFERENCES Interests(`id`);

ALTER TABLE `Profil` -- Profile to Address
ADD CONSTRAINT profile_address
FOREIGN KEY (`address_id`) REFERENCES Address(`id`);

ALTER TABLE `User` -- User to Preference
ADD CONSTRAINT user_preference
FOREIGN KEY (`preference_id`) REFERENCES Preference(`id`);
