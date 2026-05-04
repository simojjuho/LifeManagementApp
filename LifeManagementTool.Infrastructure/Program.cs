var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCustomSwagger();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/", () =>
{
    var response = new
    {
        Message = "Hello! Welcome to LifeManagementTool! Endpoint documentation at /swagger",
        Version = "0.1",
        TimeOnly = DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss"),
    };
    return Results.Ok(response);
}).WithName("WelcomeMessage");

app.Run();  