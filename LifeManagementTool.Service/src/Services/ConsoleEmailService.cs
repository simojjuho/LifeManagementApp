using Microsoft.AspNetCore.Identity.UI.Services;

namespace LifeManagementTool.Service.Services;

public class ConsoleEmailService : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"To: {email}\n\nSubject: {subject}\n\n{htmlMessage}");
        return Task.CompletedTask;
    }
}