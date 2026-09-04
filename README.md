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
- `CI_CD_Green_Build.png` – evidence of a successful GitHub Actions build

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
### Key Relationships

The main relationships in the RaceDay database are:

- One Role can be assigned to many AppUsers.
- One Organiser can create many Events.
- One Event can contain many EventCategories.
- One Category can be used by many EventCategories.
- One Participant can have many Enrolments.
- One EventCategory can have many Enrolments.
- One Enrolment can have a maximum of one Result.

The EventCategory table resolves the many-to-many relationship between Events and Categories.

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
│   ├── RaceDay_Database.sql
│   └── CI_CD_Green_Build.png
│
└── .github/
    └── workflows/
        └── part1-validation.yml
```

---

## Running the Database Script

### Requirements

To run the RaceDay database script you need:

- Microsoft SQL Server
- SQL Server Management Studio (SSMS)

### Instructions

1. Open SQL Server Management Studio.
2. Connect to a SQL Server instance.
3. Open `docs/RaceDay_Database.sql`.
4. Execute the complete SQL script.
5. The script creates the `RaceDay` database.
6. The required tables are created automatically.
7. Sample data is inserted.
8. Verification SELECT queries display the inserted data.

The script is designed to recreate the RaceDay database when executed, allowing it to be tested from a clean state.

---

## CI/CD

GitHub Actions is used to validate the Part 1 repository structure.

The workflow checks that:

- The `/docs` directory exists.
- `RaceDay_ERD.png` exists.
- `API_Endpoint_Plan.md` exists.
- `RaceDay_Database.sql` exists.
- `README.md` exists.
- The SQL script contains the required RaceDay tables.

The workflow is stored at:

`.github/workflows/part1-validation.yml`

### Successful Build

The GitHub Actions workflow successfully validates the RaceDay Part 1 repository structure.

![Successful RaceDay CI/CD Build](docs/CI_CD_Green_Build.png)

---

## Video Presentation

An unlisted YouTube video demonstrating and explaining Part 1 will be added before final submission.

**YouTube Video:** ADD VIDEO LINK HERE

The presentation will demonstrate and explain:

- RaceDay system planning
- ERD design decisions
- Entity relationships
- API endpoint planning
- Role-based access decisions
- SQL database structure
- SQL script execution in SSMS
- Sample data and verification results
- GitHub Actions CI/CD validation

---

## AI Use Disclosure

AI tools were used as a support resource during the planning and development process, including assistance with structuring documentation, reviewing database design, explaining SQL concepts and reviewing code.

The final work was reviewed, tested and understood before submission. Database functionality was tested directly using SQL Server Management Studio.

---

## Author

PROG6212 – Programming 2B  
RaceDay Event Management System  
2026
