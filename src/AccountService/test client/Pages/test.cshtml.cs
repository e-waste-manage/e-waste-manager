using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class CreateProductModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateProductModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public Product Product { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        using (var memoryStream = new MemoryStream())
        {
            await Product.testphoto.CopyToAsync(memoryStream);
            Product.PhotoFile = memoryStream.ToArray();
        }

        // Send the form data to your API
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("https://localhost:44363/api/products", Product);

        if (response.IsSuccessStatusCode)
        {
            // Product listing created successfully
            return RedirectToPage("Index"); // Redirect to a success page or another page
        }
        else
        {
            // Handle API errors
            ModelState.AddModelError(string.Empty, "Error creating product listing.");
            return Page();
        }
    }
}