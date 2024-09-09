using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata;
using System.Transactions;

#region Addingrequiredservices
ServiceCollection services = new ServiceCollection();
// serivice bus namespace 
// The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.
//
// Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
// If you use the default AmqpTcp, ensure that ports 5671 and 5672 are open.

services.AddSingleton(new ServiceBusClient("learning-servicebus.servicebus.windows.net", new DefaultAzureCredential(), new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
}));
ServiceProvider serviceProvider = services.BuildServiceProvider();
#endregion

#region Solvingrequiredservices
ServiceBusClient serviceBusClient = serviceProvider.GetRequiredService<ServiceBusClient>();

#endregion

#region ProcessingMessagesOneByOne
//Features:
//Event - Driven: Uses asynchronous event handlers to process messages as they arrive. This makes it well-suited for real-time processing scenarios.
//Automatic Handling: Automatically handles message completion, abandonment, or dead-lettering based on how you configure your message handlers.
//Error Handling: Provides a built-in mechanism to handle errors via the ProcessErrorAsync event.
//Built-In Optimizations: Handles message batching internally and optimizes message processing to reduce the number of network calls
//Advantages:
//Simplifies message processing with less boilerplate code.
//Handles message locking, renewing, and management automatically.
//Suitable for scenarios where you want to process messages as they arrive and rely on built-in retry and error handling features.
//Disadvantages:
//Less control over the exact number of messages processed in a batch and timing.
//Might introduce 
var serviceBusProcessor = serviceBusClient.CreateProcessor("duplicate-queue", new ServiceBusProcessorOptions());

try
{
    serviceBusProcessor.ProcessMessageAsync += MessageHandler;
    serviceBusProcessor.ProcessErrorAsync += ErrorHandler;

    await serviceBusProcessor.StartProcessingAsync();
    Console.WriteLine("");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await serviceBusProcessor.DisposeAsync();
    await serviceBusClient.DisposeAsync();
}

// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    // complete the message. message is deleted from the queue. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}
#endregion

#region ProcessingMessagesByBatch
//Features:

//Explicit Control: Allows you to manually control message batching and processing, which can be useful for scenarios that require fine-grained control over the message flow.
//Custom Logic: Enables you to implement custom logic for receiving messages and handling exceptions.
//Manual Management: You manually handle message completion, abandonment, and dead-lettering.
//Advantages:

//Greater flexibility in managing message receipt and processing.
//Allows you to customize batching behavior, such as the exact number of messages and timing.
//Suitable for scenarios where you need to integrate with other components or perform custom operations before completing or abandoning messages.
//Disadvantages:

//Requires more code to handle message processing, batching, and error handling.
//You must manually manage message lock renewal and completion, which can add complexity.
ServiceBusReceiver serviceBusReceiver = serviceBusClient.CreateReceiver("duplicate-queue");
try
{
    while (true)
    {
        var messages = await serviceBusReceiver.ReceiveMessagesAsync(10, TimeSpan.FromSeconds(10));
        if (messages.Count == 0)
        {
            Console.WriteLine("No messages received. Waiting...");
            continue;
        }

        foreach (var message in messages)
        {
            string body = message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            await serviceBusReceiver.CompleteMessageAsync(message);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
#endregion

#region ProcessingDeferredMessages
var receiver = serviceBusClient.CreateReceiver("duplicate-queue", new ServiceBusReceiverOptions
{
    // default is peek lock
    ReceiveMode = ServiceBusReceiveMode.PeekLock
});
while (true)
{
    Console.WriteLine($"Protocol: {serviceBusClient.TransportType}");
    Console.WriteLine($"Receiver Mode: {receiver.ReceiveMode}");
    using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    {
        var messages = await receiver.ReceiveMessagesAsync(10, TimeSpan.FromSeconds(10));

        List<long> sequenceNumbers = new List<long>();
        foreach (var message in messages)
        {
            sequenceNumbers.Add(message.SequenceNumber);
            await receiver.DeferMessageAsync(message);
        }

        Thread.Sleep(TimeSpan.FromSeconds(10));

        var defferedMessages = await receiver.ReceiveDeferredMessagesAsync(sequenceNumbers);

        foreach (var message in defferedMessages)
        {
            Console.WriteLine(message.Body);
            await receiver.CompleteMessageAsync(message);
        }

        ts.Complete();
    }
}
#endregion