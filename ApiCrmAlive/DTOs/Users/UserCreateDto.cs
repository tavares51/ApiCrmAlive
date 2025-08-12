namespace ApiCrmAlive.DTOs.Users;

public record UserCreateDto(
    string Name,
    string Email,
    string Password,
    string? Role,
    string? Phone,
    bool? ReceiveNotifications
);
