using Cleipnir.ResilientFunctions;
using Cleipnir.ResilientFunctions.AspNetCore.Core;
using Cleipnir.ResilientFunctions.CoreRuntime.Invocation;
using Cleipnir.ResilientFunctions.Reactive;
using NewsletterOrchestratedSagaRpc.ExternalServices;

namespace NewsletterOrchestratedSagaRpc.Saga;

public class Onboarder : IRegisterRFuncOnInstantiation
{
    private readonly RAction<string> _registration;
    private readonly IEmailService _emailService;

    public Onboarder(RFunctions rFunctions, IEmailService emailService)
    {
        _emailService = emailService;
        _registration = rFunctions.RegisterAction<string>(
            nameof(Onboarder),
            ExecuteSagaWorkflow
        );
    }

    public async Task SignUpEmail(string email)
    {
        await _registration.Schedule(
            functionInstanceId: email,
            param: email
        );
    }

    private async Task ExecuteSagaWorkflow(string email, Context context)
    {
        var eventSource = await context.EventSource; //each saga has an associated event source (useful for messaging and timeout events)
        
        //send welcome email
        await _emailService.SendWelcomeEmail(email);
        
        //wait 5 seconds
        await eventSource.SuspendFor(resumeAt: TimeSpan.FromSeconds(5), timeoutId: "suspension");
        //alternative: await Task.Delay(5_000);
        
        //send followup email
        await _emailService.SendFollowUpEmail(email);
    }
}