using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OdevAPI.Entities;
using OdevAPI.Data;
using OdevAPI.DTOs;
using OdevAPI.Enums;
using OdevAPI.Interfaces;
using OdevAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();
builder.Services.AddScoped<ILoanService, LoanService>();
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

app.UseHttpsRedirection();

app.MapControllers();

// ========== USERS ==========
app.MapGet("/users", async (AppDbContext db) => await db.Users.ToListAsync());

app.MapGet("/users/{id}", async (int id, AppDbContext db) => await db.Users.FindAsync(id));

app.MapPost("/users", async (User user, AppDbContext db) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
});

app.MapPut("/users/{id}", async (int id, User inputUser, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound("User not found");

    user.Name = inputUser.Name;
    user.LastName = inputUser.LastName;
    user.Email = inputUser.Email;
    user.Phone = inputUser.Phone;
    user.Address = inputUser.Address;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/users/{id}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound("User not found");

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ========== CATEGORIES ==========
app.MapGet("/categories", async (AppDbContext db) => await db.Categories.ToListAsync());

app.MapGet("/categories/{id}", async (int id, AppDbContext db) => await db.Categories.FindAsync(id));

app.MapPost("/categories", async (Category category, AppDbContext db) =>
{
    db.Categories.Add(category);
    await db.SaveChangesAsync();
    return Results.Created($"/categories/{category.Id}", category);
});

app.MapPut("/categories/{id}", async (int id, Category inputCategory, AppDbContext db) =>
{
    var category = await db.Categories.FindAsync(id);
    if (category is null) return Results.NotFound("Category not found");

    category.Name = inputCategory.Name;
    category.Description = inputCategory.Description;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/categories/{id}", async (int id, AppDbContext db) =>
{
    var category = await db.Categories.FindAsync(id);
    if (category is null) return Results.NotFound("Category not found");

    db.Categories.Remove(category);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Nested resource: Get books by category
app.MapGet("/categories/{id}/books", async (int id, AppDbContext db) => 
    await db.Books.Where(b => b.CategoryId == id).ToListAsync());

// ========== BOOKS ==========
app.MapGet("/books", async (AppDbContext db) => await db.Books.ToListAsync());

app.MapGet("/books/{id}", async (int id, AppDbContext db) => await db.Books.FindAsync(id));

app.MapPost("/books", async (Book book, AppDbContext db) =>
{
    var category = await db.Categories.FindAsync(book.CategoryId);
    if (category is null) return Results.NotFound("Category not found");

    db.Books.Add(book);
    await db.SaveChangesAsync();
    return Results.Created($"/books/{book.Id}", book);
});

app.MapPut("/books/{id}", async (int id, Book inputBook, AppDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound("Book not found");

    book.Title = inputBook.Title;
    book.Author = inputBook.Author;
    book.ISBN = inputBook.ISBN;
    book.Description = inputBook.Description;
    book.TotalCopies = inputBook.TotalCopies;
    book.AvailableCopies = inputBook.AvailableCopies;
    book.CategoryId = inputBook.CategoryId;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/books/{id}", async (int id, AppDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound("Book not found");

    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ========== LOANS ==========
app.MapGet("/loans", async (AppDbContext db) => await db.Loans.ToListAsync());

app.MapGet("/loans/{id}", async (int id, AppDbContext db) => await db.Loans.FindAsync(id));

// Nested resource: Get loans by user
app.MapGet("/users/{id}/loans", async (int id, AppDbContext db) => 
    await db.Loans.Where(l => l.UserId == id).ToListAsync());

app.MapPost("/loans", async (Loan loan, AppDbContext db) =>
{
    var book = await db.Books.FindAsync(loan.BookId);
    if (book is null) return Results.NotFound("Book not found");
    if (book.AvailableCopies < 1) return Results.Conflict("No copies available");

    book.AvailableCopies -= 1;
    db.Books.Update(book);

    loan.LoanDate = DateTimeOffset.UtcNow;
    loan.DueDate = DateTimeOffset.UtcNow.AddDays(14);
    loan.Status = LoanStatus.Active;

    db.Loans.Add(loan);
    await db.SaveChangesAsync();
    return Results.Created($"/loans/{loan.Id}", loan);
});

app.MapPut("/loans/{id}", async (int id, LoanUpdateDto inputLoan, AppDbContext db) =>
{
    var loan = await db.Loans.FindAsync(id);
    if (loan is null) return Results.NotFound("Loan not found");

    loan.Notes = inputLoan.Notes;
    loan.Status = inputLoan.Status;
    loan.DueDate = inputLoan.DueDate;
    loan.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/loans/{id}", async (int id, AppDbContext db) =>
{
    var loan = await db.Loans.FindAsync(id);
    if (loan is null) return Results.NotFound("Loan not found");

    db.Loans.Remove(loan);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Return a book
app.MapPatch("/loans/{id}/return", async (int id, AppDbContext db) =>
{
    var loan = await db.Loans.FindAsync(id);
    if (loan is null) return Results.NotFound("Loan not found");
    if (loan.Status == LoanStatus.Returned) return Results.Conflict("Book already returned");

    var book = await db.Books.FindAsync(loan.BookId);
    if (book is not null)
    {
        book.AvailableCopies += 1;
        db.Books.Update(book);
    }

    loan.Status = LoanStatus.Returned;
    loan.ReturnDate = DateTimeOffset.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(loan);
});

app.Run();
