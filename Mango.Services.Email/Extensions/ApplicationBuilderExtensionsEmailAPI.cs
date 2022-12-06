using Mango.Services.Email.Messaging;

namespace Mango.Services.Email.Extensions
{
    public static class ApplicationBuilderExtensionsEmailAPI
    {
        public static IAzureServiceBusConsumerEmailAPI ServiceBusConsumer { get; set; }
        public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
        {
            ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumerEmailAPI>();
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
