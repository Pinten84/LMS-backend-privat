namespace LMS.Application.Contracts.DTOs.AuthDtos;

public record RefreshRequestDto(string AccessToken, string RefreshToken, string? IpAddress = null, string? UserAgent = null);
