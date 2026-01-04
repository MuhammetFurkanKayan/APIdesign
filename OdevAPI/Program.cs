using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OdevAPI.Entities;
using OdevAPI.Data;
using OdevAPI.DTOs;
using OdevAPI.Enums;
using OdevAPI.Interfaces;
using OdevAPI.Services;
using Serilog;
using Serilog.Context;
using OdevAPI.Middleware;
using OdevAPI.Common;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "LibraryMgmt")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .CreateLogger();

try
{
    Log.Information("Starting Library Management application...");

    var builder = WebApplication.CreateBuilder(args);
    
    // Serilog'u Host'a ekle
    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "LibraryMgmt")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.AddOpenApi();
    builder.Services.AddScoped<ILoanService, LoanService>();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings?.Issuer,
                ValidAudience = jwtSettings?.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? ""))
            };
        });
    builder.Services.AddAuthorization();
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (args.Contains("migrate"))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        return;
    }

    // Seed initial data
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        DbSeeder.SeedData(db);
    }

    // Global Exception Handler
    app.UseGlobalExceptionHandler();

    // Request logging middleware
    app.Use(async (context, next) =>
    {
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("TraceIdentifier", context.TraceIdentifier))
        {
            await next();
        }
    });

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnostics, context) =>
        {
            diagnostics.Set("TraceIdentifier", context.TraceIdentifier);
            diagnostics.Set("RequestHost", context.Request.Host.Value);
            diagnostics.Set("RequestProtocol", context.Request.Protocol);
            diagnostics.Set("ClientIp", context.Connection.RemoteIpAddress?.ToString());
            diagnostics.Set("UserAgent", context.Request.Headers["User-Agent"].ToString());
        };
    });

    app.MapControllers();

    // ========== USERS ==========
    app.MapGet("/users", async (AppDbContext db) => 
    {
        var users = await db.Users.Where(u => !u.IsDeleted).ToListAsync();
        return Results.Ok(new ApiResponse<List<UserResponseDto>>
        {
            Success = true,
            Message = "Users listed",
            Data = users.ToDto()
        });
    });

    app.MapGet("/users/{id}", async (int id, AppDbContext db) =>
    {
        var user = await db.Users.FindAsync(id);
        if (user is null || user.IsDeleted)
            return Results.NotFound(new ApiResponse<UserResponseDto> { Success = false, Message = "User not found", Data = null });
        return Results.Ok(new ApiResponse<UserResponseDto>
        {
            Success = true,
            Message = "User found",
            Data = user.ToDto()
        });
    });

    app.MapPost("/users", async (User user, AppDbContext db) =>
    {
        user.CreatedAt = DateTime.UtcNow;
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Results.Created($"/users/{user.Id}", new ApiResponse<UserResponseDto>
        {
            Success = true,
            Message = "User created",
            Data = user.ToDto()
        });
    });

    app.MapPut("/users/{id}", async (int id, User inputUser, AppDbContext db) =>
    {
        var user = await db.Users.FindAsync(id);
        if (user is null || user.IsDeleted)
            return Results.NotFound(new ApiResponse<UserResponseDto> { Success = false, Message = "User not found", Data = null });

        user.Name = inputUser.Name;
        user.LastName = inputUser.LastName;
        user.Email = inputUser.Email;
        user.Phone = inputUser.Phone;
        user.Address = inputUser.Address;
        user.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(new ApiResponse<UserResponseDto>
        {
            Success = true,
            Message = "User updated",
            Data = user.ToDto()
        });
    });

    app.MapDelete("/users/{id}", async (int id, AppDbContext db) =>
    {
        var user = await db.Users.FindAsync(id);
        if (user is null || user.IsDeleted)
            return Results.NotFound(new ApiResponse<bool> { Success = false, Message = "User not found", Data = false });

        // Soft delete
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "User deleted",
            Data = true
        });
    });

    // ========== CATEGORIES ==========
    app.MapGet("/categories", async (AppDbContext db) =>
    {
        var categories = await db.Categories.Where(c => !c.IsDeleted).ToListAsync();
        return Results.Ok(new ApiResponse<List<CategoryResponseDto>>
        {
            Success = true,
            Message = "Categories listed",
            Data = categories.ToDto()
        });
    });

    app.MapGet("/categories/{id}", async (int id, AppDbContext db) =>
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null || category.IsDeleted)
            return Results.NotFound(new ApiResponse<CategoryResponseDto> { Success = false, Message = "Category not found", Data = null });
        return Results.Ok(new ApiResponse<CategoryResponseDto>
        {
            Success = true,
            Message = "Category found",
            Data = category.ToDto()
        });
    });

    app.MapPost("/categories", async (Category category, AppDbContext db) =>
    {
        category.CreatedAt = DateTime.UtcNow;
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return Results.Created($"/categories/{category.Id}", new ApiResponse<CategoryResponseDto>
        {
            Success = true,
            Message = "Category created",
            Data = category.ToDto()
        });
    });

    app.MapPut("/categories/{id}", async (int id, Category inputCategory, AppDbContext db) =>
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null || category.IsDeleted)
            return Results.NotFound(new ApiResponse<CategoryResponseDto> { Success = false, Message = "Category not found", Data = null });

        category.Name = inputCategory.Name;
        category.Description = inputCategory.Description;
        category.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(new ApiResponse<CategoryResponseDto>
        {
            Success = true,
            Message = "Category updated",
            Data = category.ToDto()
        });
    });

    app.MapDelete("/categories/{id}", async (int id, AppDbContext db) =>
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null || category.IsDeleted)
            return Results.NotFound(new ApiResponse<bool> { Success = false, Message = "Category not found", Data = false });

        // Soft delete
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Category deleted",
            Data = true
        });
    });

    // Nested resource: Get books by category
    app.MapGet("/categories/{id}/books", async (int id, AppDbContext db) =>
    {
        var books = await db.Books.Where(b => b.CategoryId == id && !b.IsDeleted).ToListAsync();
        return Results.Ok(new ApiResponse<List<BookResponseDto>>
        {
            Success = true,
            Message = "Books in category listed",
            Data = books.ToDto()
        });
    });

    // ========== BOOKS ==========
    app.MapGet("/books", async (AppDbContext db) =>
    {
        var books = await db.Books.Where(b => !b.IsDeleted).ToListAsync();
        return Results.Ok(new ApiResponse<List<BookResponseDto>>
        {
            Success = true,
            Message = "Books listed",
            Data = books.ToDto()
        });
    });

    app.MapGet("/books/{id}", async (int id, AppDbContext db) =>
    {
        var book = await db.Books.FindAsync(id);
        if (book is null || book.IsDeleted)
            return Results.NotFound(new ApiResponse<BookResponseDto> { Success = false, Message = "Book not found", Data = null });
        return Results.Ok(new ApiResponse<BookResponseDto>
        {
            Success = true,
            Message = "Book found",
            Data = book.ToDto()
        });
    });

    app.MapPost("/books", async (Book book, AppDbContext db) =>
    {
        var category = await db.Categories.FindAsync(book.CategoryId);
        if (category is null || category.IsDeleted)
            return Results.NotFound(new ApiResponse<BookResponseDto> { Success = false, Message = "Category not found", Data = null });

        book.CreatedAt = DateTime.UtcNow;
        db.Books.Add(book);
        await db.SaveChangesAsync();
        return Results.Created($"/books/{book.Id}", new ApiResponse<BookResponseDto>
        {
            Success = true,
            Message = "Book created",
            Data = book.ToDto()
        });
    });

    app.MapPut("/books/{id}", async (int id, Book inputBook, AppDbContext db) =>
    {
        var book = await db.Books.FindAsync(id);
        if (book is null || book.IsDeleted)
            return Results.NotFound(new ApiResponse<BookResponseDto> { Success = false, Message = "Book not found", Data = null });

        book.Title = inputBook.Title;
        book.Author = inputBook.Author;
        book.ISBN = inputBook.ISBN;
        book.Description = inputBook.Description;
        book.TotalCopies = inputBook.TotalCopies;
        book.AvailableCopies = inputBook.AvailableCopies;
        book.CategoryId = inputBook.CategoryId;
        book.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(new ApiResponse<BookResponseDto>
        {
            Success = true,
            Message = "Book updated",
            Data = book.ToDto()
        });
    });

    app.MapDelete("/books/{id}", async (int id, AppDbContext db) =>
    {
        var book = await db.Books.FindAsync(id);
        if (book is null || book.IsDeleted)
            return Results.NotFound(new ApiResponse<bool> { Success = false, Message = "Book not found", Data = false });

        // Soft delete
        book.IsDeleted = true;
        book.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Book deleted",
            Data = true
        });
    });

    // ========== LOANS ==========
    app.MapGet("/loans", async (AppDbContext db) =>
    {
        var loans = await db.Loans.ToListAsync();
        return Results.Ok(new ApiResponse<List<LoanResponseDto>>
        {
            Success = true,
            Message = "Loans listed",
            Data = loans.ToDto()
        });
    });

    app.MapGet("/loans/{id}", async (int id, AppDbContext db) =>
    {
        var loan = await db.Loans.FindAsync(id);
        if (loan is null)
            return Results.NotFound(new ApiResponse<LoanResponseDto> { Success = false, Message = "Loan not found", Data = null });
        return Results.Ok(new ApiResponse<LoanResponseDto>
        {
            Success = true,
            Message = "Loan found",
            Data = loan.ToDto()
        });
    });

    // Nested resource: Get loans by user
    app.MapGet("/users/{id}/loans", async (int id, AppDbContext db) =>
    {
        var loans = await db.Loans.Where(l => l.UserId == id).ToListAsync();
        return Results.Ok(new ApiResponse<List<LoanResponseDto>>
        {
            Success = true,
            Message = "User loans listed",
            Data = loans.ToDto()
        });
    });

    app.MapPost("/loans", async (Loan loan, AppDbContext db) =>
    {
        var book = await db.Books.FindAsync(loan.BookId);
        if (book is null || book.IsDeleted)
            return Results.NotFound(new ApiResponse<LoanResponseDto> { Success = false, Message = "Book not found", Data = null });
        if (book.AvailableCopies < 1)
            return Results.Conflict(new ApiResponse<LoanResponseDto> { Success = false, Message = "No copies available", Data = null });

        book.AvailableCopies -= 1;
        db.Books.Update(book);

        loan.LoanDate = DateTimeOffset.UtcNow;
        loan.DueDate = DateTimeOffset.UtcNow.AddDays(14);
        loan.Status = LoanStatus.Active;
        loan.CreatedAt = DateTime.UtcNow;

        db.Loans.Add(loan);
        await db.SaveChangesAsync();
        return Results.Created($"/loans/{loan.Id}", new ApiResponse<LoanResponseDto>
        {
            Success = true,
            Message = "Loan created",
            Data = loan.ToDto()
        });
    });

    app.MapPut("/loans/{id}", async (int id, LoanUpdateDto inputLoan, AppDbContext db) =>
    {
        var loan = await db.Loans.FindAsync(id);
        if (loan is null)
            return Results.NotFound(new ApiResponse<LoanResponseDto> { Success = false, Message = "Loan not found", Data = null });

        loan.Notes = inputLoan.Notes;
        loan.Status = inputLoan.Status;
        loan.DueDate = inputLoan.DueDate;
        loan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(new ApiResponse<LoanResponseDto>
        {
            Success = true,
            Message = "Loan updated",
            Data = loan.ToDto()
        });
    });

    app.MapDelete("/loans/{id}", async (int id, AppDbContext db) =>
    {
        var loan = await db.Loans.FindAsync(id);
        if (loan is null)
            return Results.NotFound(new ApiResponse<bool> { Success = false, Message = "Loan not found", Data = false });

        db.Loans.Remove(loan);
        await db.SaveChangesAsync();
        return Results.Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Loan deleted",
            Data = true
        });
    });

    // Return a book
    app.MapPatch("/loans/{id}/return", async (int id, AppDbContext db) =>
    {
        var loan = await db.Loans.FindAsync(id);
        if (loan is null)
            return Results.NotFound(new ApiResponse<LoanResponseDto> { Success = false, Message = "Loan not found", Data = null });
        if (loan.Status == LoanStatus.Returned)
            return Results.Conflict(new ApiResponse<LoanResponseDto> { Success = false, Message = "Book already returned", Data = null });

        var book = await db.Books.FindAsync(loan.BookId);
        if (book is not null)
        {
            book.AvailableCopies += 1;
            db.Books.Update(book);
        }

        loan.Status = LoanStatus.Returned;
        loan.ReturnDate = DateTimeOffset.UtcNow;
        loan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(new ApiResponse<LoanResponseDto>
        {
            Success = true,
            Message = "Book returned",
            Data = loan.ToDto()
        });
    });

    // Add HTTP Request Logging middleware
    app.UseSerilogRequestLogging();
    Log.Information("Application configured successfully");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
