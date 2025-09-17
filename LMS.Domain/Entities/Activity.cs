using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

// <copyright file="Activity.cs" company="Team Dragon Lexicon">
//     Copyright (c) Team Dragon Lexicon. All rights reserved.
// </copyright>
// <summary>
//     Represents an activity entity in the LMS.
// </summary>
namespace LMS.Domain.Entities;
public class Activity
{
            public int Id
            {
                get; set;
            }
            [Required]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;
            [Required]
            [StringLength(50)]
            public string Type { get; set; } = string.Empty;
            [StringLength(500)]
            public string Description { get; set; } = string.Empty;
            public DateTime StartTime
            {
                get; set;
            }
            public DateTime EndTime
            {
                get; set;
            }
            public int ModuleId
            {
                get; set;
            }
            public Module Module { get; set; } = null!;
            public ICollection<Document> Documents { get; set; } = new List<Document>();
}
