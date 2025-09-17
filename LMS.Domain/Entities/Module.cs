using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace LMS.Domain.Entities;
public class Module
{
            public int Id
            {
                // <copyright file="Module.cs" company="Team Dragon Lexicon">
                //     Copyright (c) Team Dragon Lexicon. All rights reserved.
                // </copyright>
                // <summary>
                //     Represents a module entity in the LMS.
                // </summary>
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
            public DateTime EndDate
            {
                get; set;
            }
            public int CourseId
            {
                get; set;
            }
            public Course Course { get; set; } = null!;
            public ICollection<Activity> Activities { get; set; } = new List<Activity>();
            public ICollection<Document> Documents { get; set; } = new List<Document>();
}
