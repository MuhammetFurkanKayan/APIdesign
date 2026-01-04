# Library Management API

.NET 9 REST API ile geliştirilmiş Kütüphane Yönetim Sistemi.

## Mimari

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │  Controllers │  │  Minimal API │  │  Swagger/OpenAPI │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│                      Business Layer                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │   Services   │  │  Interfaces  │  │       DTOs       │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│                      Data Layer                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │  AppDbContext│  │   Entities   │  │   Migrations     │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│                      Infrastructure                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │   SQLite DB  │  │   Serilog    │  │  Email Service   │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Entity İlişkileri

```
┌──────────────┐       ┌──────────────┐       ┌──────────────┐
│   Category   │ 1   * │     Book     │ 1   * │     Loan     │
├──────────────┤◄──────├──────────────┤◄──────├──────────────┤
│ - Id         │       │ - Id         │       │ - Id         │
│ - Name       │       │ - Title      │       │ - Notes      │
│ - Description│       │ - Author     │       │ - Status     │
│ - CreatedAt  │       │ - ISBN       │       │ - LoanDate   │
│ - UpdatedAt  │       │ - CategoryId │       │ - DueDate    │
│ - IsDeleted  │       │ - CreatedAt  │       │ - ReturnDate │
└──────────────┘       │ - UpdatedAt  │       │ - UserId     │
                       │ - IsDeleted  │       │ - BookId     │
                       └──────────────┘       │ - CreatedAt  │
                                              │ - UpdatedAt  │
                                              │ - IsDeleted  │
                                              └──────────────┘
                                                     │
                                                     │ *
                                                     ▼ 1
                                              ┌──────────────┐
                                              │     User     │
                                              ├──────────────┤
                                              │ - Id         │
                                              │ - Name       │
                                              │ - LastName   │
                                              │ - Email      │
                                              │ - Username   │
                                              │ - Role       │
                                              │ - Phone      │
                                              │ - Address    │
                                              │ - CreatedAt  │
                                              │ - UpdatedAt  │
                                              │ - IsDeleted  │
                                              └──────────────┘
```

## Teknolojiler

- .NET 9
- Entity Framework Core
- SQLite
- Serilog (JSON Format)
- JWT Bearer Authentication
- Swagger/OpenAPI

## Authentication

### Auth Endpoints

| Method | Endpoint                | Açıklama             |
| ------ | ----------------------- | -------------------- |
| POST   | `/api/v1/auth/login`    | Kullanıcı girişi     |
| POST   | `/api/v1/auth/register` | Yeni kullanıcı kaydı |

### Login Request

```json
{
  "username": "admin",
  "password": "admin123"
}
```

