using Cleipnir.ResilientFunctions.AspNetCore.Core;
using Cleipnir.ResilientFunctions.AspNetCore.SqlServer;
using Cleipnir.ResilientFunctions.SqlServer;
using NewsletterOrchestratedSagaMessaging.Bus;
using NewsletterOrchestratedSagaMessaging.Saga;
using NewsletterOrchestratedSagaRpc.ExternalServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var bus = new PoormansBus();
/*bus.Subscribe(
    queue: "test",
    handler: o =>
    {
        Console.WriteLine(JsonSerializer.Serialize(o));
        return Task.CompletedTask;
    },
    deleteExistingQueue: true
);

bus.Publish("hello world");

Console.ReadLine();*/

builder.Services.AddSingleton(bus);
var emailService = new EmailService(bus);
builder.Services.AddSingleton<IEmailService>(emailService);
builder.Services.AddSingleton<Onboarder>();

const string connectionString = "Server=localhost,1434;Database=NewsletterMessaging;User Id=sa;Password=Strong_password_123!;Encrypt=True;TrustServerCertificate=True;";
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
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();