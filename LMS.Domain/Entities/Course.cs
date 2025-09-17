// <copyright file="Course.cs" company="Team Dragon Lexicon">
//     Copyright (c) Team Dragon Lexicon. All rights reserved.
// </copyright>
// <summary>
//     Represents a course entity in the LMS.
// </summary>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS.Domain.Entities;

public class Course
{
    public int Id
    {
        get; set;
    }
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate
    {
        get; set;
    }
    public DateTime? EndDate
    {
        get; set;
    }
    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<ApplicationUser> Students { get; set; } = new List<ApplicationUser>();
    public string? TeacherId
    {
        get; set;
    }
    public ApplicationUser? Teacher
    {
        get; set;
    }
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
