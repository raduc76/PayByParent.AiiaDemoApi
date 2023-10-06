using AiiaDemoApi.Commands;
using AiiaDemoApi.Helpers;
using AiiaDemoApi.Model;
using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace AiiaDemoApi.Handlers
{
    public class ProcessWebhookHandler : IRequestHandler<ProcessWebhookCommand, string>
    {
        const string PAYMENT_USER_NAME = "AA Curca";

        private readonly ILogger<ProcessWebhookHandler> logger;
        private readonly AiiaOptions aiiaOptions;

        public ProcessWebhookHandler(ILogger<ProcessWebhookHandler> logger, IOptions<AiiaOptions> aiiaOptions)
        {
            this.logger = logger;
            this.aiiaOptions = aiiaOptions.Value;
        }

        public async Task<string> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received webhook: {PayloadString}", request.PayloadString);

            // `X-Aiia-Signature` is provided optionally if client has configured `WebhookSecret` and is used to verify that webhook was sent by Aiia
            if (!AiiaHelper.VerifySignature(request.Timestamp, request.EventId, request.EventType, request.AiiaSignature, request.PayloadString, aiiaOptions.WebHookSecret))
            {
                logger.LogWarning("Failed to verify webhook signature");
            }
            else
            {
                var payload = JObject.Parse(request.PayloadString);

                var webHookType = payload.Properties().First().Name;
                var data = payload[webHookType];

                if (data == null)
                {
                    logger.LogInformation("Webhook data not parsed");
                }
                else
                {
                    var consentId = string.IsNullOrEmpty(data["consentId"]?.Value<string>())
                        ? string.Empty
                        : data["consentId"].Value<string>();
                    var webHookEvent = string.IsNullOrEmpty(data["event"]?.Value<string>())
                        ? string.Empty
                        : data["event"].Value<string>();
                    var webHookData = data["data"];

                    if (!string.IsNullOrEmpty(webHookEvent) && (webHookData != null))
                    {
                        //if (webHookEvent == "PaymentAuthorizationCreated")
                        if (webHookEvent == "PaymentAuthorizationPaymentsUpdated")
                        {
                            var accountId = string.IsNullOrEmpty(webHookData["accountId"]?.Value<string>())
                                ? string.Empty
                                : webHookData["accountId"].Value<string>();
                            var paymentAuthorizationId = string.IsNullOrEmpty(webHookData["paymentAuthorizationId"]?.Value<string>())
                                ? string.Empty
                                : webHookData["paymentAuthorizationId"].Value<string>();

                            if (!string.IsNullOrEmpty(paymentAuthorizationId))
                            {                                
                                var userName = GetUserByAuthId(paymentAuthorizationId);

                                var connection = new HubConnectionBuilder().WithUrl(aiiaOptions.SignalRUrl).Build();
                                await connection.StartAsync();

                                await connection.SendAsync("NotifyPaymentAuthorization", userName, paymentAuthorizationId);

                                logger.LogInformation("SignalR called");
                            }
                        }
                        else if (webHookEvent == "PaymentUpdated")
                        {
                            var accountId = string.IsNullOrEmpty(webHookData["accountId"]?.Value<string>())
                                ? string.Empty
                                : webHookData["accountId"].Value<string>();
                            var paymentId = string.IsNullOrEmpty(webHookData["paymentId"]?.Value<string>())
                                ? string.Empty
                                : webHookData["paymentId"].Value<string>();

                            if (!string.IsNullOrEmpty(paymentId))
                            {
                                var userName = GetUserByAuthId(paymentId);

                                var connection = new HubConnectionBuilder().WithUrl(aiiaOptions.SignalRUrl).Build();
                                await connection.StartAsync();

                                await connection.SendAsync("NotifyPaymentAuthorization", userName, paymentId);

                                logger.LogInformation("SignalR called");
                            }
                        }
                    }

                    return $"{webHookType} received";
                }
            }

            return string.Empty;
        }

        private string GetUserByAuthId(string paymentAuthorizationId)
        {
            return PAYMENT_USER_NAME;
        }
    }
}
