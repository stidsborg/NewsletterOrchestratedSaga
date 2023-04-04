namespace NewsletterOrchestratedSagaRpc.ExternalServices;

public interface IEmailService
{
    Task SendWelcomeEmail(string email);
    Task SendFollowUpEmail(string email);
}

public class EmailService : IEmailService
{
    public Task SendWelcomeEmail(string email)
        => Task.CompletedTask;

    public Task SendFollowUpEmail(string email)
        => Task.CompletedTask;
}