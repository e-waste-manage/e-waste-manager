using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using ReceiverService.Models;

namespace ReceiverService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReceiverController : ControllerBase
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _dynamoDBTableName = "RequestList";
        private readonly ILogger<ReceiverController> _logger;
        private readonly HttpClient _httpClient;
        private readonly string updateproductstatusurl;

        public ReceiverController(ILogger<ReceiverController> logger, IAmazonDynamoDB dynamoDbClient)
        {
            _logger = logger;
            _dynamoDbClient = dynamoDbClient;
            _httpClient = new HttpClient();
            updateproductstatusurl = "";
        }

        [HttpGet("{requestId}")]
        public async Task<IActionResult> GetReceiverItem(Guid requestId)
        {
            try
            {
                // Perform a database query or any other necessary logic to retrieve the item by ID
                // For example, you can use _dynamoDbClient to fetch the item from DynamoDB
                // Replace this with your actual data retrieval logic
                var table = Table.LoadTable(_dynamoDbClient, _dynamoDBTableName);
                var search = table.Query(new QueryFilter("RequestId", QueryOperator.Equal, requestId));


                var document = await search.GetNextSetAsync();
                if (document.Count == 0)    
                {
                    return NotFound($"Request with ID {requestId} not found.");
                }

                var receiverItemRequest = new ReceiverItemRequest
                {
                    RequestId = Guid.Parse(document[0]["RequestId"]),
                    ReceiverId = Guid.Parse(document[0]["ReceiverId"]),
                    ContactNumber = document[0]["ContactNumber"],
                    RequestItemName = document[0]["RequestItemName"],
                    Description = document[0]["Description"],
                    Category = (RequestCategory)Enum.Parse(typeof(RequestCategory), document[0]["Category"]),
                    Quantity = (int)document[0]["Quantity"],
                    PickupLocation = document[0]["PickupLocation"],
                    Status = (RequestStatus)Enum.Parse(typeof(RequestStatus), document[0]["RequestStatus"]),
                    RequestDate = DateTime.Parse(document[0]["RequestDate"])
                };

                // Return the item as a JSON response
                return Ok(receiverItemRequest);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error response
                _logger.LogError(ex, "Error fetching receiver item");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] ReceiverItemRequest receiverItemRequest)
        {
            try
            {
                // Input validation
                if (receiverItemRequest == null)
                {
                    return BadRequest("Invalid request data.");
                }

                // Your existing code to put the item in DynamoDB
                var request = new PutItemRequest
                {
                    TableName = _dynamoDBTableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "RequestId", new AttributeValue { S = Guid.NewGuid().ToString() } },
                        { "ReceiverId", new AttributeValue { S = receiverItemRequest.ReceiverId.ToString() } },
                        { "ContactNumber", new AttributeValue { S = receiverItemRequest.ContactNumber.ToString() } },
                        { "RequestItemName", new AttributeValue { S = receiverItemRequest.RequestItemName } },
                        { "Description", new AttributeValue { S = receiverItemRequest.Description} },
                        { "Category", new AttributeValue { S = receiverItemRequest.Category.ToString() } },
                        { "Quantity" , new AttributeValue {N = receiverItemRequest.Quantity.ToString() } },
                        { "PickupLocation", new AttributeValue { S = receiverItemRequest.PickupLocation.ToString() } },
                        { "RequestStatus", new AttributeValue { S = receiverItemRequest.Status.ToString() } },
                        { "RequestDate" , new AttributeValue {S = DateTime.Now.ToString() } }
                    }
                };

                await _dynamoDbClient.PutItemAsync(request);

                // Generate the URL of the newly created resource
                string requestUrl = $"/api/requests/{receiverItemRequest.RequestId}";
                //string requestUrl = Url.Action("GetReceiverItem", new { id = receiverItemRequest.RequestId }, Request.Scheme);

                return Created(requestUrl, receiverItemRequest);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error response
                _logger.LogError(ex, "Error creating request");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/products/{productId}
        [HttpPut("{requestId}")]
        public async Task<IActionResult> UpdateProduct(Guid requestId, [FromBody] ReceiverItemRequest updatedItemRequest)
        {
            try
            {
                // Check if the product with the given ID exists
                var table = Table.LoadTable(_dynamoDbClient, _dynamoDBTableName);
                var search = table.Query(new QueryFilter("RequestId", QueryOperator.Equal, requestId));

                var document = await search.GetNextSetAsync();
                if (document.Count == 0)
                {
                    return NotFound($"Request with ID {requestId} not found.");
                }

                // Update the product attributes
                var existingItemRequest = document[0];
                existingItemRequest["RequestItemName"] = updatedItemRequest.RequestItemName;
                existingItemRequest["Description"] = updatedItemRequest.Description;
                existingItemRequest["Category"] = updatedItemRequest.Category.ToString();
                existingItemRequest["Quantity"] = updatedItemRequest.Quantity;
                existingItemRequest["PickupLocation"] = updatedItemRequest.PickupLocation;
                existingItemRequest["ContactNumber"] = updatedItemRequest.ContactNumber;
                existingItemRequest["RequestStatus"] = updatedItemRequest.Status.ToString();


                // Save the updated product to DynamoDB
                await table.UpdateItemAsync(existingItemRequest);

                return Ok(existingItemRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET: api/updatestatus/{productId}/{status}
        [HttpGet("updatestatus/{requestId}/{status}/{productid}")]
        public async Task<IActionResult> UpdateProductStatus(Guid requestId, RequestStatus status, Guid? productid)
        {
            try
            {
                // Check if the product with the given ID exists
                var table = Table.LoadTable(_dynamoDbClient, _dynamoDBTableName);
                var search = table.Query(new QueryFilter("RequestId", QueryOperator.Equal, requestId));

                var document = await search.GetNextSetAsync();
                if (document.Count == 0)
                {
                    return NotFound($"Request with ID {requestId} not found.");
                }

                // Update the product attributes
                var existingItemRequest = document[0];
                existingItemRequest["RequestStatus"] = status.ToString();

                if(productid != null &&  productid != Guid.Empty)
                {
                    existingItemRequest["ProductId"] = productid.ToString();
                    if (status == RequestStatus.PendingAcceptance)
                    {
                        string updateurl = updateproductstatusurl + $"/{productid}/1";
                        _httpClient.GetAsync(updateurl).Wait();
                    }
                }

                // Save the updated product to DynamoDB
                await table.UpdateItemAsync(existingItemRequest);

                return Ok(existingItemRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpDelete("{requestId}")]
        public async Task<IActionResult> DeleteProduct(Guid requestId)
        {
            try
            {
                // Check if the product with the given ID exists
                var table = Table.LoadTable(_dynamoDbClient, _dynamoDBTableName);
                var search = table.Query(new QueryFilter("RequestId", QueryOperator.Equal, requestId));

                var document = await search.GetNextSetAsync();

                if (document.Count == 0)
                {
                    return NotFound($"Request with ID {requestId} not found.");
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

        [HttpGet("category/{categoryName}")]
        public async Task<IActionResult> FilterandSort(RequestCategory categoryName)
        {
            try
            {
                using (var client = new AmazonDynamoDBClient())
                {
                    var request = new QueryRequest
                    {
                        TableName = "RequestList",
                        IndexName = "CategorySort",
                        KeyConditionExpression = "Category = :category",
                        FilterExpression = "RequestStatus = :status",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":category", new AttributeValue { S = categoryName.ToString() }},
                        { ":status", new AttributeValue { S = RequestStatus.Created.ToString() }}
                    },
                    };

                    var response = await client.QueryAsync(request);

                    var ItemRequests = new List<ReceiverItemRequest>();

                    foreach (var item in response.Items)
                    {
                        var ItemRequest = new ReceiverItemRequest
                        {
                            RequestId = Guid.Parse(item["RequestId"].S),
                            RequestItemName = item["RequestItemName"].S,
                            Description = item["Description"].S,
                            ContactNumber = item["ContactNumber"].S,
                            RequestDate = DateTime.Parse(item["RequestDate"].S)
                        };

                        ItemRequests.Add(ItemRequest);
                    }


                    return Ok(ItemRequests);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}