using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LifeManagementTool.Core.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    
}