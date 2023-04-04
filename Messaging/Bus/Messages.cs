namespace NewsletterOrchestratedSagaMessaging.Bus;

public record FollowUpEmailSent(string Email);
public record SendFollowUpEmail(string Email);
public record SendWelcomeEmail(string Email);
public record SubscribeToNewsletter(string Email);
public record WelcomeEmailSent(string Email);