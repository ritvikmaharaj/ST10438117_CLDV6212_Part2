using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Grpc.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace QueueFunction
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly string _storageConnectionString;
        private TableClient _tableClient;
        private BlobContainerClient _blobContainerClient;
        private readonly IFileStorageService _fileStorageService;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
            _storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=st10438117storage;AccountKey=X7eDt8qLiaWfigyHvcl6jPh0q9GQ8W4adalivKtZS3dHD1QNZIQwlnT/ktOIgbJn2JNRcX0hj+oY+AStlagTxw==;EndpointSuffix=core.windows.net";
            var serviceClient = new TableServiceClient(_storageConnectionString);
            _tableClient = serviceClient.GetTableClient("OrderTable");
            _blobContainerClient = new BlobContainerClient(
                _storageConnectionString, "sneaker-pics");
            string fileShareName = "uploads";
            _fileStorageService = new AzureFileShareStorageService(_storageConnectionString, fileShareName);
        }






        // Queue Storage Function
        [FunctionName(nameof(ProcessOrderQueueMessage))]
        public async Task ProcessOrderQueueMessage([QueueTrigger("table", Connection = "connection")] string messageText)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {messageText}");

            // Ensure the table exists before adding entities
            await _tableClient.CreateIfNotExistsAsync();

            // Deserialize the JSON string into your Order object
            var order = JsonSerializer.Deserialize<Order>(messageText);

            if (order == null)
            {
                _logger.LogError("Failed to deserialize JSON message into Order.");
                return;
            }

            // Set required PartitionKey and RowKey for Table Storage
            if (string.IsNullOrEmpty(order.PartitionKey))
                order.PartitionKey = "Orders";

            if (string.IsNullOrEmpty(order.RowKey))
                order.RowKey = Guid.NewGuid().ToString();

            _logger.LogInformation($"Saving order entity with RowKey: {order.RowKey}");

            // Add the entity to the Order table
            await _tableClient.AddEntityAsync(order);
            _logger.LogInformation("Successfully saved order to table.");
        }


        // Table Storage Function
        [FunctionName("GetOrder")]
        public async Task<IActionResult> GetOrder(
        [Microsoft.Azure.WebJobs.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Anonymous, "get", Route = "order")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request to get all orders");

            try
            {
                var orders = await _tableClient.QueryAsync<Order>(o => o.PartitionKey == "Orders").ToListAsync();
                return new OkObjectResult(orders);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to query table storage");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        // Blob Storage Function
        [FunctionName("AddOrderWithImage")]
        public async Task<HttpResponseData> AddOrderWithImage(
     [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sneaker-with-image")] HttpRequestData req)
        {

            _logger.LogInformation("C# HTTP Trigger Function to add order with image received request");

            var newOrder = new Order();
            string? uploadedBlobUrl = null;

            var multipartReader = new MultipartReader(
                req.Headers.GetValues("Content-Type").First().Split(';')[1].Trim().Split('=')[1], req.Body);

            var section = await multipartReader.ReadNextSectionAsync();

            while (section != null)
            {
                var contentDisposition = section.Headers["Content-Disposition"].ToString();
                var name = contentDisposition.Split(";")[1].Trim().Split('=')[1].Trim('"');

                if (name == "CustomerName" || name == "ProductName")
                {
                    var value = await new StreamReader(section.Body).ReadToEndAsync();
                    if (name == "CustomerName")
                        newOrder.CustomerName = value;

                    else if (name == "ProductName")
                        newOrder.ProductName = value;

                    else if (name == "Total")
                    {
                        if (double.TryParse(value, out var total))
                        {
                            newOrder.Total = total;
                        }
                    }
                }
                else if (name == "SneakerImage")
                {
                    var fileName = contentDisposition.Split(';')[2].Trim().Split('=')[1].Trim('"');
                    var uniqueFileName = $"{Guid.NewGuid()}-{Path.GetFileName(fileName)}{Path.GetExtension(fileName)}";
                    var blobClient = _blobContainerClient.GetBlobClient(uniqueFileName);

                    await blobClient.UploadAsync(section.Body, true);
                    uploadedBlobUrl = blobClient.Uri.ToString();
                }

                section = await multipartReader.ReadNextSectionAsync();
            }


            if (string.IsNullOrEmpty(newOrder.CustomerName) ||
                string.IsNullOrEmpty(newOrder.ProductName) ||
                string.IsNullOrEmpty(uploadedBlobUrl))
            {
                _logger.LogWarning("Missing required fields.");
                return (HttpResponseData)req.CreateResponse(HttpStatusCode.BadRequest);
            }


            newOrder.PartitionKey = "Orders";
            newOrder.RowKey = Guid.NewGuid().ToString();
            newOrder.SneakerPictureUrl = uploadedBlobUrl;

            await _tableClient.AddEntityAsync(newOrder);
            _logger.LogInformation($"Successfully added order");

            return (HttpResponseData)req.CreateResponse(HttpStatusCode.Created);
        }

        // File Storage Function
        [FunctionName("UploadFileToShare")]
        public async Task<HttpResponseData> UploadFileToShare(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload-file")] HttpRequestData req,
    FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UploadFileToShare");
            logger.LogInformation("Azure Function 'UploadFileToShare' triggered via HTTP POST");

            if (!req.Headers.TryGetValues("Content-Type", out var contentTypeValues))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Missing Content-Type header.");
                return badResponse;
            }

            var contentType = contentTypeValues.FirstOrDefault();
            if (string.IsNullOrEmpty(contentType))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Missing Content-Type value.");
                return badResponse;
            }

            var boundary = HeaderUtilities.RemoveQuotes(System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType).Boundary).Value;
            if (string.IsNullOrEmpty(boundary))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid multipart/form-data request.");
                return badResponse;
            }

            var reader = new MultipartReader(boundary, req.Body);
            var section = await reader.ReadNextSectionAsync();

            string? fileName = null;
            MemoryStream fileStream = new MemoryStream();

            while (section != null)
            {
                var contentDisposition = section.GetContentDispositionHeader();

                if (contentDisposition != null &&
                    contentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    fileName = contentDisposition.FileName.Value.Trim('"');
                    await section.Body.CopyToAsync(fileStream);
                    fileStream.Position = 0;
                    break;
                }

                section = await reader.ReadNextSectionAsync();
            }

            if (string.IsNullOrEmpty(fileName) || fileStream.Length == 0)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("No valid file found in upload.");
                return badResponse;
            }

            try
            {
                string directoryName = "uploads";
                await _fileStorageService.UploadFileAsync(directoryName, fileName, fileStream);

                var okResponse = req.CreateResponse(HttpStatusCode.OK);
                await okResponse.WriteStringAsync($"Successfully uploaded file: {fileName}");
                return okResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "File upload failed.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Upload failed: " + ex.Message);
                return errorResponse;
            }
        }

    }
}

