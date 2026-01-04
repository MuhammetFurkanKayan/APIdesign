# Library Management API

.NET 9 REST API ile geliştirilmiş Kütüphane Yönetim Sistemi.

## 🏗️ Mimari

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐    │
│  │  Controllers │  │  Minimal API │  │  Swagger/OpenAPI │    │
│  └──────────────┘  └──────────────┘  └──────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│                      Business Layer                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐    │
│  │   Services   │  │  Interfaces  │  │       DTOs       │    │
│  └──────────────┘  └──────────────┘  └──────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│                      Data Layer                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐    │
│  │  AppDbContext│  │   Entities   │  │   Migrations     │    │
│  └──────────────┘  └──────────────┘  └──────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│                      Infrastructure                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐    │
│  │   SQLite DB  │  │   Serilog    │  │  Email Service   │    │
│  └──────────────┘  └──────────────┘  └──────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## 📊 Entity İlişkileri

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
                                              │ - Phone      │
                                              │ - Address    │
                                              │ - CreatedAt  │
                                              │ - UpdatedAt  │
                                              │ - IsDeleted  │
                                              └──────────────┘
```

## 🚀 Teknolojiler

- **.NET 9** - Framework
- **Entity Framework Core** - ORM
- **SQLite** - Database
- **Serilog** - Logging (JSON Format)
- **Swagger/OpenAPI** - API Dokümantasyonu

## 📋 Endpoint Listesi

### Users (Minimal API)

| Method | Endpoint      | Açıklama                  |
| ------ | ------------- | ------------------------- |
| GET    | `/users`      | Tüm kullanıcıları listele |
| GET    | `/users/{id}` | Kullanıcı detayı          |
| POST   | `/users`      | Yeni kullanıcı oluştur    |
| PUT    | `/users/{id}` | Kullanıcı güncelle        |
| DELETE | `/users/{id}` | Kullanıcı sil             |

### Books (Minimal API)

| Method | Endpoint      | Açıklama              |
| ------ | ------------- | --------------------- |
| GET    | `/books`      | Tüm kitapları listele |
| GET    | `/books/{id}` | Kitap detayı          |
| POST   | `/books`      | Yeni kitap oluştur    |
| PUT    | `/books/{id}` | Kitap güncelle        |
| DELETE | `/books/{id}` | Kitap sil             |

### Categories (Minimal API)

| Method | Endpoint                 | Açıklama                 |
| ------ | ------------------------ | ------------------------ |
| GET    | `/categories`            | Tüm kategorileri listele |
| GET    | `/categories/{id}`       | Kategori detayı          |
| GET    | `/categories/{id}/books` | Kategoriye ait kitaplar  |
| POST   | `/categories`            | Yeni kategori oluştur    |
| PUT    | `/categories/{id}`       | Kategori güncelle        |
| DELETE | `/categories/{id}`       | Kategori sil             |

### Loans (Controller - Layered Architecture)

| Method | Endpoint             | Açıklama                      |
| ------ | -------------------- | ----------------------------- |
| GET    | `/api/v1/loans`      | Tüm ödünç işlemlerini listele |
| GET    | `/api/v1/loans/{id}` | Ödünç detayı                  |
| POST   | `/api/v1/loans`      | Yeni ödünç oluştur            |
| PUT    | `/api/v1/loans/{id}` | Ödünç güncelle                |
| PATCH  | `/api/v1/loans/{id}` | Ödünç kısmi güncelle          |
| DELETE | `/api/v1/loans/{id}` | Ödünç sil                     |
| PATCH  | `/loans/{id}/return` | Kitap iade et                 |

### Nested Resources

| Method | Endpoint                 | Açıklama                     |
| ------ | ------------------------ | ---------------------------- |
| GET    | `/users/{id}/loans`      | Kullanıcının ödünç işlemleri |
| GET    | `/categories/{id}/books` | Kategorideki kitaplar        |

## 📝 API Response Formatı

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

### Hata Response

```json
{
  "success": false,
  "message": "Loan not found",
  "data": null
}
```

## 🔧 Kurulum

### Gereksinimler

- .NET 9 SDK
- Git

### Adımlar

```bash
# Repository'yi klonla
git clone https://github.com/MuhammetFurkanKayan/APIdesign.git
cd APIdesign

# Bağımlılıkları yükle
cd OdevAPI
dotnet restore

# Database migration'larını uygula
dotnet run migrate

# Uygulamayı çalıştır
dotnet run

# Swagger UI
# https://localhost:7002/swagger
```

### Docker ile Çalıştırma

```bash
# Image oluştur
docker build -t library-mgmt .

# Container çalıştır
docker run -p 8080:8080 library-mgmt
```

## 📁 Proje Yapısı

```
OdevAPI/
├── Common/
│   ├── ApiResponse.cs          # Standart API response
│   └── Logging/
│       └── CustomJsonFormatter.cs  # JSON log formatter
├── Controllers/
│   ├── LoanController.cs       # Loan CRUD (Layered)
│   └── AuditLogController.cs   # Audit log controller
├── Data/
│   ├── AppDbContext.cs         # EF Core DbContext
│   └── AppDbContextFactory.cs  # Design-time factory
├── DTOs/
│   ├── LoanCreateDto.cs
│   ├── LoanUpdateDto.cs
│   ├── LoanPatchDto.cs
│   ├── ResponseDtos.cs         # Response DTOs
│   └── EmailSettings.cs
├── Entities/
│   ├── User.cs
│   ├── Book.cs
│   ├── Category.cs
│   ├── Loan.cs
│   └── AuditLog.cs
├── Enums/
│   └── LoanStatus.cs
├── Interfaces/
│   ├── ILoanService.cs
│   ├── IAuditLogService.cs
│   └── IEmailService.cs
├── Middleware/
│   └── GlobalExceptionMiddleware.cs  # Global exception handler
├── Migrations/
│   └── InitialCreate.cs
├── Services/
│   ├── LoanService.cs
│   ├── AuditLogService.cs
│   └── EmailService.cs
├── Templates/
│   ├── LoanConfirmationTemplate.html
│   └── LoanReturnTemplate.html
├── Program.cs                  # Application entry point
├── appsettings.json           # Configuration
└── Dockerfile                 # Docker configuration
```

## ✅ Özellikler

- [x] .NET 9 REST API
- [x] Minimal API + Layered Architecture
- [x] 5 Entity (User, Book, Category, Loan, AuditLog)
- [x] Entity ilişkileri (1-N)
- [x] CreatedAt/UpdatedAt alanları
- [x] DTO kullanımı (Create, Update, Response)
- [x] Standart API Response formatı
- [x] Global Exception Handling
- [x] Doğru HTTP Status Codes
- [x] RESTful URL yapısı
- [x] Swagger/OpenAPI
- [x] Serilog JSON Logging
- [x] EF Core Migrations
- [x] Docker desteği
- [x] CI/CD Pipeline (GitHub Actions)
- [x] Email servisi

### Bonus

- [x] Soft Delete (IsDeleted alanı)

## 👤 Geliştirici

- **Ad:** Muhammet Furkan Kayan
- **GitHub:** [@MuhammetFurkanKayan](https://github.com/MuhammetFurkanKayan)
