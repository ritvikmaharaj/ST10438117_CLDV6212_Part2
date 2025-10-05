using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using DesignerCloset.Models;


namespace DesignerCloset.Controllers
{
    public class BlobController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public BlobController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(SneakerImage model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var httpClient = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["FunctionApi:BaseUrl"];


            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(new StringContent(model.CustomerName), "CustomerName");
                formData.Add(new StringContent(model.ProductName), "ProductName");


                if (model.SneakerPicture != null)
                {
                    formData.Add(
                        new StreamContent(model.SneakerPicture.OpenReadStream()),
                        "SneakerImage",
                        model.SneakerPicture.FileName
                    );
                }

                var httpResponseMessage = await httpClient.PostAsync($"{apiBaseUrl}sneaker-with-image", formData);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Successfully added {model.CustomerName}'s order with an image!";
                    return RedirectToAction("OrderFunction");
                }

            }

            ModelState.AddModelError(string.Empty, "An error occurred while calling the API.");
            return View(model);
        }
    }
}

