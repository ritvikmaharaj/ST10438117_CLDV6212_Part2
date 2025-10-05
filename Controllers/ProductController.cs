using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace DesignerCloset.Controllers
{
    public class ProductController : Controller
    {
        private readonly BlobStorageService _blobStorageService;
        private readonly TableStorageService _tableStorageService;

        private const string PartitionKey = "Images";

        public ProductController()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=st10438117storage;AccountKey=X7eDt8qLiaWfigyHvcl6jPh0q9GQ8W4adalivKtZS3dHD1QNZIQwlnT/ktOIgbJn2JNRcX0hj+oY+AStlagTxw==;EndpointSuffix=core.windows.net";
            string containerName = "abcretail";
            string tableName = "Product";

            _blobStorageService = new BlobStorageService(connectionString, containerName);
            _tableStorageService = new TableStorageService(connectionString, tableName);
        }

        
        public async Task<IActionResult> Index()
        {
            var metadataList = await _tableStorageService.GetAllMetadataAsync();
            return View(metadataList);
        }

     
        [HttpPost]
 
        public async Task<IActionResult> Upload(IFormFile uploadedFile, string name, double price, string description)
        {
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                await _blobStorageService.UploadFileToBlobStorageAsync(uploadedFile);

                string blobUrl = _blobStorageService.GetBlobUrl(uploadedFile.FileName);

                var product = new ProductEntity
                {
                    PartitionKey = PartitionKey,
                    RowKey = uploadedFile.FileName,
                    FileName = uploadedFile.FileName,
                    BlobUrl = blobUrl,
                    Name = string.IsNullOrWhiteSpace(name) ? "No name" : name,
                    Price = price,
                    Description = string.IsNullOrWhiteSpace(description) ? "No description" : description,
                    UploadDate = DateTimeOffset.UtcNow
                };

                await _tableStorageService.UpsertImageMetadataAsync(product);
            }
            return RedirectToAction("Index");
        }

    }
}

