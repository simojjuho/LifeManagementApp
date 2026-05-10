using System.Text;
using LifeManagementTool.Controller.DTOs;
using LifeManagementTool.Core.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace LifeManagementTool.Controller.Controllers;

public static class CustomIdentityEndpoints
{
    public static IEndpointRouteBuilder MapCustomIdentityEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/auth")
            .WithTags("CustomIdentity");

        group.MapPost("/register-admin", RegisterUser)
            .WithName("RegisterAdmin")
            .WithSummary("Register a User")
            .WithDescription("Registers a user must have admin role");
        //.RequireAuthorization("AdminPolicy");

        return route;
    }

    private static async Task<IResult> RegisterUser(
        RegisterUserRequest userDto,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailSender emailSender,
        IConfiguration config)
    {
        if (await userManager.FindByEmailAsync(userDto.Email) is not null)
        {
            return Results.BadRequest($"User with email {userDto.Email} already exists");
        }

        var user = new ApplicationUser
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
        };
        
        var tempPassword = "TempPassword!24";
        
        var created = await userManager.CreateAsync(user, tempPassword);

        if (!created.Succeeded)
        {
            return Results.BadRequest(new { Error = created.Errors });
        }

        if (await roleManager.RoleExistsAsync("Researcher"))
        {
            await userManager.AddToRoleAsync(user, "Researcher");
        }
        
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        
        var baseUrl = config["BaseURL"] ?? "localhost:7166";
        
        await emailSender.SendEmailAsync(
            userDto.Email,
            "Account created",
            $"""
             Your account has been created. Please change your password by visiting {baseUrl}/Identity/Account/Setpassword.html
             
             {baseUrl}/Setpassword.html?email={userDto.Email}&resetCode={encodedToken}")
             
             """);
        
        
        return Results.Ok((new { Message = $"Successfully registered user with email {userDto.Email}" }));

    }
}