using OdevAPI.Entities;

namespace OdevAPI.DTOs;

public static class EntityToDtoMapper
{
    public static UserResponseDto ToDto(this User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role,
            Phone = user.Phone,
            Address = user.Address,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public static List<UserResponseDto> ToDto(this List<User> users)
    {
        return users.Select(u => u.ToDto()).ToList();
    }

    public static BookResponseDto ToDto(this Book book)
    {
        return new BookResponseDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Description = book.Description,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            CategoryId = book.CategoryId,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt
        };
    }

    public static List<BookResponseDto> ToDto(this List<Book> books)
    {
        return books.Select(b => b.ToDto()).ToList();
    }

    public static CategoryResponseDto ToDto(this Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    public static List<CategoryResponseDto> ToDto(this List<Category> categories)
    {
        return categories.Select(c => c.ToDto()).ToList();
    }

    public static LoanResponseDto ToDto(this Loan loan)
    {
        return new LoanResponseDto
        {
            Id = loan.Id,
            Notes = loan.Notes,
            Status = loan.Status.ToString(),
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            UserId = loan.UserId,
            BookId = loan.BookId,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        };
    }

    public static List<LoanResponseDto> ToDto(this List<Loan> loans)
    {
        return loans.Select(l => l.ToDto()).ToList();
    }
}
