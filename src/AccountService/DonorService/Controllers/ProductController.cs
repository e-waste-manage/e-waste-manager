using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using Amazon.S3;
using Amazon.S3.Model;
using DonorService;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IAmazonS3 _s3Client;
    private readonly string _s3BucketName = "ewastestore1";
    private readonly string _dynamoDBTableName = "ProductList";

    public ProductsController(IAmazonDynamoDB dynamoDbClient, IAmazonS3 s3Client)
    {
        _dynamoDbClient = dynamoDbClient;
        _s3Client = s3Client;
    }

    // GET: api/products/{productId}
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(Guid productId)
    {
        try
        {
            // Retrieve the product by ProductId from DynamoDB
            var table = Table.LoadTable(_dynamoDbClient, _dynamoDBTableName);
            var search = table.Query(new QueryFilter("ProductId", QueryOperator.Equal, productId));

            var document = await search.GetNextSetAsync();
            if (document.Count == 0)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            // Map the DynamoDB document to your Product model
            var product = new Product
            {
                ProductId = Guid.Parse(document[0]["ProductId"]),
                Name = document[0]["Name"],
                ListedDate = (DateTime)document[0]["ListedDate"],
                Quantity = (int)document[0]["Quantity"],
                Description = document[0]["Description"],
                Category = (ProductCategory)Enum.Parse(typeof(ProductCategory), document[0]["Category"]),
                PickupLocation = document[0]["PickupLocation"],
                ContactNumber = document[0]["ContactNumber"],
                Status = (ProductStatus)Enum.Parse(typeof(ProductStatus),document[0]["ProductStatus"]),
                UserID = Guid.Parse(document[0]["UserID"])
            };

            if (document[0].TryGetValue("PhotoUrl", out DynamoDBEntry photourl))
            {
                string signedPhotoUrl = GetSignedS3ObjectUrl(photourl);
                product.PhotoUrl = signedPhotoUrl;
            }

            if (document[0].TryGetValue("VideoUrl", out DynamoDBEntry videourl))
            {
                string signedVideoUrl = GetSignedS3ObjectUrl(videourl);
                product.VideoUrl = signedVideoUrl;
            }
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
            TableName = _dynamoDBTableName,
            Item = new Dictionary<string, AttributeValue>
                {
                    { "ProductId", new AttributeValue { S = Guid.NewGuid().ToString() } },
                    { "ListedDate" , new AttributeValue {S = DateTime.Now.ToString() } },
                    { "Quantity" , new AttributeValue {N = product.Quantity.ToString() } },
                    { "Name", new AttributeValue { S = product.Name } },
                    { "Description", new AttributeValue { S = product.Description } },
                    { "Category", new AttributeValue { S = product.Category.ToString() } },
                    { "PickupLocation", new AttributeValue { S = product.PickupLocation.ToString() } },
                    { "ContactNumber", new AttributeValue { S = product.ContactNumber.ToString() } },
                    { "ProductStatus", new AttributeValue { S = product.Status.ToString() } },
                    { "UserID", new AttributeValue { S = product.UserID.ToString() } }
                }
        };


        if (product.VideoUrl != null)
        {
            request.Item.Add("VideoUrl", new AttributeValue { S = product.VideoUrl });
        }

        if (product.PhotoUrl != null)
        {
            request.Item.Add("PhotoUrl", new AttributeValue { S = product.PhotoUrl });
        }

        // Perform the PutItem operation to insert the product into DynamoDB
        await _dynamoDbClient.PutItemAsync(request);

        // Return a response to the client, including the newly created product's data.
        // Replace with the actual URL or resource path for accessing the product.
        string productUrl = $"/api/products/{product.ProductId}";
        return Created(productUrl, product);
    }

    // PUT: api/products/{productId}
    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateProduct(Guid productId, [FromBody] Product updatedProduct)
    {
        try
        {
            // Check if the product with the given ID exists
            var table = Table.LoadTable(_dynamoDbClient, _dynamoDBTableName);
            var search = table.Query(new QueryFilter("ProductId", QueryOperator.Equal, productId));

            var document = await search.GetNextSetAsync();
            if (document.Count == 0)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            // Update the product attributes
            var existingProduct = document[0];
            existingProduct["Name"] = updatedProduct.Name;
            existingProduct["Description"] = updatedProduct.Description;
            existingProduct["Category"] = updatedProduct.Category.ToString();
            existingProduct["Quantity"] = updatedProduct.Quantity;
            existingProduct["PickupLocation"] = updatedProduct.PickupLocation;
            existingProduct["ContactNumber"] = updatedProduct.ContactNumber;
            existingProduct["ProductStatus"] = updatedProduct.Status.ToString();

            if (updatedProduct.VideoFile != null)
            {
                string videoObjectKey = Guid.NewGuid().ToString() + ".mp4";
                await UploadFileToS3(updatedProduct.VideoFile, videoObjectKey);
                updatedProduct.VideoUrl = GetS3ObjectUrl(videoObjectKey);
            }

            if (updatedProduct.PhotoFile != null)
            {
                string photoObjectKey = Guid.NewGuid().ToString() + ".jpg";
                await UploadFileToS3(updatedProduct.PhotoFile, photoObjectKey);
                updatedProduct.PhotoUrl = GetS3ObjectUrl(photoObjectKey);
            }

            if (existingProduct.TryGetValue("VideoUrl", out var vurl) && updatedProduct.VideoUrl != null)
            {
                _ = DeleteFileFromS3(vurl);
                existingProduct["VideoUrl"] = updatedProduct.VideoUrl;
            }
            else existingProduct.Add("VideoUrl", updatedProduct.VideoUrl);

            if (existingProduct.TryGetValue("PhotoUrl", out var purl) && updatedProduct.PhotoUrl != null )
            {
                _ = DeleteFileFromS3(purl);
                existingProduct["PhotoUrl"] = updatedProduct.PhotoUrl;
            }
            else existingProduct.Add("PhotoUrl", updatedProduct.PhotoUrl);

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
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        try
        {
            // Check if the product with the given ID exists
            var table = Table.LoadTable(_dynamoDbClient, _dynamoDBTableName);
            var search = table.Query(new QueryFilter("ProductId", QueryOperator.Equal, productId));

            var document = await search.GetNextSetAsync();

            if (document.Count == 0)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            if (document[0].TryGetValue("PhotoUrl", out DynamoDBEntry photourl))
            {
                _ = DeleteFileFromS3(photourl);
            }

            if (document[0].TryGetValue("VideoUrl", out DynamoDBEntry videourl))
            {
                _ = DeleteFileFromS3(videourl);
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
    public async Task<IActionResult> FilterandSort(ProductCategory categoryName)
    {
        try
        {
            using (var client = new AmazonDynamoDBClient())
            {
                var request = new QueryRequest
                {
                    TableName = "ProductList",
                    IndexName = "CategorySort",
                    KeyConditionExpression = "Category = :category",
                    ScanIndexForward = false,
                    FilterExpression = "ProductStatus = :status", // Filter condition on a non-key attribute
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":category", new AttributeValue { S = categoryName.ToString() }},
                        { ":status", new AttributeValue { S = ProductStatus.Created.ToString() }}
                    },
                };

                var response = await client.QueryAsync(request);

                var products = new List<Product>();

                foreach (var item in response.Items)
                {
                    var product = new Product
                    {
                        ProductId = Guid.Parse(item["ProductId"].S),
                        Name = item["Name"].S,
                        Description = item["Description"].S,
                        Category = (ProductCategory)Enum.Parse(typeof(ProductCategory), item["Category"].S)
                    };
                    if (item.TryGetValue("VideoUrl", out var vurl)) product.VideoUrl = GetSignedS3ObjectUrl(vurl.S);
                    if (item.TryGetValue("PhotoUrl", out var purl)) product.PhotoUrl = GetSignedS3ObjectUrl(purl.S);
                    products.Add(product);
                }


                return Ok(products);
            }
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
    private async Task DeleteFileFromS3(string s3Url)
    {
        string[] parts = s3Url.Split('/');
        string objectKey = parts[parts.Length - 1];

        var request = new DeleteObjectRequest
        {
            BucketName = _s3BucketName,
            Key = objectKey,
        };
        await _s3Client.DeleteObjectAsync(request);  
    }

    private string GetS3ObjectUrl(string objectKey)
    {
        return $"https://{_s3BucketName}.s3.amazonaws.com/{objectKey}";
    }

    private string GetSignedS3ObjectUrl(DynamoDBEntry unsignedUrl)
    {
        string[] parts = unsignedUrl.AsString().Split('/');
        string objectKey = parts[parts.Length - 1];

        GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
        {
            BucketName = _s3BucketName,
            Key = objectKey,
            Expires = DateTime.Now.AddMinutes(5)
        };

        string signedUrl = _s3Client.GetPreSignedURL(request);

        return signedUrl;
    }
}