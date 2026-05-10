# 📘 Product Requirements Document (PRD)
## Student Management System — Zest India IT Pvt Ltd
**Version:** 1.0.0 | **Date:** May 2026 | **Stack:** ASP.NET Core · SQL Server · Angular/React (Optional)

---

## 1. Overview

### 1.1 Purpose
This document defines the complete product requirements for the **Student Management System (SMS)** — a RESTful Web API built with ASP.NET Core 8. The system enables secure CRUD operations on student records, enforces JWT-based authentication, follows layered architecture principles, and is production-ready with logging, error handling, and Swagger documentation.

### 1.2 Goals
- Deliver a fully working, secured REST API for student data management
- Follow industry-standard patterns: layered architecture, DI, repository pattern
- Demonstrate code quality, security awareness, and documentation discipline
- (Optional) Provide a frontend UI and Docker support

---

## 2. Stakeholders

| Role | Responsibility |
|---|---|
| Developer | Build, test, and deliver the system |
| Evaluator (Zest India IT) | Review code quality, architecture, and functionality |
| End User (Admin) | Manage student records via API or UI |

---

## 3. Tech Stack

| Layer | Technology |
|---|---|
| Backend Framework | ASP.NET Core 8 Web API |
| Language | C# 12 |
| Database | SQL Server 2019+ |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens |
| Logging | Serilog (Console + File sinks) |
| API Documentation | Swagger / Swashbuckle |
| Testing (Optional) | xUnit + Moq |
| Containerization (Optional) | Docker + Docker Compose |
| Frontend (Optional) | Angular 17 or React 18 |

---

## 4. System Architecture

### 4.1 Layered Architecture

```
┌────────────────────────────────────────────────┐
│              Presentation Layer                │
│        ASP.NET Core Web API Controllers        │
│     (AuthController, StudentsController)       │
├────────────────────────────────────────────────┤
│                Service Layer                   │
│        Business Logic + Validation             │
│    (IStudentService, IAuthService + impls)     │
├────────────────────────────────────────────────┤
│              Repository Layer                  │
│       Data Access via Entity Framework         │
│   (IStudentRepository + implementation)        │
├────────────────────────────────────────────────┤
│               Database Layer                   │
│             SQL Server + EF Core               │
└────────────────────────────────────────────────┘
```

### 4.2 Project Structure

```
StudentManagementSystem/
├── SMS.API/                          # Web API project
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── StudentsController.cs
│   ├── Middleware/
│   │   └── GlobalExceptionMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── SMS.Application/                  # Service layer
│   ├── Interfaces/
│   │   ├── IStudentService.cs
│   │   └── IAuthService.cs
│   ├── Services/
│   │   ├── StudentService.cs
│   │   └── AuthService.cs
│   └── DTOs/
│       ├── StudentDto.cs
│       ├── CreateStudentDto.cs
│       ├── UpdateStudentDto.cs
│       └── LoginDto.cs
│
├── SMS.Domain/                       # Entities / domain models
│   └── Entities/
│       ├── Student.cs
│       └── User.cs
│
├── SMS.Infrastructure/               # EF Core + repositories
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Repositories/
│   │   └── StudentRepository.cs
│   └── Migrations/
│
├── SMS.Tests/                        # [Optional] Unit tests
│   ├── Services/
│   │   └── StudentServiceTests.cs
│   └── Controllers/
│       └── StudentsControllerTests.cs
│
├── frontend/                         # [Optional] Angular or React
├── docker-compose.yml                # [Optional]
├── .gitignore
└── README.md
```

---

## 5. Database Design

### 5.1 Students Table

| Column | Type | Constraints |
|---|---|---|
| `Id` | INT | PRIMARY KEY, IDENTITY(1,1) |
| `Name` | NVARCHAR(100) | NOT NULL |
| `Email` | NVARCHAR(150) | NOT NULL, UNIQUE |
| `Age` | INT | NOT NULL, CHECK (Age BETWEEN 1 AND 120) |
| `Course` | NVARCHAR(100) | NOT NULL |
| `CreatedDate` | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() |

### 5.2 Users Table (JWT Auth)

| Column | Type | Constraints |
|---|---|---|
| `Id` | INT | PRIMARY KEY, IDENTITY(1,1) |
| `Username` | NVARCHAR(100) | NOT NULL, UNIQUE |
| `PasswordHash` | NVARCHAR(500) | NOT NULL |
| `Role` | NVARCHAR(50) | NOT NULL, DEFAULT 'Admin' |
| `CreatedDate` | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() |

