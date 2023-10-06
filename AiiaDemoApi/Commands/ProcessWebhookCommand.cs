using MediatR;

namespace AiiaDemoApi.Commands
{
    public class ProcessWebhookCommand: IRequest<string>
    {
        public string Timestamp;

        public string EventId;

        public string EventType;

        public string AiiaSignature;

        public string PayloadString;
    }
}
