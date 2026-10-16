# RaceDay Event Management System
## PROG6212 – Programming 2B | Part 2: RESTful API Development

### Project Overview

RaceDay is an event management RESTful API developed using ASP.NET Core, C#, Entity Framework Core and Microsoft SQL Server.

The system supports sporting events such as running, walking and cycling competitions. It allows Organisers to create and manage events, assign event categories, monitor participant enrolments and record race results.

Participants can register accounts, log in, browse events, enrol in event categories and view their own race results.

Part 2 builds on the database design and API planning completed in Part 1.

### Technologies Used

- C# and ASP.NET Core Web API
- .NET 8
- Entity Framework Core 8
- Microsoft SQL Server
- Visual Studio 2022
- Swagger / OpenAPI
- xUnit testing framework
- Entity Framework Core InMemory testing provider
- Git and GitHub
- GitHub Actions CI/CD

### Project Structure

- `RaceDayAPI/` – Main RESTful API project, including controllers, models, database context and migrations
- `RaceDayAPI.Tests/` – Automated xUnit test project
- `RaceDayAPI.sln` – Visual Studio solution file (or equivalent `.slnx` file)

### User Roles

**Organiser**

Organisers are responsible for creating and managing events, assigning categories, viewing enrolments for their events, and recording and updating participant results.

**Participant**

Participants can register, log in, browse events, enrol in categories, update their own enrolment information and view their personal race results.

### Authentication and Security

RaceDay implements cookie-based authentication and server-side session management.

Passwords are hashed using ASP.NET Core's PasswordHasher. Authenticated sessions maintain the user ID and role.

Role-based authorization uses the Organiser and Participant roles. Ownership checks restrict access to private enrolment and result information.

### RESTful API Endpoints

| Controller | Endpoints |
|---|---|
| Auth | Register, login, logout, current user |
| Events | GET, GET by ID, POST, PUT, DELETE |
| Categories | GET, GET by ID, POST, PUT, DELETE |
| EventCategories | GET, GET by ID, GET by event, POST, DELETE |
| Enrolments | GET, GET my enrolments, GET by ID, POST, PUT, DELETE |
| Results | GET, GET my results, GET by ID, POST, PUT, DELETE |
| UserProfiles | GET my profile, PUT my profile |

API endpoints are documented and tested using Swagger.

### Database Implementation

The database contains seven main entities:

1. Role
2. AppUser
3. Event
4. Category
5. EventCategory
6. Enrolment
7. Result

Entity Framework Core is used to map these models to SQL Server tables and manage database migrations.

Relationships and constraints support enrolment integrity, event ownership and one race result per enrolment.

### Running the Application

1. Open the RaceDayAPI solution using Visual Studio 2022.
2. Ensure the .NET 8 SDK and SQL Server LocalDB or a compatible SQL Server instance are installed.
3. Configure the `RaceDayConnection` connection string for the local database.
4. Restore NuGet dependencies.
5. Apply the Entity Framework Core migrations to the database.
6. Run the RaceDayAPI project using the HTTPS profile.
7. Open Swagger using the local `/swagger` address.

### Automated Testing

RaceDay includes an xUnit test project with 17 locally executed tests covering request validation, registration, password hashing, duplicate-email handling, login rejection, enrolment creation, duplicate enrolment prevention, and selected authorization and ownership checks.

**Local test results:**
- Passed: 17
- Failed: 0

The main race workflow was also manually tested through Swagger, including event creation, category assignment, participant enrolment, result recording and participant result retrieval.

### GitHub Actions CI/CD

GitHub Actions is configured to automatically restore dependencies, build the project and run automated tests.

The Part 2 workflow is located at:

`.github/workflows/part2-api.yml`

The workflow has completed successfully with a green checkmark.

### Demonstration Video

An unlisted YouTube video demonstrating Part 2 will be linked here after recording.

**Part 2 Video:** [ADD YOUTUBE VIDEO LINK]

### AI Use Disclosure

AI tools were used as a development support resource for explaining programming concepts, reviewing C# code, assisting with controller implementation, diagnosing errors, and preparing tests and documentation.

The API was built and tested in Visual Studio, with endpoint testing conducted in Swagger and automated testing conducted using xUnit and GitHub Actions.

### Repository

https://github.com/nyakazat/RaceDay

### Author

Thimna Nyakaza

PROG6212 – Programming 2B

2026