### 5.3 SQL Creation Script

```sql
CREATE TABLE Students (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(100) NOT NULL,
    Email       NVARCHAR(150) NOT NULL UNIQUE,
    Age         INT           NOT NULL CHECK (Age > 0 AND Age < 120),
    Course      NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Users (
    Id           INT           PRIMARY KEY IDENTITY(1,1),
    Username     NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role         NVARCHAR(50)  NOT NULL DEFAULT 'Admin',
    CreatedDate  DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
```

---

## 6. Authentication & Security

### 6.1 JWT Flow

```
Client                            API
  |                                |
  |-- POST /api/auth/login ------> |
  |   { username, password }       |
  |                                |-- Validate credentials
  |                                |-- Generate JWT (HS256)
  |<-- 200 OK { token, expiry } -- |
  |                                |
  |-- GET /api/students ---------->|  Authorization: Bearer <token>
  |                                |-- Validate JWT signature + expiry
  |<-- 200 OK [ students ] ------- |
```

### 6.2 JWT Config (`appsettings.json`)

```json
"JwtSettings": {
  "SecretKey": "your-256-bit-super-secret-key-here",
  "Issuer":    "SMS.API",
  "Audience":  "SMS.Client",
  "ExpiryInMinutes": 60
}
```

### 6.3 Security Rules

| Rule | Detail |
|---|---|
| All `/api/students/*` | Require `[Authorize]` attribute |
| `/api/auth/login`, `/api/auth/register` | Public endpoints |
| Passwords | BCrypt hashed — never stored as plaintext |
| Token expiry | 60 minutes (configurable) |
| HTTPS | Enforced via `UseHttpsRedirection()` |
| Secrets | In `appsettings.json` or environment variables, never hardcoded |

---

## 7. API Specification

### 7.1 Standard Response Envelope

All responses use a consistent wrapper:

```json
{
  "success": true,
  "message": "...",
  "data": { },
  "errors": { },
  "count": 0
}
```

---

### 7.2 Auth Endpoints

#### `POST /api/auth/register`
**Body:**
```json
{ "username": "admin", "password": "Admin@123" }
```
**201 Created:**
```json
{ "success": true, "message": "User registered successfully." }
```
**409 Conflict:**
```json
{ "success": false, "message": "Username already exists." }
```

---

#### `POST /api/auth/login`
**Body:**
```json
{ "username": "admin", "password": "Admin@123" }
```
**200 OK:**
```json
{ "success": true, "data": { "token": "eyJ...", "expiresAt": "2026-05-10T14:00:00Z" } }
```
**401 Unauthorized:**
```json
{ "success": false, "message": "Invalid username or password." }
```

---

### 7.3 Student Endpoints *(All require `Authorization: Bearer <token>`)*

#### `GET /api/students` — Get All Students
**200 OK:**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Rahul Sharma", "email": "rahul@example.com",
      "age": 21, "course": "B.Tech CSE", "createdDate": "2026-05-01T10:30:00Z" }
  ],
  "count": 1
}
```

---

#### `GET /api/students/{id}` — Get Student by ID
**200 OK:** *(student object)*
**404 Not Found:**
```json
{ "success": false, "message": "Student with ID 99 not found." }
```

---

#### `POST /api/students` — Add New Student
**Body:**
```json
{ "name": "Rahul Sharma", "email": "rahul@example.com", "age": 21, "course": "B.Tech CSE" }
```
**201 Created:** *(created student object)*
**400 Bad Request:**
```json
{ "success": false, "errors": { "Email": ["Email is already registered."] } }
```

---

#### `PUT /api/students/{id}` — Update Student
**Body:** *(same fields as POST)*
**200 OK:** *(updated student object)*
**404 Not Found / 400 Bad Request** as above

---

#### `DELETE /api/students/{id}` — Delete Student
**200 OK:**
```json
{ "success": true, "message": "Student deleted successfully." }
```
**404 Not Found:** *(student not found error)*

---

## 8. Functional Requirements

| ID | Feature | Priority |
|---|---|---|
| FR-01 | Get all students | Must Have |
| FR-02 | Get student by ID | Must Have |
| FR-03 | Add new student | Must Have |
| FR-04 | Update existing student | Must Have |
| FR-05 | Delete student | Must Have |
| FR-06 | User registration | Must Have |
| FR-07 | User login, returns JWT | Must Have |
| FR-08 | Protect all student endpoints with JWT | Must Have |
| FR-09 | Return 404 for missing students | Must Have |
| FR-10 | Return 400 for validation failures | Must Have |

### Validation Rules

| Field | Rules |
|---|---|
| `Name` | Required, 2–100 characters |
| `Email` | Required, valid format, must be unique |
| `Age` | Required, integer between 1 and 120 |
| `Course` | Required, 2–100 characters |
| `Password` | Min 8 chars, must include uppercase, digit, and special char |

---

## 9. Non-Functional Requirements

| ID | Requirement | Detail |
|---|---|---|
| NFR-01 | Layered Architecture | Strict separation: Controller → Service → Repository |
| NFR-02 | JWT Authentication | All student APIs secured, token validated per request |
| NFR-03 | Global Exception Handling | Middleware catches all unhandled exceptions |
| NFR-04 | Structured Logging | Serilog with Console + rolling File sinks |
| NFR-05 | Swagger Documentation | All endpoints documented, JWT auth configurable in UI |
| NFR-06 | Consistent Responses | `ApiResponse<T>` wrapper on all responses |
| NFR-07 | Input Validation | FluentValidation or Data Annotations |
| NFR-08 | Password Security | BCrypt hashing |
| NFR-09 | Dependency Injection | All services, repositories injected via constructor DI |
| NFR-10 | Clean Code | SOLID principles, meaningful naming, XML doc comments |

---

## 10. Global Exception Handling Middleware

### Behavior
```
HTTP Request
     ↓
