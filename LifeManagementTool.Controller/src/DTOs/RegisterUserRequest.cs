using System.ComponentModel.DataAnnotations;

namespace LifeManagementTool.Controller.DTOs;

public class RegisterUserRequest
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
}