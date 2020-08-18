using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace CoreReceiverApp
{
    class Program
    {
        const string ServiceBusConnectionString = "<Connection_String>";
        static IQueueClient queueClient;
        static async Task Main(string[] args)
        {
            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register QueueClient's MessageHandler and receive messages in a loop
            // Register Session Handler and Receive Session Messages
            //RegisterOnSessionHandlerAndReceiveSessionMessages("offense");

            string[] playTypes = { "offense_sideline", "offense_endzone", "defense_sideline", "defense_endzone", "specialteams" };
            string messagePrinter;

            foreach (string play in playTypes)
            {
                Console.WriteLine($"Would you like to show commands for {play}? [y/n]");

                messagePrinter = Console.ReadLine();
                if (messagePrinter.ToLower() == "y")
                {
                    RegisterOnMessageHandlerAndReceiveMessagesAsync(play);

                    Console.ReadKey();

                    await queueClient.CloseAsync();
                }

                Console.WriteLine("======================================================");

            }

            // Send messages with sessionId set
            //await SendSessionMessagesAsync(numberOfSessions, numberOfMessagesPerSession);
        }

        //static void RegisterOnSessionHandlerAndReceiveSessionMessages(string QueueName)
        //{
        //    queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

        //    // Configure the SessionHandler Options in terms of exception handling, number of concurrent sessions to deliver etc.
        //    var sessionHandlerOptions =
        //            new SessionHandlerOptions(ExceptionReceivedHandler)
        //            {
        //                // Maximum number of Concurrent calls to the callback `ProcessSessionMessagesAsync`
        //                // Value 2 below indicates the callback can be called with a message for 2 unique
        //                // session Id's in parallel. Set it according to how many messages the application 
        //                // wants to process in parallel.
        //                MaxConcurrentSessions = 1,

        //                // Indicates the maximum time the Session Pump should wait for receiving messages for sessions.
        //                // If no message is received within the specified time, the pump will close that session and try to get messages
        //                // from a different session. Default is to wait for 1 minute to fetch messages for a session. Set to a 1 second
        //                // value here to allow the sample execution to finish fast but ideally leave this as 1 minute unless there 
        //                // is a specific reason to timeout earlier.
        //                MessageWaitTimeout = TimeSpan.FromSeconds(1),

        //                // Indicates whether SessionPump should automatically complete the messages after returning from User Callback.
        //                // False below indicates the Complete will be handled by the User Callback as in `ProcessSessionMessagesAsync`.
        //                AutoComplete = false
        //            };

        //    // Register the function that will process session messages
        //    queueClient.RegisterSessionHandler(ProcessSessionMessagesAsync, sessionHandlerOptions);

        //}

        static void RegisterOnMessageHandlerAndReceiveMessagesAsync(string QueueName)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };

            // Register the function that processes messages.
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        //static async Task ProcessSessionMessagesAsync(IMessageSession session, Message message, CancellationToken token)
        //{

        //    var offMessage = Encoding.UTF8.GetString(message.Body);

        //    Console.WriteLine($"Received Session: {session.SessionId} message: SequenceNumber: {message.SystemProperties.SequenceNumber} Body:{offMessage} at time: {DateTime.Now}");

        //    // Complete the message so that it is not received again.
        //    // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
        //    await session.CompleteAsync(message.SystemProperties.LockToken);

        //    // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
        //    // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
        //    // to avoid unnecessary exceptions.
        //}

        // Use this Handler to look at the exceptions received on the SessionPump
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

    }
}