[GlobalExceptionMiddleware]
     ↓
Controller → Service → Repository
     ↑  (throws exception)
[Middleware catches it]
     ↓
Logs full stack trace (Serilog Error)
     ↓
Returns structured JSON error response
```

### HTTP Status Mapping

| Exception | HTTP Status |
|---|---|
| `NotFoundException` | 404 Not Found |
| `ValidationException` | 400 Bad Request |
| `ConflictException` (duplicate email) | 409 Conflict |
| `UnauthorizedException` | 401 Unauthorized |
| Unhandled `Exception` | 500 Internal Server Error |

### Error Response Format (500)

```json
{
  "success": false,
  "statusCode": 500,
  "message": "An unexpected error occurred. Please try again later.",
  "traceId": "0HN4K2L9AB3C:00000001"
}
```

---

## 11. Logging (Serilog)

### Sinks
- **Console** — colored, structured output during development
- **File** — rolling daily logs at `logs/sms-YYYYMMDD.log`

### Log Events

| Event | Level |
|---|---|
| Application startup / shutdown | Information |
| Incoming API request | Information |
| Student created / updated / deleted | Information |
| Validation failure | Warning |
| Student not found | Warning |
| Duplicate email attempt | Warning |
| Unhandled exception | Error |

### `appsettings.json` Config

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": { "Microsoft": "Warning", "System": "Warning" }
  },
  "WriteTo": [
    { "Name": "Console" },
    { "Name": "File", "Args": { "path": "logs/sms-.log", "rollingInterval": "Day" } }
  ]
}
```

---

## 12. Swagger / API Documentation

| Requirement | Detail |
|---|---|
| All endpoints listed | HTTP method, URL, description, parameters |
| Request body schemas | With field types and validation notes |
| Response schemas | Per status code (200, 201, 400, 401, 404, 500) |
| JWT Auth | "Authorize" button in Swagger UI accepts Bearer token |
| Access URL | `http://localhost:5000/swagger` (development only) |

---

## 13. BONUS — Unit Testing (xUnit + Moq)

### Scope

| Layer | Test Cases |
|---|---|
| `StudentService` | GetAll, GetById (found/not-found), Create (valid/duplicate), Update, Delete |
| `AuthService` | Login success, Login with bad credentials |
| `StudentsController` | Returns correct HTTP status codes per service outcome |

### Coverage Goal
≥ **80%** on Service and Controller layers

### Sample Test

```csharp
[Fact]
public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenStudentNotFound()
{
    // Arrange
    _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Student?)null);

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => _studentService.GetByIdAsync(99));
}
```

---

## 14. BONUS — Docker

### `Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish SMS.API/SMS.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SMS.API.dll"]
```

### `docker-compose.yml`

```yaml
version: "3.9"
services:
  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=SMS_DB;User=sa;Password=YourStrong@Pass123;TrustServerCertificate=True
      - JwtSettings__SecretKey=your-256-bit-secret-key
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      SA_PASSWORD: "YourStrong@Pass123"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

**Run:**
```bash
docker-compose up --build
```

---

## 15. BONUS — Frontend UI

### Screens

