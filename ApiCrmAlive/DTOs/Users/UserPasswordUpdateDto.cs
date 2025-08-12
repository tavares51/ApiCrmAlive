namespace ApiCrmAlive.DTOs.Users;

public record UserPasswordUpdateDto(
    string CurrentPassword,
    string NewPassword
);