### Login Response

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "username": "admin",
    "role": "Admin",
    "expiresAt": "2024-01-01T11:00:00Z"
  }
}
```

### Varsayılan Kullanıcılar

| Username | Password | Role  |
| -------- | -------- | ----- |
| admin    | admin123 | Admin |
| ahmet    | user123  | User  |
| ayse     | user123  | User  |
| mehmet   | user123  | User  |

## Endpoint Listesi

### Users (Minimal API)

| Method | Endpoint      | Açıklama                    |
| ------ | ------------- | --------------------------- |
| GET    | `/users`      | Tüm kullanıcıları listele   |
| GET    | `/users/{id}` | Kullanıcı detayı            |
| POST   | `/users`      | Yeni kullanıcı oluştur      |
| PUT    | `/users/{id}` | Kullanıcı güncelle          |
| DELETE | `/users/{id}` | Kullanıcı sil (soft delete) |

### Books (Minimal API)

| Method | Endpoint      | Açıklama                |
| ------ | ------------- | ----------------------- |
| GET    | `/books`      | Tüm kitapları listele   |
| GET    | `/books/{id}` | Kitap detayı            |
| POST   | `/books`      | Yeni kitap oluştur      |
| PUT    | `/books/{id}` | Kitap güncelle          |
| DELETE | `/books/{id}` | Kitap sil (soft delete) |

### Categories (Minimal API)

| Method | Endpoint           | Açıklama                   |
| ------ | ------------------ | -------------------------- |
| GET    | `/categories`      | Tüm kategorileri listele   |
| GET    | `/categories/{id}` | Kategori detayı            |
| POST   | `/categories`      | Yeni kategori oluştur      |
| PUT    | `/categories/{id}` | Kategori güncelle          |
| DELETE | `/categories/{id}` | Kategori sil (soft delete) |

### Loans (Layered Architecture - Controller)

| Method | Endpoint                    | Açıklama             | Yetki         |
| ------ | --------------------------- | -------------------- | ------------- |
| GET    | `/api/v1/loans`             | Tüm ödünç işlemleri  | Authenticated |
| GET    | `/api/v1/loans/{id}`        | Ödünç detayı         | Authenticated |
| POST   | `/api/v1/loans`             | Yeni ödünç oluştur   | Authenticated |
| PUT    | `/api/v1/loans/{id}`        | Ödünç güncelle       | Authenticated |
| PATCH  | `/api/v1/loans/{id}`        | Ödünç kısmi güncelle | Authenticated |
| DELETE | `/api/v1/loans/{id}`        | Ödünç sil            | Admin         |
| PATCH  | `/api/v1/loans/{id}/return` | Kitap iade           | Authenticated |

### Nested Resources

| Method | Endpoint                 | Açıklama                     |
| ------ | ------------------------ | ---------------------------- |
| GET    | `/users/{id}/loans`      | Kullanıcının ödünç işlemleri |
| GET    | `/categories/{id}/books` | Kategorideki kitaplar        |

## API Response Formatı

Başarılı response:

```json
{
  "success": true,
  "message": "Loans listed",
  "data": [
    {
      "id": 1,
      "notes": "First loan",
      "status": "Active",
      "loanDate": "2024-01-01T10:00:00Z",
      "dueDate": "2024-01-15T10:00:00Z",
      "userId": 1,
      "bookId": 1
    }
  ]
}
```

Hata response:

```json
{
  "success": false,
  "message": "Loan not found",
  "data": null
}
```

## Kurulum

### Gereksinimler

- .NET 9 SDK
- Git

### Yerel Kurulum

```bash
# Repository'yi klonla
git clone https://github.com/MuhammetFurkanKayan/APIdesign.git
cd APIdesign

# Bağımlılıkları yükle
dotnet restore

# Uygulamayı çalıştır
cd OdevAPI
dotnet run
```

Uygulama varsayılan olarak `http://localhost:5000` adresinde çalışır.

### Docker ile Kurulum

```bash
# Image oluştur
docker build -t library-api .

# Container çalıştır
docker run -p 5000:8080 -v library-data:/app/data library-api
```

## Proje Yapısı

```
OdevAPI/
├── Controllers/
│   ├── LoanController.cs
│   ├── AuditLogController.cs
│   └── AuthController.cs
├── Services/
│   ├── LoanService.cs
│   ├── AuditLogService.cs
│   ├── AuthService.cs
│   └── EmailService.cs
├── Interfaces/
│   ├── ILoanService.cs
│   ├── IAuditLogService.cs
│   ├── IAuthService.cs
│   └── IEmailService.cs
├── Data/
│   ├── AppDbContext.cs
│   ├── AppDbContextFactory.cs
│   └── DbSeeder.cs
├── Entities/
│   ├── User.cs
│   ├── Book.cs
│   ├── Category.cs
│   ├── Loan.cs
│   └── AuditLog.cs
├── DTOs/
│   ├── LoanDtos.cs
│   ├── ResponseDtos.cs
│   ├── AuthDtos.cs
│   └── EntityToDtoMapper.cs
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Migrations/
├── Program.cs
├── appsettings.json
└── Dockerfile
```

## Özellikler

- .NET 9 REST API
- Minimal API + Layered Architecture
- 5 Entity (User, Book, Category, Loan, AuditLog)
- Entity ilişkileri (1-N)
- CreatedAt/UpdatedAt alanları
- DTO kullanımı (Create, Update, Response)
- Standart API Response formatı
- Global Exception Handling
- HTTP Status Codes (200, 201, 400, 401, 404, 409, 500)
- RESTful URL yapısı
- Swagger/OpenAPI
- Serilog JSON Logging
- EF Core Migrations
- Docker desteği
- CI/CD Pipeline (GitHub Actions)
- Email servisi
- JWT Authentication
- Role-based Access Control (Admin/User)
- Soft Delete
- Seed Data

## Geliştirici

Muhammet Furkan Kayan
