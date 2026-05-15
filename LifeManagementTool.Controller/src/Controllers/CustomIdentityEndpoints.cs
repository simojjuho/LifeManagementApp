using System.Security.Claims;
using System.Text;
using LifeManagementTool.Controller.DTOs;
using LifeManagementTool.Controller.Models.Request;
using LifeManagementTool.Controller.Models.Response;
using LifeManagementTool.Core.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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
            .WithDescription("Registers a user must have admin role")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);;
        //.RequireAuthorization("AdminPolicy");

        group.MapPost("/reset-password", ResetPassword)
            .WithName("ResetPassword")
            .WithSummary("Resets a password")
            .WithDescription("Resets a password")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        
        group.MapPost("/forgot-password", ForgotPassword)
            .WithName("ForgotPassword")
            .WithSummary("Reset your forgotten password")
            .WithDescription("Reset your password")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);;

        group.MapGet("/manage/profile", GetProfileInfo)
            .WithName("Get profile")
            .WithDescription("Get current user profile")
            .WithSummary("Get current user profile")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/manage/profile", UpdateProfile)
            .WithName("Update profile")
            .WithSummary("Update current user profile")
            .WithDescription("Update current user profile")
            .RequireAuthorization()
            .Produces((StatusCodes.Status200OK))
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        return route;
    }
    
    private static async Task<IResult> GetProfileInfo(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager
    )
    {
        var user = await userManager.GetUserAsync(principal);

        if (user is null)
        {
            return Results.NotFound(new { Error = "User not found" });
        }

        var response = new UserProfileResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            FullName = user.FullName
        };
        
        return Results.Ok(response);
    }

    private static async Task<IResult> RegisterUser(
        RegisterUserRequest request,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailSender emailSender,
        IConfiguration config)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        { 
            return Results.BadRequest($"User with email {request.Email} already exists");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
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
            request.Email,
            "Account created",
            $"""
             Your account has been created. Please change your password by visiting {baseUrl}/Identity/Account/Setpassword.html
             
             {baseUrl}/Setpassword.html?email={request.Email}&resetCode={encodedToken}")
             
             """);
        
        
        return Results.Ok((new { Message = $"Successfully registered user with email {request.Email}" }));

    }

    private static async Task<IResult> ResetPassword(
        ResetPasswordRequest request,
        UserManager<ApplicationUser> userManager)
    {
        if(
            string.IsNullOrEmpty(request.Email) ||
            string.IsNullOrEmpty(request.ResetCode) ||
            string.IsNullOrEmpty(request.NewPassword))
        {
            return Results.BadRequest(new { Error = "All fields must be filled" });
        }
        
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Results.BadRequest(new { Error = "User not found" });
        }

        try
        {
            var decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(request.ResetCode));
            var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (result.Succeeded)
            {
                return Results.Ok(new { Message = "Successfully reset password" });
            }
            
            return Results.BadRequest(new { Message = "Make sure you used big and small letters, numbers and special characters!" });
        }
        catch (FormatException)
        {
            return Results.BadRequest("Invalid reset code");
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    private static async Task<IResult> ForgotPassword(
        ForgotPasswordRequest  request,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IConfiguration config)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return Results.BadRequest(new { Error = "Email is required" });
        }
        
        var user = await userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            return Results.BadRequest(new { Error = "If the user exists a message will be sent in the email address." });
        }
        
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        
        var baseUrl = config["BaseURL"] ?? "localhost:7166";
        var resetLink = $"{baseUrl}/reset-password.html?email={request.Email}&resetCode={encodedToken}";
        
        await emailSender.SendEmailAsync(
            request.Email,
            "Password reset",
            $"""
             Here is a link to reset your password. Please change your password by visiting {baseUrl}/Identity/Account/Setpassword.html

             If you did not request for a new password please ignore this post. This email was sent without having a correct password.
             
             {resetLink}")
             """);
        
        
        return Results.Ok(new { Message = "Check your email for a password reset link." });
    }

    private static async Task<IResult> UpdateProfile(
        UpdateProfileRequest request,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager)
    {
        if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
        {
            return Results.BadRequest(new { Error = "First name and last name must be provided" });
        }
        
        var user = await userManager.GetUserAsync(principal);

        if (user is null)
        {
            return Results.BadRequest( new { Error = "User not found" } );
        }
        
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Results.InternalServerError($"Could not update profile: {result.Errors.First()}");
        }
        
        return Results.Ok(new { Message = "Successfully updated profile" });
    }
}