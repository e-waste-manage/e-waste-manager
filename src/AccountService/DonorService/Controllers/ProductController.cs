using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IAmazonS3 _s3Client;
    private readonly string _s3BucketName = "YOUR_S3_BUCKET_NAME";

    public ProductsController(IAmazonDynamoDB dynamoDbClient, IAmazonS3 s3Client)
    {
        _dynamoDbClient = dynamoDbClient;
        _s3Client = s3Client;
    }

    // GET: api/products/{productId}
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(int productId)
    {
        try
        {
            // Retrieve the product by ProductId from DynamoDB
            var table = Table.LoadTable(_dynamoDbClient, "Product_List");
            var search = table.Query(new QueryFilter("ProductId", QueryOperator.Equal, productId));

            var document = await search.GetNextSetAsync();
            if (document.Count == 0)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            // Map the DynamoDB document to your Product model
            var product = new Product
            {
                ProductId = int.Parse(document[0]["ProductId"]),
                Name = document[0]["Name"],
                Description = document[0]["Description"],
                Price = decimal.Parse(document[0]["Price"]),
                Category = document[0]["Category"],
                VideoUrl = document[0]["VideoUrl"],
                PhotoUrl = document[0]["PhotoUrl"]
            };

            return Ok(product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        // Handle video and photo upload to Amazon S3.
        if (product.VideoFile != null)
        {
            string videoObjectKey = Guid.NewGuid().ToString() + ".mp4";
            await UploadFileToS3(product.VideoFile, videoObjectKey);
            product.VideoUrl = GetS3ObjectUrl(videoObjectKey);
        }

        if (product.PhotoFile != null)
        {
            string photoObjectKey = Guid.NewGuid().ToString() + ".jpg";
            await UploadFileToS3(product.PhotoFile, photoObjectKey);
            product.PhotoUrl = GetS3ObjectUrl(photoObjectKey);
        }

        var request = new PutItemRequest
        {
            TableName = "Product_List",
            Item = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { N = product.ProductId.ToString() } },
                    { "Name", new AttributeValue { S = product.Name } },
                    { "Description", new AttributeValue { S = product.Description } },
                    { "Price", new AttributeValue { N = product.Price.ToString() } },
                    { "Category", new AttributeValue { S = product.Category } },
                    { "VideoUrl", new AttributeValue { S = product.VideoUrl } },
                    { "PhotoUrl", new AttributeValue { S = product.PhotoUrl } }
                }
        };

        // Perform the PutItem operation to insert the product into DynamoDB
        await _dynamoDbClient.PutItemAsync(request);

        // Return a response to the client, including the newly created product's data.
        // Replace with the actual URL or resource path for accessing the product.
        string productUrl = $"/api/products/{product.ProductId}";
        return Created(productUrl, product);
    }

    // PUT: api/products/{productId}
    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateProduct(int productId, [FromBody] Product updatedProduct)
    {
        try
        {
            // Check if the product with the given ID exists
            var table = Table.LoadTable(_dynamoDbClient, "Product_List");
            var search = table.Query(new QueryFilter("ProductId", QueryOperator.Equal, productId.ToString()));

            var document = await search.GetNextSetAsync();
            if (document.Count == 0)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            // Update the product attributes
            var existingProduct = document[0];
            existingProduct["Name"] = updatedProduct.Name;
            existingProduct["Description"] = updatedProduct.Description;
            existingProduct["Price"] = updatedProduct.Price.ToString();
            existingProduct["Category"] = updatedProduct.Category;

            // Save the updated product to DynamoDB
            await table.UpdateItemAsync(existingProduct);

            return Ok(existingProduct);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    // DELETE: api/products/{productId}
    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        try
        {
            // Check if the product with the given ID exists
            var table = Table.LoadTable(_dynamoDbClient, "Product_List");
            var search = table.Query(new QueryFilter("ProductId", QueryOperator.Equal, productId.ToString()));

            var document = await search.GetNextSetAsync();
            if (document.Count == 0)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            // Delete the product from DynamoDB
            await table.DeleteItemAsync(document[0]);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    // GET: api/products/category/{categoryName}
    [HttpGet("category/{categoryName}")]
    public async Task<IActionResult> GetProductsByCategory(string categoryName)
    {
        try
        {
            // Query DynamoDB for products with the specified category
            var table = Table.LoadTable(_dynamoDbClient, "Product_List");
            var filter = new QueryFilter("Category", QueryOperator.Equal, categoryName);
            var search = table.Query(filter);

            var productDocuments = await search.GetNextSetAsync();

            if (productDocuments.Count == 0)
            {
                return NotFound($"No products found in category '{categoryName}'.");
            }

            // Map the DynamoDB documents to a list of Product models
            var products = new List<Product>();
            foreach (var document in productDocuments)
            {
                var product = new Product
                {
                    ProductId = int.Parse(document["ProductId"]),
                    Name = document["Name"],
                    Description = document["Description"],
                    Price = decimal.Parse(document["Price"]),
                    Category = document["Category"],
                    VideoUrl = document["VideoUrl"],
                    PhotoUrl = document["PhotoUrl"]
                };
                products.Add(product);
            }

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    private async Task UploadFileToS3(byte[] fileData, string objectKey)
    {
        using (var stream = new MemoryStream(fileData))
        {
            var request = new PutObjectRequest
            {
                BucketName = _s3BucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = "image/jpeg", // Adjust as needed
            };

            await _s3Client.PutObjectAsync(request);
        }
    }

    private string GetS3ObjectUrl(string objectKey)
    {
        return $"https://{_s3BucketName}.s3.amazonaws.com/{objectKey}";
    }
}