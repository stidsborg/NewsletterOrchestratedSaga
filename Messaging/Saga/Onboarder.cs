using Cleipnir.ResilientFunctions;
using Cleipnir.ResilientFunctions.AspNetCore.Core;
using Cleipnir.ResilientFunctions.CoreRuntime.Invocation;
using Cleipnir.ResilientFunctions.Reactive;
using NewsletterOrchestratedSagaMessaging.Bus;

namespace NewsletterOrchestratedSagaMessaging.Saga;

public class Onboarder : IRegisterRFuncOnInstantiation
{
    private readonly RAction<string> _registration;
    private readonly PoormansBus _bus;

    public Onboarder(RFunctions rFunctions, PoormansBus bus)
    {
        _bus = bus;
        _registration = rFunctions.RegisterAction<string>(
            functionTypeId: "NewsletterOrchestrator",
            ExecuteSagaWorkflow
        );
        
        bus.Subscribe(
            queue: "NewsletterOrchestrator",
            handler: async msg =>
            {
                if (msg is WelcomeEmailSent welcomeEmailSent)
                    await _registration.EventSourceWriters.For(welcomeEmailSent.Email).AppendEvent(welcomeEmailSent);
                else if (msg is FollowUpEmailSent followUpEmailSent)
                    await _registration.EventSourceWriters.For(followUpEmailSent.Email).AppendEvent(followUpEmailSent);
            },
            deleteExistingQueue: true
        );
    }

    public async Task SignUpEmail(string email)
    {
        await _registration.Invoke( //alternative use Schedule if you do not want to wait for the saga to complete before returning
            functionInstanceId: email,
            param: email
        );
    }

    private async Task ExecuteSagaWorkflow(string email, Context context)
    {
        var eventSource = await context.EventSource; //each saga has an associated event source (useful for messaging and timeout events)
        
        //send welcome email
        _bus.Publish(new SendWelcomeEmail(email)); 
        await eventSource.NextOfType<WelcomeEmailSent>();
        
        //wait 5 seconds
        await Task.Delay(5_000); //simple solution
        //alternative with suspension: await eventSource.SuspendFor(resumeAt: TimeSpan.FromSeconds(5), timeoutId: "wait_5_seconds");
        
        //send followup email
        _bus.Publish(new SendFollowUpEmail(email));
        await eventSource.NextOfType<FollowUpEmailSent>();
    }
}