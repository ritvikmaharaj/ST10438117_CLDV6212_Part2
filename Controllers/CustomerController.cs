using DesignerCloset.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Reflection;

namespace DesignerCloset.Controllers
{
    public class CustomerController : Controller
    {

        private readonly TableStorageService _tableStorageService;
        public CustomerController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {

            var customers = await _tableStorageService.GetCustomersAsync();

            return View(customers);

        }

            public async Task<IActionResult> Delete(string partitionKey, string rowkey)
             {

            await _tableStorageService.DeleteCustomerAsync(partitionKey, rowkey); 
            return RedirectToAction("Index");

              }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
            public async Task<IActionResult> Create(Customer customer)
        {

            customer.PartitionKey = "Clients";

            customer.RowKey = Guid.NewGuid().ToString();

            await _tableStorageService.AddCustomerAsync(customer);

            return RedirectToAction("Index");

        }

      
    }
}
