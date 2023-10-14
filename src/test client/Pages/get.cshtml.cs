using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class GetProductModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GetProductModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public string ProductId { get; set; }

    public Product Product { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Send a GET request to your API to retrieve product details
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"https://localhost:44363/api/products/{ProductId}";
            var response = await httpClient.GetFromJsonAsync<Product>(apiUrl);

            if (response != null)
            {
                Product = response;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Product not found.");
            }
        }
        catch (Exception ex)
        {
            // Handle API errors
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
        }

        return Page();
    }
}
