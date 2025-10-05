using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DesignerCloset.Services;
using System;
using System.Threading.Tasks;
using DesignerCloset.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DesignerCloset.Controllers
{
    public class ContractController : Controller
    {
        private readonly FileStorageService _fileStorageService;

        public ContractController(FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Index()
        {
            List<FileModel> files;
            try
            {
                files = await _fileStorageService.ListFilesAsync("uploads");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Failed to load files : {ex.Message}";
                files = new List<FileModel>();
            }
            return View(files);
        }

        [HttpPost]

        public async Task<IActionResult> UploadFile(IFormFile file)
        {

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a file to upload");
                return await Index();
            }
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    string directoryName = "uploads";
                    string fileName = file.FileName;
                    await _fileStorageService.UploadFileAsync(directoryName, fileName, stream);
                }
                TempData["Message"] = $"Your contract has uploaded successfully. Our team will be in touch soon!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"File upload failed";

            }
            return RedirectToAction("Index");

        }
        [HttpPost]

        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be null of empty");
            }
            try
            {
                var fileStream = await _fileStorageService.DownloadFileAsync("uploads", fileName);
                if (fileStream == null)
                {
                    return NotFound($"File '{fileName}' not found");
                }
                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Error downloading file" + ex.Message);
            }
        }
    }
}
