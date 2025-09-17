using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.Domain.Entities;

public class Document
{
    // <copyright file="Document.cs" company="Team Dragon Lexicon">
    //     Copyright (c) Team Dragon Lexicon. All rights reserved.
    // </copyright>
    // <summary>
    //     Represents a document entity in the LMS.
    // </summary>
    public int Id
    {
        get; set;
    }
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp
    {
        get; set;
    }
    public string UploadedByUserId { get; set; } = string.Empty;
    public ApplicationUser UploadedByUser { get; set; } = null!;
    public string LinkedEntityType { get; set; } = string.Empty; // Course | Module | Activity
    public int LinkedEntityId
    {
        get; set;
    }
}
