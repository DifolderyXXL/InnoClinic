# InnoClinic Microservice Platform

**InnoClinic** is a distributed, event-driven healthcare management system built on **.NET 10** following **Clean Architecture** principles.

## Global Architecture & Shared Tech Stack

* **Core Platform:** .NET 10 / ASP.NET Core
* **Messaging & Event-Driven Integration:** MassTransit with RabbitMQ
* **Orchestration & Service Discovery:** .NET Aspire
* **Security & Access Control:** OAuth 2.0 / OpenID Connect (OIDC), Duende IdentityServer, Policy-Based Authorization
* **API Gateway & Frontend Pattern:** Backend-For-Frontend (BFF) with YARP Reverse Proxy and HTTP-only session cookies

---

### ServicesAPI

Backend service responsible for managing the medical service catalog, specializations, and patient scheduling within **InnoClinic**.

* **Database:** PostgreSQL (`btree_gist` extension)
* **Key Capabilities:**
  * **Service Catalog:** Full management of services, categories, and medical specializations.
  * **Scheduling & Reservations:** Slot calculation and overlap-proof time reservations using PostgreSQL exclusion constraints (`btree_gist`).
  * **Reservation Lifecycle:** Event-driven handling of confirmations, cancellations, rescheduling, and expirations.


* **MassTransit Events:** `ReservationSubmitted`, `ReservationConfirmed`, `ReservationCancelled`, `ReservationRescheduled`, `ReservationExpired`
* **Key Endpoints:**
  * `/api/v1/schedules/available-positions`
  * `/api/v1/services`
  * `/api/v1/categories`
  * `/api/v1/specializations`



---

### AppointmentsAPI

Backend service responsible for appointment booking, and doctor schedules within **InnoClinic**.

* **Database:** PostgreSQL
* **Key Capabilities:**
  * **Appointment Booking:** Patient self-booking and admin booking with cross-service validation (verifying doctor, patient, service, and specialization alignment via `Profiles` and `Services` clients).
  * **Saga Orchestration (LLT):** Uses MassTransit Saga State Machine to manage Long-Lived Transactions—coordinating temporary slot reservations, awaiting administrative approval, and executing final reservation state updates.
  * **Lifecycle & Rescheduling:** Asynchronous workflow handling for booking approvals, rejections, cancellations, and reschedule requests.
  * **Schedule Management:** Queries for daily and dated schedules tailored for doctors and client portals.
  * **Account Data Cleanup:** System-level endpoints to purge appointments upon user deletion.


* **MassTransit Events:** `AppointmentSubmitted`, `AppointmentRescheduleRequested`, `AppointmentApproved`, `AppointmentDeclined`
* **Key Endpoints:**
  * `/api/v1/appointments` (booking & patient/clinic listings)
  * `/api/v1/appointments/{id}/approve`, `/api/v1/appointments/{id}/decline`
  * `/api/v1/appointments/{id}/reschedule/me`
  * `/api/v1/schedule/me`, `/api/v1/schedule/today/me`



---

### DocumentsAPI

Backend service responsible for medical examination records, PDF report generation, and media storage (avatars and photos) within **InnoClinic**.

* **Database & Storage:** MongoDB, Azure Blob Storage, Redis (Distributed Lock)
* **Key Capabilities:**
  * **Medical Results Management:** MongoDB-backed storage for raw diagnostic data (complaints, diagnoses, conclusions, and recommendations).
  * **Lazy PDF Generation & Distributed Locking:** On-demand PDF report rendering backed by Redis distributed locking to prevent redundant computations—ensuring a single service instance generates and uploads the file to Azure Blob Storage while concurrent client requests wait.
  * **Two-Stage Distributed Media Upload:** Two-step upload architecture where photos are initially staged in a temporary Azure Blob container (30-minute TTL) and only transferred to persistent storage upon cross-service confirmation.
  * **Performance & Caching:** HTTP Response Caching for public and doctor media assets to minimize storage I/O and latency.
  * **User Data Cleanup:** System-level routines to purge all documents and binary blobs when user accounts are deleted.


* **MassTransit Events:** `MedicalResultUpdatedIntegrationEvent`
* **Key Endpoints:**
  * `/api/v1/medicalresults/appointments/{appointmentId}` (medical data CRUD & PDF exports)
  * `/api/v1/photos/users/avatar` (user profile photo uploads)
  * `/api/v1/photos/offices/{officeId}/avatar` (office photo management & confirmation)
  * `/api/v1/photos/doctors/{doctorId}/avatar/{photoId}` (doctor avatar retrieval)



---

### OfficesAPI

Backend service managing clinic branch offices, location data, registry contact details, and operational statuses within **InnoClinic**.

