using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using E_waste.Areas.Identity.Data;
using E_waste.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace E_waste.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductDBContext _context;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClient2;

        public ProductsController(ProductDBContext context)
        {
            _context = context;
            _httpClient = new HttpClient()
            {
                //BaseAddress = new Uri("https://localhost:44363/")
                BaseAddress = new Uri("http://donorservice-dev.eba-3msbepdm.ap-southeast-1.elasticbeanstalk.com")
            };
            _httpClient2 = new HttpClient()
            {
                //BaseAddress = new Uri("https://localhost:7128")
                BaseAddress = new Uri("http://receiverservice-dev.eba-ucbaszhk.ap-southeast-1.elasticbeanstalk.com")
            };
        }

        // GET: Products
        public async Task<IActionResult> Index([Bind("ProductId,ListedDate,Quantity,UserID,PickupLocation,ContactNumber,Status,Name,Description,Category,VideoUrl,PhotoUrl,VideoFile,PhotoFile")] Product product)
        {
            if (HttpContext.Session.GetString("Userid") == "" || HttpContext.Session.GetString("Userid") == null)
                return LocalRedirect("/Identity/Account/Login");

            var products = new List<Product>();

            HttpResponseMessage response = await _httpClient.GetAsync("/api/products/category/Laptop");
            string responsemessage = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Handle the API response here
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (apiResponse != null)
                {
                    products = JsonConvert.DeserializeObject<List<Product>>(apiResponse);
                }
            }

            return products != null ? 
                          View(products) :
                          Problem("Entity set 'ProductDBContext.Products'  is null.");
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (HttpContext.Session.GetString("Userid") == "" || HttpContext.Session.GetString("Userid") == null)
                return LocalRedirect("/Identity/Account/Login");

            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/AddOrEdit
        public IActionResult AddOrEdit()
        {
            if (HttpContext.Session.GetString("Userid") == "" || HttpContext.Session.GetString("Userid") == null)
                return LocalRedirect("/Identity/Account/Login");

            return View(new Product());
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("ProductId,ListedDate,Quantity,UserID,PickupLocation,ContactNumber,Status,Name,Description,Category,VideoUrl,PhotoUrl,VideoFile,PhotoFile,ProductPhoto")] Product product)
        {

            if (HttpContext.Session.GetString("Userid") != "")
            {
                Guid userguid = new Guid(HttpContext.Session.GetString("Userid"));
                product.UserID = userguid;
            }
                 
            if (ModelState.IsValid)
            {
                if (product.ProductPhoto != null && product.ProductPhoto.Length > 0)
                {
                    // Read the file stream into a byte array
                    using (var stream = new MemoryStream())
                    {
                        await product.ProductPhoto.CopyToAsync(stream);
                        product.ProductPhoto = null; // Set the property to null to avoid saving the file to the server
                        product.PhotoFile = stream.ToArray(); // Assign the byte array to the model property
                    }
                }

                var responseMessage = await _httpClient.PostAsJsonAsync("api/products", product);
                string response = await responseMessage.Content.ReadAsStringAsync();

                //product.ProductId = Guid.NewGuid();
                //_context.Add(product);
                //await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Request/5
        public async Task<IActionResult> Request(Guid? id)
        {
            if (HttpContext.Session.GetString("Userid") == "" || HttpContext.Session.GetString("Userid") == null)
                return LocalRedirect("/Identity/Account/Login");

            if (id == null)
            {
                return NotFound();
            }

            Product product = new Product();
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/{id}");
            await _httpClient.GetAsync($"api/products/updatestatus/{id}/{ProductStatus.Reserved}");
            if (response.IsSuccessStatusCode)
            {
                // Handle the API response here
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (apiResponse != null)
                {
                    product = JsonConvert.DeserializeObject<Product>(apiResponse);
                }
            }
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (HttpContext.Session.GetString("Userid") == "" || HttpContext.Session.GetString("Userid") == null)
                return LocalRedirect("/Identity/Account/Login");

            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (HttpContext.Session.GetString("Userid") == "" || HttpContext.Session.GetString("Userid") == null)
                return LocalRedirect("/Identity/Account/Login");

            if (_context.Products == null)
            {
                return Problem("Entity set 'ProductDBContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(Guid id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }


        public IActionResult CreateRequest()
        {
            if (HttpContext.Session.GetString("Userid") == "" || HttpContext.Session.GetString("Userid") == null)
                return LocalRedirect("/Identity/Account/Login");

            return View(new ReceiverRequestItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(ReceiverRequestItem requestItem)
        {
            if (HttpContext.Session.GetString("Userid") == "" || HttpContext.Session.GetString("Userid") == null)
                return LocalRedirect("/Identity/Account/Login");
            else
            {
                Guid userguid = new Guid(HttpContext.Session.GetString("Userid"));
                requestItem.ReceiverId = userguid;
            }
            
            if (ModelState.IsValid)
            {
                var responseMessage = await _httpClient2.PostAsJsonAsync("Receiver", requestItem);
                return View("RequestCreated",requestItem);
            }
            return View(requestItem);
        }
    }
}
