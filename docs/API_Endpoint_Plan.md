# RaceDay — API Endpoint Plan (Part 1, Section B)

This document lists every API endpoint the RaceDay system will expose, planned ahead of implementation in Part 2. All routes are prefixed with `/api/`.

---

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Creates a new user account with a specified role | None (public) | `{ full_name, email, password, role }` | 201 Created – new user record.<br>400 Bad Request – invalid input.<br>409 Conflict – email already exists |
| POST | /api/auth/login | Authenticates a user and returns an access token | None (public) | `{ email, password }` | 200 OK – auth token + user info.<br>401 Unauthorized – invalid credentials |

## User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | Retrieves the logged-in user's own profile information | Any (logged in) | None | 200 OK – user profile data.<br>401 Unauthorized – not logged in |
| PUT | /api/users/me | Updates the logged-in user's own profile information | Any (logged in) | `{ full_name, email }` | 200 OK – updated profile.<br>400 Bad Request – invalid input.<br>401 Unauthorized – not logged in |

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events | Creates a new event | Organiser | `{ name, description, event_date, location, distance_km, event_type_id }` | 201 Created – new event record.<br>400 Bad Request – invalid input.<br>401 Unauthorized |
| GET | /api/events | Retrieves a list of all events | Any (logged in) | None | 200 OK – list of events |
| GET | /api/events/{id} | Retrieves details of a single event | Any (logged in) | None | 200 OK – event details.<br>404 Not Found – event does not exist |
| PUT | /api/events/{id} | Updates an existing event | Organiser (must own the event) | `{ name, description, event_date, location, distance_km, event_type_id, status }` | 200 OK – updated event.<br>403 Forbidden – not the owning Organiser.<br>404 Not Found |
| DELETE | /api/events/{id} | Deletes an event | Organiser (must own the event) | None | 200 OK / 204 No Content.<br>403 Forbidden – not the owning Organiser.<br>404 Not Found |

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events/{id}/categories | Creates a new category for a specific event | Organiser (must own the event) | `{ category_name }` | 201 Created – new category record.<br>403 Forbidden – not the owning Organiser.<br>404 Not Found – event does not exist |
| GET | /api/events/{id}/categories | Retrieves all categories for a specific event | Any (logged in) | None | 200 OK – list of categories.<br>404 Not Found – event does not exist |

## Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events/{id}/enrolments | Enrols the logged-in Participant into the event under a chosen category | Participant | `{ category_id }` | 201 Created – new enrolment record.<br>400 Bad Request – category does not belong to this event.<br>404 Not Found – event or category not found.<br>409 Conflict – already enrolled |
| GET | /api/events/{id}/enrolments | Retrieves all enrolments for a specific event | Organiser (must own the event) | None | 200 OK – list of enrolments.<br>403 Forbidden – not the owning Organiser.<br>404 Not Found |

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events/{id}/results | Captures a Participant's finish time and position for an event | Organiser (must own the event) | `{ participant_id, finish_time, finishing_position }` | 201 Created – new result record.<br>400 Bad Request – invalid input.<br>403 Forbidden – not the owning Organiser.<br>404 Not Found |
| GET | /api/results/me | Retrieves the logged-in Participant's own results across all events | Participant | None | 200 OK – list of results.<br>401 Unauthorized |

---

**Total endpoints: 14**
