using Microsoft.AspNetCore.Mvc;
using NewsletterOrchestratedSagaMessaging.Saga;

namespace NewsletterOrchestratedSagaMessaging.Controllers;

[ApiController]
[Route("[controller]")]
public class NewslettersController : ControllerBase
{
    private readonly Onboarder _onboarder;

    public NewslettersController(Onboarder onboarder) => _onboarder = onboarder;

    [HttpPost]
    public async Task<ActionResult> Post(string email)
    {
        await _onboarder.SignUpEmail(email);
        return Ok(email);
    }
}
