namespace Mango.Services.PaymentAPI.Messaging
{
    public interface IAzureServiceBusConsumerPaymentAPI
    {
        Task Start();
        Task Stop();

    }
}
