namespace Mango.Services.Email.Messaging
{
    public interface IAzureServiceBusConsumerEmailAPI
    {
        Task Start();
        Task Stop();
    }
}
