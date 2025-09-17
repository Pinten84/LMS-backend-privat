using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace LMS.Domain.Entities
{
	// <copyright file="ApplicationUser.cs" company="Team Dragon Lexicon">
	//     Copyright (c) Team Dragon Lexicon. All rights reserved.
	// </copyright>
	// <summary>
	//     Represents an application user with refresh tokens.
	// </summary>
	public class ApplicationUser : IdentityUser
	{
		public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
	}
}
