using System;
using System.IO;
using Azure.Storage.Queues; // For Azure Queue Storage
using Azure.Storage.Queues.Models; // For QueueMessage
using Azure.Storage.Files.Shares; // For Azure File Storage
using Azure.Storage.Files.Shares.Models; // For Directory and File Client

class Program
{
    static void Main(string[] args)
    {
        // Connection string and queue/file information
        string connectionString = "Your_Connection_String";
        string queueName = "fibonacci-queue";
        string fileName = "Olebogeng-Dibodu.txt";

        // Create QueueClient to access Azure Queue Storage
        QueueClient queueClient = new QueueClient(connectionString, queueName);
        queueClient.CreateIfNotExists();

        // Generate Fibonacci sequence and store it in the queue
        if (queueClient.Exists())
        {
            Console.WriteLine("Generating Fibonacci sequence and storing in queue...");

            int a = 0, b = 1;
            while (a <= 233)
            {
                // Add each Fibonacci number to the queue as a message
                queueClient.SendMessage(a.ToString());
                Console.WriteLine($"Enqueued: {a}");

                // Generate next Fibonacci number
                int temp = a;
                a = b;
                b = temp + b;
            }

            Console.WriteLine("Fibonacci sequence generated and stored in Azure Queue.");
        }
        else
        {
            Console.WriteLine("Error: Queue does not exist.");
            return;
        }

        // Create ShareClient for Azure File Storage
        ShareClient share = new ShareClient(connectionString, "fibonacci-files");
        share.CreateIfNotExists();

        // Create the directory and the file in Azure File Storage
        ShareDirectoryClient directory = share.GetRootDirectoryClient();
        directory.CreateIfNotExists();

        ShareFileClient file = directory.GetFileClient(fileName);
        file.Create(1024); // Create a file with a certain size

        // Retrieve messages from the queue and write them to a file
        if (queueClient.Exists())
        {
            Console.WriteLine("Processing Fibonacci sequence messages...");

            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    // Retrieve and process each message
                    foreach (QueueMessage message in queueClient.ReceiveMessages(32).Value)
                    {
                        writer.WriteLine(message.MessageText);
                        queueClient.DeleteMessage(message.MessageId, message.PopReceipt); // Delete the message from the queue
                    }

                    writer.Flush();
                    stream.Position = 0;
                    file.Upload(stream); // Upload the text file to Azure File Storage
                }
            }

            Console.WriteLine($"Fibonacci sequence written to {fileName} in Azure File Storage.");
        }
        else
        {
            Console.WriteLine("Error: Queue does not exist.");
        }
    }
}