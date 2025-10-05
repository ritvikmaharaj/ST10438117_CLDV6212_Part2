using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class FileController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _functionBaseUrl;

    public FileController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _functionBaseUrl = configuration["FunctionApi:BaseUrl"];
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file.";
            return RedirectToAction("Index");
        }

        var client = _httpClientFactory.CreateClient();

        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream();

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

       
        var functionUploadUrl = $"{_functionBaseUrl}/upload-file";

        var response = await client.PostAsync(functionUploadUrl, content);

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "File uploaded successfully.";
        }
        else
        {
            var errorMsg = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"File upload failed: {errorMsg}";
        }

        return RedirectToAction("Index");
    }

    public IActionResult Index()
    {
        return View();
    }
}

