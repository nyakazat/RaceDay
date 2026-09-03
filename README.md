# RaceDay Event Management System

## PROG6212 – Programming 2B

### Part 1: System Planning and Database

RaceDay is a web-based event management system designed for the South African road running, walking and cycling community.

The purpose of the system is to provide a central platform where Event Organisers can create and manage sporting events while Participants can browse available events, enter event categories and track their race results.

Part 1 focuses on planning the RaceDay system before API development begins. It includes the Entity Relationship Diagram (ERD), RESTful API endpoint plan, SQL Server database script and GitHub Actions validation workflow.

---

## User Roles

RaceDay supports two main user roles.

### Organiser

An Organiser is responsible for managing RaceDay events.

Organisers can:

- Create events
- Edit existing events
- Delete events
- Manage event categories
- View participant enrolments
- Capture participant race results
- Update participant results
- View results for their events

### Participant

A Participant uses RaceDay to discover and enter sporting events.

Participants can:

- Register an account
- Log into RaceDay
- View and update their profile
- Browse upcoming events
- View event categories
- Enter an event by selecting a category
- View their own event enrolments
- Track their personal race results and performance history

---

## Part 1 Documentation

All planning documents are stored inside the `/docs` directory.

The folder contains:

- `RaceDay_ERD.png` – Entity Relationship Diagram
- `API_Endpoint_Plan.md` – planned RESTful API endpoints
- `RaceDay_Database.sql` – SQL Server database creation and sample data script

---

## Entity Relationship Diagram

The RaceDay database contains seven main entities:

1. Role
2. AppUser
3. Event
4. Category
5. EventCategory
6. Enrolment
7. Result

The Role entity identifies whether an AppUser is an Organiser or Participant.

An Organiser can create multiple Events.

Events and Categories have a many-to-many relationship. The EventCategory entity resolves this relationship by connecting categories to individual events.

Participants enter events through the Enrolment entity. Each enrolment connects an AppUser to an EventCategory.

A Result is associated with an Enrolment. This ensures that race results can only be captured for participants who have enrolled in an event.

---

## Database

The RaceDay database was designed and tested using Microsoft SQL Server and SQL Server Management Studio (SSMS).

The SQL script:

- Creates the RaceDay database
- Creates all seven entities represented in the ERD
- Defines primary keys
- Defines foreign keys
- Implements NOT NULL constraints
- Implements UNIQUE constraints
- Implements DEFAULT values
- Implements CHECK constraints
- Inserts realistic sample data
- Includes verification queries

The sample database contains:

- 2 roles
- 2 Organisers
- 2 Participants
- 3 Events
- 6 Categories
- Categories assigned to every event
- Sample participant enrolments
- Sample participant results

The script was tested in SSMS and successfully creates and populates the RaceDay database.

---

## API Endpoint Plan

The RESTful API endpoint plan is available in:

`/docs/API_Endpoint_Plan.md`

The API plan covers:

- Authentication
- User profiles
- Events
- Categories
- Event categories
- Event enrolments
- Results

Each endpoint specifies:

- HTTP Method
- Route
- Description
- Required role
- Request body
- Expected HTTP response

Role-based access is planned so that Organiser-only functionality cannot be accessed by Participants.

---

## Repository Structure

```text
RaceDay/
│
├── README.md
│
├── docs/
│   ├── RaceDay_ERD.png
│   ├── API_Endpoint_Plan.md
│   └── RaceDay_Database.sql
│
└── .github/
    └── workflows/
        └── part1-validation.yml
