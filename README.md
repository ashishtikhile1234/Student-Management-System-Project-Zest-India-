# 🎓 Student Management System

A production-ready **ASP.NET Core 8 Web API** for managing student records.  
Built as a full-stack technical assignment for **Zest India IT Pvt Ltd**.

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019-red)](https://www.microsoft.com/sql-server)
[![JWT](https://img.shields.io/badge/Auth-JWT%20Bearer-orange)](https://jwt.io/)
[![Swagger](https://img.shields.io/badge/Docs-Swagger%20UI-green)](https://swagger.io/)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue)](https://www.docker.com/)

---

## ✨ Features

| Feature | Detail |
|---|---|
| **CRUD Operations** | Create, Read (all + by ID), Update, Delete students |
| **JWT Authentication** | Register → Login → Bearer token on all student endpoints |
| **Layered Architecture** | Controller → Service → Repository (strict separation) |
| **Global Exception Handling** | Middleware catches all exceptions, returns structured JSON |
| **Serilog Logging** | Console + rolling daily file logs (`logs/`) |
| **Swagger UI** | Full API documentation with JWT auth support |
| **Input Validation** | Data Annotations on all DTOs |
| **Unit Tests** | 17 tests with xUnit + Moq (Service + Controller layers) |
| **Docker** | Multi-stage Dockerfile + docker-compose (API + SQL Server) |

---

## 🏗️ Architecture.

```
┌─────────────────────────────────────────┐
│           SMS.API (Web API)             │
│    Controllers + Middleware + Program   │
├─────────────────────────────────────────┤
│        SMS.Application (Business)      │
│   Services + Interfaces + DTOs         │
├─────────────────────────────────────────┤
│        SMS.Infrastructure (Data)       │
│   EF Core + Repositories + DbContext   │
├─────────────────────────────────────────┤
│          SMS.Domain (Entities)         │
│   Student, User, Custom Exceptions     │
└─────────────────────────────────────────┘
```

---

## 📁 Project Structure

```
StudentManagementSystem/
├── src/
│   ├── SMS.API/
│   │   ├── Controllers/         # AuthController, StudentsController
│   │   ├── Middleware/          # GlobalExceptionMiddleware
│   │   ├── Program.cs           # DI, JWT, Serilog, Swagger setup
│   │   └── appsettings.json
│   ├── SMS.Application/
│   │   ├── DTOs/                # StudentDtos, AuthDtos, ApiResponse<T>
│   │   ├── Interfaces/          # IStudentRepository, IStudentService, IAuthService
│   │   └── Services/            # StudentService, AuthService
│   ├── SMS.Domain/
│   │   ├── Entities/            # Student.cs, User.cs
│   │   └── Exceptions/          # NotFoundException, ConflictException, ValidationException
│   ├── SMS.Infrastructure/
│   │   ├── Data/                # AppDbContext (EF Core)
│   │   └── Repositories/        # StudentRepository, UserRepository
│   └── SMS.Tests/
│       ├── Services/            # StudentServiceTests (10 tests)
│       └── Controllers/         # StudentsControllerTests (7 tests)
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
├── .gitignore
└── README.md
```

---

## ⚙️ Prerequisites

| Tool | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ |
| [SQL Server](https://www.microsoft.com/sql-server) | 2019+ (or use Docker) |
| [Docker](https://www.docker.com/get-started) | (Optional) |
| [Git](https://git-scm.com/) | Any |

---

## 🚀 Getting Started (Local Setup)

### 1. Clone the Repository
```bash
git clone https://github.com/ashishtikhile1234/Student-Management-System-Project-Zest-India-.git
cd "Student Management System Project (Zest India)"
```

### 2. Configure the Database

Open `src/SMS.API/appsettings.json` and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> For SQL Server with username/password:
> ```
> Server=localhost;Database=StudentManagementDB;User=sa;Password=YourPass;TrustServerCertificate=True;
> ```

### 3. Configure JWT Settings

In `appsettings.json`, update the JWT secret (use a strong 256-bit key in production):

```json
"JwtSettings": {
  "SecretKey": "Your-Strong-256-bit-Secret-Key-Here!",
  "Issuer": "SMS.API",
  "Audience": "SMS.Client",
  "ExpiryInMinutes": 60
}
```

### 4. Apply Database Migrations

```bash
cd src/SMS.API
dotnet ef database update --project ../SMS.Infrastructure
```

> **Note:** Migrations auto-apply on startup too (via `db.Database.Migrate()` in `Program.cs`).

### 5. Run the API

```bash
dotnet run --project src/SMS.API
```

The API starts at: `http://localhost:5000`  
Swagger UI: **`http://localhost:5000/swagger`**

---

## 🐳 Docker Setup (Recommended)

Runs both the API and SQL Server — no local SQL Server install required.

```bash
# Build and start all services
docker-compose up --build

# Stop all services
docker-compose down

# Remove volumes (clears DB data)
docker-compose down -v
```

| Service | URL |
|---|---|
| API | `http://localhost:5000` |
| Swagger UI | `http://localhost:5000/swagger` |
| SQL Server | `localhost:1433` (SA password: `YourStrong@Pass123`) |

---

## 🔐 Authentication

All student endpoints are **JWT protected**. Follow these steps:

### Step 1: Register a User
```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

### Step 2: Login to Get Token
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "expiresAt": "2026-05-10T15:00:00Z",
    "username": "admin",
    "role": "Admin"
  }
}
```

### Step 3: Use the Token
Add to every student request:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

In **Swagger UI**: Click **Authorize** → Enter `Bearer <your-token>`.

---

## 📡 API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | ❌ Public | Register a new user |
| `POST` | `/api/auth/login` | ❌ Public | Login & get JWT token |
| `GET` | `/api/students` | ✅ JWT | Get all students |
| `GET` | `/api/students/{id}` | ✅ JWT | Get student by ID |
| `POST` | `/api/students` | ✅ JWT | Add new student |
| `PUT` | `/api/students/{id}` | ✅ JWT | Update student |
| `DELETE` | `/api/students/{id}` | ✅ JWT | Delete student |

### Sample: Add Student
```http
POST /api/students
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Rahul Sharma",
  "email": "rahul@example.com",
  "age": 21,
  "course": "B.Tech Computer Science"
}
```

**Response 201:**
```json
{
  "success": true,
  "message": "Student created successfully.",
  "data": {
    "id": 1,
    "name": "Rahul Sharma",
    "email": "rahul@example.com",
    "age": 21,
    "course": "B.Tech Computer Science",
    "createdDate": "2026-05-10T10:30:00Z"
  }
}
```

---

## 🧪 Running Unit Tests

```bash
dotnet test src/SMS.Tests
```

**Test Coverage:**

| Test Class | Tests |
|---|---|
| `StudentServiceTests` | 10 tests — GetAll, GetById, Create, Update, Delete |
| `StudentsControllerTests` | 7 tests — HTTP status code verification |
| **Total** | **17 tests** |

```bash
# Run with verbose output
dotnet test src/SMS.Tests --verbosity normal

# Run with code coverage
dotnet test src/SMS.Tests --collect:"XPlat Code Coverage"
```

---

## 📋 Validation Rules

| Field | Rules |
|---|---|
| `Name` | Required, 2–100 characters |
| `Email` | Required, valid format, unique per student |
| `Age` | Required, integer 1–120 |
| `Course` | Required, 2–100 characters |
| `Password` | Min 8 chars, 1 uppercase, 1 digit, 1 special char |

---

## 📝 Error Response Format

All errors follow this structure:
```json
{
  "success": false,
  "statusCode": 404,
  "message": "Student with ID 99 was not found.",
  "traceId": "0HN4K2L9AB3C:00000001"
}
```

| Status | Meaning |
|---|---|
| `400` | Validation failed |
| `401` | Invalid/missing JWT |
| `404` | Student not found |
| `409` | Duplicate email |
| `500` | Unexpected server error |

---

## 🔧 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| Language | C# 12 |
| Database | SQL Server 2019+ |
| ORM | Entity Framework Core 8 |
| Auth | JWT Bearer (HMAC-SHA256) |
| Password Hashing | BCrypt.Net |
| Logging | Serilog (Console + File) |
| API Documentation | Swagger / Swashbuckle |
| Validation | Data Annotations |
| Testing | xUnit + Moq + FluentAssertions |
| Containerization | Docker + Docker Compose |

---

## 📜 License

This project is built as a technical assignment for **Zest India IT Pvt Ltd**.

---

*Built with ❤️ by Ashish Tikhile*
