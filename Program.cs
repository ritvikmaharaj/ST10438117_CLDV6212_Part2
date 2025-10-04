using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using System.Text.Json;

namespace TestQueue
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=st10438117storage;AccountKey=X7eDt8qLiaWfigyHvcl6jPh0q9GQ8W4adalivKtZS3dHD1QNZIQwlnT/ktOIgbJn2JNRcX0hj+oY+AStlagTxw==;EndpointSuffix=core.windows.net";

            var queueClient = new QueueClient(
                connectionString,
                "table",
                new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 }
                );

            await queueClient.CreateIfNotExistsAsync();
            var order = new { CustomerName = "John", ProductName = "Gucci Sneaker", Total = 15000 };
            string json = JsonSerializer.Serialize(order);

            await queueClient.SendMessageAsync(json);
            Console.WriteLine($"Message sent {json}");


        }
    }
}
