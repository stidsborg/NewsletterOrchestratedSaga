using Cleipnir.ResilientFunctions.AspNetCore.Core;
using Cleipnir.ResilientFunctions.AspNetCore.SqlServer;
using Cleipnir.ResilientFunctions.SqlServer;
using NewsletterOrchestratedSagaRpc.ExternalServices;
using NewsletterOrchestratedSagaRpc.Saga;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<Onboarder>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
const string connectionString = "Server=localhost,1434;Database=NewsletterRpc;User Id=sa;Password=Strong_password_123!;Encrypt=True;TrustServerCertificate=True;";
await DatabaseHelper.CreateDatabaseIfNotExists(connectionString);
builder.Services.UseResilientFunctions(
    connectionString,
    options: _ => new Options(
        unhandledExceptionHandler: Console.WriteLine,
        crashedCheckFrequency: TimeSpan.FromSeconds(5)
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NewsletterOrchestratedSaga");
        options.RoutePrefix = "";
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