* **Database:** MongoDB
* **Key Capabilities:**
  * **Office Management:** CRUD operations for medical facility profiles (city, street, registry phone, and active status control).
  * **Distributed Photo Confirmation:** Triggers cross-service calls to `DocumentsAPI` (`DocumentsClient`) upon office creation or update to transition staged office avatars from temp to persistent storage.
  * **Document Persistence:** Schema-flexible MongoDB storage for branch office metadata and status configurations.


* **Key Endpoints:**
  * `/api/v1/offices`
  * `/api/v1/offices/{id}`



---

### ProfilesAPI

Backend service managing user account profiles, role-specific medical personas (Patients, Doctors, Receptionists), and cross-service profile validations within **InnoClinic**.

* **Database:** MS SQL SERVER
* **Key Capabilities:**
  * **Multi-Role Profile Management:** Unified identity management handling base account data alongside distinct role personas: Patients, Doctors, and Receptionists.
  * **Doctor Directory Search:** Advanced lookup and paginated searching of doctor profiles filtered by operational status, assigned offices, specializations, and full names.
  * **Cross-Service Profile Validation:** Inter-service endpoint (`validate-profile`) used by other microservices to verify doctor, patient, and office compatibility before executing bookings.
  * **Account Lifecycle & Administration:** User self-management (`/me` routes), administrative user provisioning with role assignments, and account data deletion routines.


* **Key Endpoints:**
  * `/api/v1/profiles/me`, `/api/v1/profiles/validate-profile`
  * `/api/v1/accounts`, `/api/v1/accounts/me`
  * `/api/v1/doctors`, `/api/v1/doctors/{id}`
  * `/api/v1/patients`, `/api/v1/receptionists`



---

### NotificationService.Worker

Asynchronous event processing service responsible for consuming system events and dispatching transactional email notifications within **InnoClinic**.

* **Key Capabilities:**
  * **Transactional Email Delivery:** Asynchronous rendering and dispatching of HTML emails for account onboarding, booking confirmations, and medical updates.
  * **Cross-Service Data Enrichment:** Integrates with `AppointmentsAPI` via an internal HTTP client (`AppointmentApiClient`) to query supplementary appointment details before dispatching medical result emails.


* **MassTransit Events (Consumed):**
  * `UserRegisteredIntegrationEvent`
  * `UserAppointmentConfirmedIntegrationEvent`
  * `MedicalResultUpdatedIntegrationEvent`

---

### IdentityAPI

Central authentication and authorization server built on Duende IdentityServer, providing OpenID Connect and OAuth 2.0 identity management within **InnoClinic**.

* **Database:** SQLite (Development)
* **Key Capabilities:**
  * **Identity Provider (IdP):** Issues OAuth 2.0 and OpenID Connect tokens to secure API resources and client applications.
  * **EF Core Store Persistence:** Entity Framework Core integration for persisting IdentityServer operational and configuration data.
  * **Admin & Quickstart UI:** Styled user interface built with Bootstrap 5 for managing clients, scopes, user claims, and authentication flows.
  * **Automated Data Seeding:** Bootstrapping logic to seed baseline configuration data, OAuth clients, and default accounts on launch.


* **Key Endpoints:**
  * `/connect/authorize`, `/connect/token`, `/connect/userinfo`



---

### BFF (Backend-For-Frontend Gateway)

API gateway acting as a secure bridge between single-page frontend applications and backend microservices within **InnoClinic**.

* **Storage / Cache:** Redis (User Revocation Store)
* **Key Capabilities:**
  * **Token Handler Pattern (BFF Security):** Replaces browser-side token storage with encrypted HTTP-only session cookies (`bff-local-session`), converting cookie sessions into OAuth2 Bearer Access Tokens on the server before forwarding requests to microservices via YARP.
  * **Unified OpenAPI / Swagger UI Aggregation:** Dynamically discovers backend microservices using .NET Aspire Service Discovery and aggregates multi-versioned OpenAPI schemas into a single Swagger UI dashboard with automated CSRF header injection (`X-CSRF: 1`).
  * **Instant User Revocation Barrier:** Consumes user deletion events (`UserDeletedBlockingConsumer`) via MassTransit, storing revoked user states in Redis (`RedisRevokedUserRepository`) and enforcing session blocking via custom middleware (`DeletedUserBarrierMiddleware`).
  * **SPA Development Proxy:** Forwards non-API SPA traffic directly to the Vite frontend server (`vite-frontend`) for a single-origin architecture.


* **MassTransit Events (Consumed):** Event consumed by `UserDeletedBlockingConsumer` (invalidates deleted/revoked user sessions in Redis).
* **Key Endpoints:**
  * `/login` (OIDC authorization trigger)
  * `/swagger/index.html` (centralized API documentation dashboard)
  * Reverse-proxied microservice routes (`{service}/api/v1/*`)
  * Catch-all SPA route fallback (`/{*rest}`)