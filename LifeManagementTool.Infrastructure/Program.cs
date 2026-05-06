using LifeManagementTool.Controller.Controllers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCustomSwagger();

//get a connection to the database
var connectionString = DataUtility.GetConnectionString(builder.Configuration);

//configure the database context for PostgreSql
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHomeEndpoints();

app.Run();  