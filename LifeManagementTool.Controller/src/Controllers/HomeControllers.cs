using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

using LifeManagementTool.Controller.Models.Response;

namespace LifeManagementTool.Controller.Controllers;

public static class HomeControllers
{
    public static IEndpointRouteBuilder MapHomeEndpoints(this IEndpointRouteBuilder route)
    {
        var homeGroup = route.MapGroup("/api/Home")
            .WithTags("Home");
        
        // /api/home/welcome
        homeGroup.MapGet("/welcome", GetWelcomeMessage)
            .WithName("GetWelcomeMessage")
            .WithSummary("Welcome to LifeManagementTool")
            .WithDescription("Displays: Welcome to LifeManagementTool");
        
        return route;
    }
    
    // Handlers

    private static async Task<Ok<WelcomeResponse>> GetWelcomeMessage(CancellationToken ct)
    {
        var welcomeMessage = new WelcomeResponse
        {
            Message = "Welcome to LifeManagementTool!",
            Version = "0.0.1",
            TimeOnly = DateTime.Now.ToString("T"),
        };

        return TypedResults.Ok(welcomeMessage);
    }
}