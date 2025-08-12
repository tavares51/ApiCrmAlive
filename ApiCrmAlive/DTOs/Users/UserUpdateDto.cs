namespace ApiCrmAlive.DTOs.Users;

public record UserUpdateDto(
    string? Name,
    string? Role,
    string? Phone,
    bool? ReceiveNotifications
);
