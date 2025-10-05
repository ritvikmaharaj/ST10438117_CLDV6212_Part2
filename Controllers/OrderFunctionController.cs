using DesignerCloset.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace DesignerCloset.Controllers
{
    public class OrderFunctionController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OrderFunctionController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["FunctionApi:BaseUrl"];

            try
            {
                var httpResponseMessage = await
                    httpClient.GetAsync(apiBaseUrl); 
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = await
                        httpResponseMessage.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var order = await
                        JsonSerializer.DeserializeAsync<IEnumerable<OrderFunction>>
                        (contentStream, options);

                    return View(order);
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Could not connect to the API. " +
                    "Please ensure the Azure Function is running. ";
                return View(new List<OrderFunction>());
            }

            ViewBag.ErrorMessage = "An unexpected error occurred retrieving orders.";
            return View(new List<OrderFunction>());

        }
    }
}

