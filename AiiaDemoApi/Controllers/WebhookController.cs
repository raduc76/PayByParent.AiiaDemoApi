using AiiaDemoApi.Commands;
using AiiaDemoApi.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AiiaDemoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IMediator mediator;

        public WebhookController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost(Name = "webhook")]
        public async Task<IActionResult> Process()
        {
            var payloadString = await HttpHelper.ReadRequestBody(Request.Body);
            var aiiaSignature = Request.Headers["X-Aiia-Signature"];
            var timestamp = Request.Headers["X-Aiia-TimeStamp"];
            var eventId = Request.Headers["X-Aiia-EventId"];
            var eventType = Request.Headers["X-Aiia-Event"];

            var processWebhookCommand = new ProcessWebhookCommand
            {
                Timestamp = timestamp,
                EventId = eventId,
                EventType = eventType,
                AiiaSignature = aiiaSignature,
                PayloadString = payloadString
            };
            var result = await mediator.Send(processWebhookCommand);

            return new OkObjectResult(result);
        }
    }
}
