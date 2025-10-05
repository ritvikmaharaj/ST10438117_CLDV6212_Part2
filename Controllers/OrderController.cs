using Azure.Storage.Queues;
using DesignerCloset.Models;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Queues;
using DesignerCloset.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DesignerCloset.Controllers
{
    public class OrderController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueClient _queueClient;

        public OrderController(TableStorageService tableStorageService, IConfiguration configuration)
        {
            _tableStorageService = tableStorageService;
            _queueClient = new QueueClient(configuration.GetConnectionString("AzureStorage"), "orderqueue");
            _queueClient.CreateIfNotExists();
        }




        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var customers = await _tableStorageService.GetCustomersAsync();
            var products = await _tableStorageService.GetProductsAsync();

            ViewBag.Customers = customers;
            ViewBag.Products = products;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            order.PartitionKey = "Purchases";
            order.RowKey = Guid.NewGuid().ToString();
            order.OrderDate = DateTime.UtcNow;

            await _tableStorageService.AddOrderAsync(order);

            var messagePayload = new
            {

                order.ProductId,
                order.ShoeSize,
                order.Quantity,
                order.OrderDate
            };

            string jsonMessage = JsonSerializer.Serialize(messagePayload);
            string base64Message = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonMessage));
            await _queueClient.SendMessageAsync(base64Message);

            return RedirectToAction("Index", "Customer");
        }
    }
}

