using NewsletterOrchestratedSagaMessaging.Bus;
using RabbitMQ.Client;

namespace NewsletterOrchestratedSagaRpc.ExternalServices;

public interface IEmailService
{
    Task SendWelcomeEmail(string email);
    Task SendFollowUpEmail(string email);
}

public class EmailService : IEmailService
{
    private readonly IModel _channel;
    private readonly PoormansBus _bus;
    
    public EmailService(PoormansBus bus)
    {
        _bus = bus;
        _channel = bus.Subscribe(
            queue: "EmailService",
            handler: async msg =>
            {
                if (msg is SendWelcomeEmail sendWelcomeEmail)
                    await SendWelcomeEmail(sendWelcomeEmail.Email);
                else if (msg is SendFollowUpEmail sendFollowUpEmail)
                    await SendFollowUpEmail(sendFollowUpEmail.Email);
            },
            deleteExistingQueue: true
        );
    }

    public Task SendWelcomeEmail(string email)
    {
        _bus.Publish(new WelcomeEmailSent(email));
        return Task.CompletedTask;
    }

    public Task SendFollowUpEmail(string email)
    {
        _bus.Publish(new FollowUpEmailSent(email));
        return Task.CompletedTask;
    }
}