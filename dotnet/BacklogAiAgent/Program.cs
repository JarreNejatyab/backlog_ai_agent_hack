using System;
using BacklogAiAgent.Config;
using BacklogAiAgent.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Configure services
builder.Services.AddSingleton<BacklogAiAgent.Config.ConfigurationManager>();
builder.Services.AddSingleton<AIService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAllOrigins");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Add a fallback route to serve index.html for SPA navigation
app.MapFallbackToFile("index.html");

// Add a specific route for the root URL to serve index.html
app.MapGet("/", () => Results.File("index.html", "text/html"));

app.Run();
