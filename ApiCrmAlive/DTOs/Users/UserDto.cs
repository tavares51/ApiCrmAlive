namespace ApiCrmAlive.DTOs.Users;

public record UserDto(
    Guid Id, string Name, string Email, string Role, string? Phone,
    bool IsActive, bool ReceiveNotifications,
    DateTime CreatedAt, DateTime UpdatedAt, Guid UpdatedBy
);
