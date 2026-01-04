using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OdevAPI.Entities;

namespace OdevAPI.Data;

public static class DbSeeder
{
    public static void SeedData(AppDbContext context)
    {
        if (context.Users.Any() || context.Categories.Any() || context.Books.Any())
            return;

        // Seed Categories
        var categories = new List<Category>
        {
            new() { Name = "Roman", Description = "Dünya ve Türk edebiyatından romanlar", CreatedAt = DateTime.UtcNow },
            new() { Name = "Bilim Kurgu", Description = "Bilim kurgu ve fantastik kitaplar", CreatedAt = DateTime.UtcNow },
            new() { Name = "Tarih", Description = "Tarih ve biyografi kitapları", CreatedAt = DateTime.UtcNow },
            new() { Name = "Teknoloji", Description = "Yazılım ve teknoloji kitapları", CreatedAt = DateTime.UtcNow }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        // Seed Users with authentication
        var users = new List<User>
        {
            new() { 
                Name = "Admin", 
                LastName = "User", 
                Email = "admin@library.com", 
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Role = "Admin",
                Phone = "555-0001", 
                Address = "Admin Street 1", 
                CreatedAt = DateTime.UtcNow 
            },
            new() { 
                Name = "Ahmet", 
                LastName = "Yılmaz", 
                Email = "ahmet@example.com", 
                Username = "ahmet",
                PasswordHash = HashPassword("user123"),
                Role = "User",
                Phone = "555-0002", 
                Address = "Kadıköy, İstanbul", 
                CreatedAt = DateTime.UtcNow 
            },
            new() { 
                Name = "Ayşe", 
                LastName = "Demir", 
                Email = "ayse@example.com",
                Username = "ayse",
                PasswordHash = HashPassword("user123"),
                Role = "User",
                Phone = "555-0003", 
                Address = "Çankaya, Ankara", 
                CreatedAt = DateTime.UtcNow 
            },
            new() { 
                Name = "Mehmet", 
                LastName = "Kaya", 
                Email = "mehmet@example.com",
                Username = "mehmet",
                PasswordHash = HashPassword("user123"),
                Role = "User",
                Phone = "555-0004", 
                Address = "Konak, İzmir", 
                CreatedAt = DateTime.UtcNow 
            }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        // Seed Books
        var books = new List<Book>
        {
            new() { Title = "Suç ve Ceza", Author = "Fyodor Dostoyevski", ISBN = "978-0-14-044913-6", Description = "Rus edebiyatının başyapıtı", TotalCopies = 5, AvailableCopies = 5, CategoryId = categories[0].Id, CreatedAt = DateTime.UtcNow },
            new() { Title = "1984", Author = "George Orwell", ISBN = "978-0-452-28423-4", Description = "Distopik klasik", TotalCopies = 3, AvailableCopies = 3, CategoryId = categories[1].Id, CreatedAt = DateTime.UtcNow },
            new() { Title = "Sapiens", Author = "Yuval Noah Harari", ISBN = "978-0-06-231609-7", Description = "İnsan türünün kısa tarihi", TotalCopies = 4, AvailableCopies = 4, CategoryId = categories[2].Id, CreatedAt = DateTime.UtcNow },
            new() { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "978-0-13-235088-4", Description = "Temiz kod yazma sanatı", TotalCopies = 2, AvailableCopies = 2, CategoryId = categories[3].Id, CreatedAt = DateTime.UtcNow },
            new() { Title = "Sefiller", Author = "Victor Hugo", ISBN = "978-0-14-044430-8", Description = "Fransız edebiyatının şaheseri", TotalCopies = 3, AvailableCopies = 3, CategoryId = categories[0].Id, CreatedAt = DateTime.UtcNow },
            new() { Title = "Dune", Author = "Frank Herbert", ISBN = "978-0-441-17271-9", Description = "Bilim kurgu destanı", TotalCopies = 4, AvailableCopies = 4, CategoryId = categories[1].Id, CreatedAt = DateTime.UtcNow }
        };
        context.Books.AddRange(books);
        context.SaveChanges();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = Guid.NewGuid().ToString();
        var saltedPassword = password + salt;
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(hash) + ":" + salt;
    }
}
