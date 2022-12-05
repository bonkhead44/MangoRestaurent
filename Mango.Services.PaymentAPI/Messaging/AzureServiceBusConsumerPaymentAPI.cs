using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.PaymentAPI.Messages;
using Newtonsoft.Json;
using PaymentProcessor;
using System.Text;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class AzureServiceBusConsumerPaymentAPI : IAzureServiceBusConsumerPaymentAPI
    {
        private readonly IConfiguration _configuration;
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionPayment;
        private readonly string orderPaymentProcessTopic;
        private readonly string orderupdatepaymentresulttopic;
        private readonly ServiceBusProcessor checkOutProcessor;
        private readonly IMessageBus _iMessageBus;
        private readonly IProcessPayment _iProcessPayment;

        public AzureServiceBusConsumerPaymentAPI(IConfiguration configuration, IMessageBus iMessageBus, IProcessPayment iProcessPayment)
        {
            _configuration = configuration;
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            subscriptionPayment = _configuration.GetValue<string>("OrderPaymentProcessSubscription");
            orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopics");
            orderupdatepaymentresulttopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");
            var client = new ServiceBusClient(serviceBusConnectionString);
            checkOutProcessor = client.CreateProcessor(orderPaymentProcessTopic, subscriptionPayment);
            _iMessageBus = iMessageBus;
            _iProcessPayment = iProcessPayment;
        }

        public async Task Start()
        {
            checkOutProcessor.ProcessMessageAsync += ProcessPayments;
            checkOutProcessor.ProcessErrorAsync += ErrorHandler;
            await checkOutProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await checkOutProcessor.StopProcessingAsync();
            await checkOutProcessor.DisposeAsync();
        }

        private async Task ProcessPayments(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            PaymentRequestMessage paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(body);
            var result = _iProcessPayment.PaymentProcessor();
            UpdatePaymentResultMessage updatePaymentResultMessage = new()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId
            };

            try
            {
                await _iMessageBus.PusblishMessage(updatePaymentResultMessage, orderupdatepaymentresulttopic);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception e)
            {
                throw;
            }

        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
