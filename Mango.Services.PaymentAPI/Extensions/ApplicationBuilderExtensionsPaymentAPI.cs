using Mango.Services.PaymentAPI.Messaging;

namespace Mango.Services.PaymentAPI.Extensions
{
    public static class ApplicationBuilderExtensionsPaymentAPI
    {
        public static IAzureServiceBusConsumerPaymentAPI ServiceBusConsumer { get; set; }
        public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
        {
            ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumerPaymentAPI>();
            var hostedApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            hostedApplicationLife.ApplicationStarted.Register(OnStart);
            hostedApplicationLife.ApplicationStopped.Register(OnStop);
            return app;
        }

        private static void OnStart()
        {
            ServiceBusConsumer.Start();
        }

        private static void OnStop()
        {
            ServiceBusConsumer.Stop();
        }
    }
}
