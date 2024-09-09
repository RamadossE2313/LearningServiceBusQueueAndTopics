using Azure.Identity;
using Azure.Messaging.ServiceBus;
using LearningServiceBusQueueAndTopicsSender.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Transactions;

#region Addingrequiredservices
ServiceCollection services = new ServiceCollection();
// serivice bus namespace 
services.AddSingleton(new ServiceBusClient("learning-servicebus.servicebus.windows.net", new DefaultAzureCredential()));
ServiceProvider serviceProvider = services.BuildServiceProvider();
#endregion

#region Solvingrequiredservices
ServiceBusClient serviceBusClient = serviceProvider.GetRequiredService<ServiceBusClient>();
// queue name
ServiceBusSender sender = serviceBusClient.CreateSender("duplicate-queue");
#endregion

#region SendingMessageToQueueOrTopic
string messageId = Guid.NewGuid().ToString();

ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(Order.DefaultOrder))
{
   MessageId = messageId,
   Subject = "Order Created",
   TimeToLive = TimeSpan.FromMinutes(1),
};
await sender.SendMessageAsync(message);
#endregion

#region SendingMessageToQueueOrTopic
ServiceBusMessage newMessage = new ServiceBusMessage(JsonSerializer.Serialize(Order.Orders.Skip(1).Take(1)))
{
    MessageId = "3fe477a02ab34aa5b56dd6e27b636d10",
    Subject = "Order Created"
};
await sender.SendMessageAsync(newMessage);
#endregion

#region SendingMessagesToQueueOrTopic
var messages = new List<ServiceBusMessage>();
foreach (var order in Order.Orders)
{
    messages.Add(new ServiceBusMessage(JsonSerializer.Serialize(order))
    {
        MessageId = "1",
        Subject = "Order Created"
    });
}
await sender.SendMessagesAsync(messages);
#endregion

#region SendingMessagesWithSessionToQueueOrTopic
var messagesWithSession = new List<ServiceBusMessage>();
foreach (var order in Order.Orders)
{
    messages.Add(new ServiceBusMessage(JsonSerializer.Serialize(Order.DefaultOrder))
    {
        Subject = "Order Created",
        SessionId = order.Id.ToString(),
    });
}
await sender.SendMessagesAsync(messagesWithSession);
#endregion

#region SendingMessagesWithSessionAndBatchesToQueueOrTopic
var messageWithSessionAndBatch = await sender.CreateMessageBatchAsync();
foreach (var order in Order.Orders)
{
    messageWithSessionAndBatch.TryAddMessage(new ServiceBusMessage(JsonSerializer.Serialize(Order.DefaultOrder))
    {
        Subject = "Order Created",
        SessionId = order.Id.ToString(),
    });
}

try
{
    await sender.SendMessagesAsync(messageWithSessionAndBatch);
    Console.WriteLine($"batch sent with count: {messageWithSessionAndBatch.Count} size: {messageWithSessionAndBatch.MaxSizeInBytes}");
}
catch (Exception)
{
    throw;
}

#endregion

#region SendingMessageWithPartitionAndScheduledMessageTimeToQueueOrTopic
ServiceBusMessage busMessage = new ServiceBusMessage(JsonSerializer.Serialize(Order.DefaultOrder))
{
    Subject = "Order Created",
    PartitionKey = Guid.NewGuid().ToString(),
    // schedule message will be send to queue but it will be available after the time specified
    ScheduledEnqueueTime = DateTime.Now.AddMinutes(30),
};
await sender.SendMessageAsync(busMessage);
#endregion

#region SendingMessageWithPartitionToQueueOrTopic
ServiceBusMessage transactionMessage = new ServiceBusMessage(JsonSerializer.Serialize(Order.DefaultOrder))
{
    Subject = "Order Created",
};

//A transaction times out after 2 minutes. The transaction timer starts when the first operation in the transaction starts.
//The operations that can be performed within a transaction scope are as follows:
//Send
//Complete
//Abandon
//Deadletter
//Defer
//Renew lock

using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
{
    await sender.SendMessageAsync(transactionMessage);
    ts.Complete();
}

#endregion