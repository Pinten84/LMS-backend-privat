using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Domain.Entities;

// <copyright file="RefreshToken.cs" company="Team Dragon Lexicon">
//     Copyright (c) Team Dragon Lexicon. All rights reserved.
// </copyright>
// <summary>
//     Represents a refresh token entity for secure authentication.
// </summary>
public class RefreshToken
{
    public int Id
    {
        get; set;
    }
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    [MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;
    public DateTime Expires
    {
        get; set;
    }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress
    {
        get; set;
    }
    public string? UserAgent
    {
        get; set;
    }
    public DateTime? RevokedAt
    {
        get; set;
    }
    public bool IsCompromised
    {
        get; set;
    }
    public int? ReplacedByTokenId
    {
        get; set;
    }
    [ForeignKey(nameof(ReplacedByTokenId))]
    public RefreshToken? ReplacedByToken
    {
        get; set;
    }
    public ApplicationUser? User
    {
        get; set;
    }
}
