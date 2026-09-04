IF DB_ID('RaceDay') IS NOT NULL
    DROP DATABASE RaceDay;
GO

-- creating RaceDay database
create database RaceDay;
--using the database
use RaceDay;

--creating user table
CREATE TABLE [User] (
    user_id INT PRIMARY KEY IDENTITY(1,1),
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);

--creating EventType table
CREATE TABLE EventType (
    event_type_id INT PRIMARY KEY IDENTITY(1,1),
    type_name VARCHAR(20) NOT NULL
);

--creating event table
CREATE TABLE Event (
    event_id INT PRIMARY KEY IDENTITY(1,1),
    organiser_id INT NOT NULL,
    event_type_id INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500) NOT NULL,
    event_date DATE NOT NULL,
    location VARCHAR(150) NOT NULL,
    distance_km DECIMAL(5,2) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Open',
    FOREIGN KEY (organiser_id) REFERENCES [User](user_id),
    FOREIGN KEY (event_type_id) REFERENCES EventType(event_type_id)
);

--creating category table
CREATE TABLE Category (
    category_id INT PRIMARY KEY IDENTITY(1,1),
    event_id INT NOT NULL,
    category_name VARCHAR(50) NOT NULL,
    FOREIGN KEY (event_id) REFERENCES Event(event_id)
);

--creating enrolment table
CREATE TABLE Enrolment (
    enrolment_id INT PRIMARY KEY IDENTITY(1,1),
    participant_id INT NOT NULL,
    event_id INT NOT NULL,
    category_id INT NOT NULL,
    enrolment_date DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (participant_id) REFERENCES [User](user_id),
    FOREIGN KEY (event_id) REFERENCES Event(event_id),
    FOREIGN KEY (category_id) REFERENCES Category(category_id)
);

--creating result table
CREATE TABLE Result (
    result_id INT PRIMARY KEY IDENTITY(1,1),
    participant_id INT NOT NULL,
    event_id INT NOT NULL,
    finish_time TIME NOT NULL,
    finishing_position INT NOT NULL,
    FOREIGN KEY (participant_id) REFERENCES [User](user_id),
    FOREIGN KEY (event_id) REFERENCES Event(event_id)
);

--inserting into user table
INSERT INTO [User] (full_name, email, password_hash, role)
VALUES
('Thabo Nkosi', 'thabo.nkosi@raceday.co.za', 'hashed_password_1', 'Organiser'),
('Lindiwe Dube', 'lindiwe.dube@raceday.co.za', 'hashed_password_2', 'Organiser'),
('Sipho Mahlangu', 'sipho.mahlangu@raceday.co.za', 'hashed_password_3', 'Participant'),
('Naledi Khumalo', 'naledi.khumalo@raceday.co.za', 'hashed_password_4', 'Participant');


--insering values into eventType table
INSERT INTO EventType (type_name)
VALUES
('Run'),
('Walk'),
('Cycle');

--inserting values into event table
INSERT INTO Event (organiser_id, event_type_id, name, description, event_date, location, distance_km, status)
VALUES
(1, 1, 'Johannesburg City Run', 'A scenic 10km run through the streets of Johannesburg.', '2026-11-15', 'Johannesburg, Gauteng', 10.00, 'Open'),
(1, 3, 'Soweto Cycle Challenge', 'A challenging 42km cycling route through Soweto.', '2026-11-22', 'Soweto, Gauteng', 42.00, 'Open'),
(2, 2, 'Durban Beachfront Walk', 'A relaxed 5km walk along the Durban beachfront.', '2026-12-05', 'Durban, KwaZulu-Natal', 5.00, 'Open');




--inserting into category table

INSERT INTO Category (event_id, category_name)
VALUES
(1, 'Under 20'),
(1, 'Senior'),
(1, '10km Open'),
(2, '42km Open'),
(2, 'Veteran'),
(3, '5km Open');


--inserting into enrolment table
INSERT INTO Enrolment (participant_id, event_id, category_id)
VALUES
(3, 1, 3),
(4, 1, 1),
(3, 2, 4),
(4, 3, 6);


--inserting into result table

INSERT INTO Result (participant_id, event_id, finish_time, finishing_position)
VALUES
(3, 1, '00:52:30', 1),
(4, 1, '00:58:15', 2);













