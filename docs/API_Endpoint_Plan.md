# RaceDay API Endpoint Plan

This document defines the planned RESTful API endpoints for the RaceDay Event Management System. The API supports two main roles: Organiser and Participant.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Registers a new RaceDay user account. | Public | First name, last name, email, password, phone number and role | 201 Created with registered user details; 400 for invalid data; 409 if email already exists |
| POST | `/api/auth/login` | Authenticates a user and returns an authentication token. | Public | Email and password | 200 OK with authentication token and user details; 401 for invalid credentials |
| GET | `/api/profile` | Gets the profile of the currently logged-in user. | Organiser / Participant | None | 200 OK with user profile; 401 if not authenticated |
| PUT | `/api/profile` | Updates the profile of the currently logged-in user. | Organiser / Participant | First name, last name and phone number | 200 OK with updated profile; 400 for invalid data; 401 if not authenticated |
| GET | `/api/events` | Gets all events available on RaceDay. | Public | None | 200 OK with list of events |
| GET | `/api/events/{id}` | Gets the details of a specific event. | Public | None | 200 OK with event details; 404 if event does not exist |
| POST | `/api/events` | Creates a new event. | Organiser | Event name, date, location and description | 201 Created with event details; 400 for invalid data; 403 if access is denied |
| PUT | `/api/events/{id}` | Updates an existing event owned by the organiser. | Organiser | Event name, date, location and description | 200 OK with updated event; 400 for invalid data; 403 if access is denied; 404 if event is not found |
| DELETE | `/api/events/{id}` | Deletes an event owned by the organiser. | Organiser | None | 204 No Content; 403 if access is denied; 404 if event is not found |
| GET | `/api/organiser/events` | Gets all events created by the logged-in organiser. | Organiser | None | 200 OK with organiser's events; 403 if access is denied |
| GET | `/api/categories` | Gets all available race categories. | Public | None | 200 OK with list of categories |
| GET | `/api/categories/{id}` | Gets a specific category. | Public | None | 200 OK with category details; 404 if category is not found |
| POST | `/api/categories` | Creates a new race category. | Organiser | Category name and description | 201 Created with category details; 400 for invalid data; 403 if access is denied |
| PUT | `/api/categories/{id}` | Updates an existing race category. | Organiser | Category name and description | 200 OK with updated category; 400 for invalid data; 404 if category is not found |
| DELETE | `/api/categories/{id}` | Deletes a race category when it is not currently in use. | Organiser | None | 204 No Content; 404 if category is not found; 409 if category is currently in use |
| GET | `/api/events/{eventId}/categories` | Gets all categories available for a specific event. | Public | None | 200 OK with event categories; 404 if event is not found |
| POST | `/api/events/{eventId}/categories` | Adds a category to an organiser's event. | Organiser | Category ID | 201 Created; 403 if organiser does not own event; 404 if event/category is not found; 409 if already linked |
| DELETE | `/api/events/{eventId}/categories/{categoryId}` | Removes a category from an event. | Organiser | None | 204 No Content; 403 if access is denied; 404 if relationship is not found |
| POST | `/api/events/{eventId}/enrolments` | Enrols the logged-in participant into a selected event category. | Participant | EventCategory ID | 201 Created with enrolment details; 400 for invalid data; 409 if participant is already enrolled |
| GET | `/api/enrolments/me` | Gets all enrolments belonging to the logged-in participant. | Participant | None | 200 OK with participant's enrolments; 403 if access is denied |
| GET | `/api/enrolments/{id}` | Gets the details of a specific enrolment when the user has permission to view it. | Organiser / Participant | None | 200 OK with enrolment details; 403 if access is denied; 404 if enrolment is not found |
| DELETE | `/api/enrolments/{id}` | Cancels a participant's enrolment when cancellation is permitted. | Participant | None | 204 No Content; 403 if enrolment does not belong to participant; 404 if enrolment is not found |
| GET | `/api/events/{eventId}/enrolments` | Gets all participant enrolments for an organiser's event. | Organiser | None | 200 OK with event enrolments; 403 if organiser does not own event; 404 if event is not found |
| POST | `/api/enrolments/{enrolmentId}/result` | Captures a participant's result for an event. | Organiser | Position, finish time and points | 201 Created with result; 400 for invalid data; 403 if access is denied; 404 if enrolment is not found |
| PUT | `/api/results/{id}` | Updates an existing participant result. | Organiser | Position, finish time and points | 200 OK with updated result; 400 for invalid data; 403 if access is denied; 404 if result is not found |
| GET | `/api/results/{id}` | Gets a specific race result. | Organiser / Participant | None | 200 OK with result details; 403 if access is denied; 404 if result is not found |
| GET | `/api/results/me` | Gets the logged-in participant's personal race results and performance history. | Participant | None | 200 OK with participant's results; 403 if access is denied |
| GET | `/api/events/{eventId}/results` | Gets all captured results for an organiser's event. | Organiser | None | 200 OK with event results; 403 if organiser does not own event; 404 if event is not found |
| DELETE | `/api/results/{id}` | Deletes an incorrectly captured result. | Organiser | None | 204 No Content; 403 if access is denied; 404 if result is not found |

## Role Summary

### Organiser

An Organiser can create, edit and delete events, manage event categories, view participant enrolments and capture or update participant results.

### Participant

A Participant can create an account, browse upcoming events, select a category and enter an event, view their own enrolments and track their personal race results.

## API Design Notes

- All API routes use the `/api/` prefix.
- Public endpoints can be accessed without authentication.
- Protected endpoints require the user to be authenticated.
- Role-based access will be enforced at API level in Part 2.
- EventCategory resolves the many-to-many relationship between Event and Category.
- Enrolment connects a Participant to an EventCategory.
- Result is connected to an Enrolment so that results can only be recorded for participants who entered an event.
- Appropriate HTTP status codes will be returned, including 200 OK, 201 Created, 204 No Content, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found and 409 Conflict.

---

## API Design Notes

The RaceDay API follows RESTful principles and uses resource-based routes.

Authentication endpoints allow users to register and log in. Event endpoints allow users to retrieve event information, while event management operations are restricted to Organisers.

Category endpoints support the management and retrieval of event categories. Enrolment endpoints connect Participants to the event categories they enter.

Result endpoints allow Organisers to capture participant race results, while Participants can retrieve their own performance information.

Role-based access will be enforced at the API level during Part 2 to ensure that Organiser and Participant functionality remains appropriately separated.