| Screen | Route | Description |
|---|---|---|
| Login | `/login` | Username + password, stores JWT in localStorage |
| Students List | `/students` | Table: Name, Email, Age, Course + Edit/Delete actions |
| Add Student | `/students/add` | Form with validation |
| Edit Student | `/students/edit/:id` | Pre-filled form |
| Delete | (Modal) | Confirm dialog before delete |

### Angular Implementation

```typescript
// app-routing.module.ts
const routes: Routes = [
  { path: 'login',            component: LoginComponent },
  { path: 'students',         component: StudentsListComponent, canActivate: [AuthGuard] },
  { path: 'students/add',     component: StudentFormComponent,  canActivate: [AuthGuard] },
  { path: 'students/edit/:id',component: StudentFormComponent,  canActivate: [AuthGuard] },
  { path: '', redirectTo: 'students', pathMatch: 'full' }
];
```

- `HttpInterceptor` to attach `Authorization: Bearer <token>` to every request
- Reactive Forms with Validators for all fields
- `MatSnackBar` for success/error toast notifications

### React Implementation

```jsx
// App.jsx (React Router v6)
<Routes>
  <Route path="/login" element={<Login />} />
  <Route path="/students" element={<PrivateRoute><StudentsList /></PrivateRoute>} />
  <Route path="/students/add" element={<PrivateRoute><StudentForm /></PrivateRoute>} />
  <Route path="/students/edit/:id" element={<PrivateRoute><StudentForm /></PrivateRoute>} />
</Routes>
```

- Axios instance with `interceptors.request` to attach JWT header
- React Hook Form + Yup for validation
- `react-toastify` for notifications

---

## 16. GitHub Submission Requirements

### Repository Contents
```
student-management-system/
├── src/                # .NET solution
├── frontend/           # [Optional] Angular/React app
├── docker-compose.yml  # [Optional]
├── .gitignore
└── README.md
```

### README.md Must Cover
1. Project overview & features
2. Prerequisites (SDK, SQL Server version)
3. Clone & restore steps
4. `appsettings.json` configuration
5. EF Core migration commands
6. How to run the API
7. Swagger URL and sample credentials
8. Docker steps (if applicable)
9. Test run commands (if applicable)

### `.gitignore` Must Exclude
`bin/`, `obj/`, `.vs/`, `*.user`, `appsettings.Development.json` (if it has secrets)

---

## 17. Evaluation Criteria

| Criterion | Weight | What Evaluators Look For |
|---|---|---|
| **Code Quality** | High | Naming, SOLID principles, no dead code, clean structure |
| **Architecture** | High | Proper 3-layer separation, DI, repository pattern |
| **Error Handling** | High | Global middleware, custom exceptions, consistent responses |
| **Security** | High | JWT on all endpoints, BCrypt passwords, no hardcoded secrets |
| **API Functionality** | High | All CRUD + auth endpoints work correctly |
| **Unit Tests** | Bonus | Meaningful assertions, mocked dependencies |
| **Docker** | Bonus | API + DB spin up with `docker-compose up` |
| **Frontend UI** | Bonus | Login, list, add, edit, delete all functional |

---

## 18. Definition of Done

A feature is complete when ALL of the following are true:

- [ ] API returns correct HTTP status codes for all scenarios
- [ ] JWT protection applied and verified
- [ ] Input validation returns descriptive error messages
- [ ] All exceptions handled by global middleware (no unhandled 500s)
- [ ] Serilog logs the event at the correct level
- [ ] Swagger documents the endpoint with request/response schema
- [ ] Code follows layered architecture (no DB calls in controllers)
- [ ] Code pushed to GitHub with a meaningful commit message

---

## 19. Suggested Timeline

| Day | Milestone |
|---|---|
| Day 1 | Solution structure, EF Core entities, DB migrations |
| Day 2 | JWT Auth endpoints (register + login), Serilog, global middleware |
| Day 3 | Student CRUD endpoints, validation, Swagger |
| Day 4 | Unit tests + Docker setup |
| Day 5 | Frontend UI, README polish, final GitHub push |

---

## 20. Key NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | EF Core SQL Server driver |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT middleware |
| `Serilog.AspNetCore` | Serilog ASP.NET Core integration |
| `Serilog.Sinks.File` | File logging sink |
| `Swashbuckle.AspNetCore` | Swagger UI |
| `BCrypt.Net-Next` | Password hashing |
| `FluentValidation.AspNetCore` | Input validation |
| `xunit` | Unit test framework |
| `Moq` | Mocking library |
| `Microsoft.EntityFrameworkCore.InMemory` | In-memory DB for tests |
