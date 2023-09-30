using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.SimpleEmail;
using ReceiverService.Models;
using DonorService;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProductMatching;

public class DynamoDBStreamFunction
{
    private readonly IAmazonDynamoDB _dynamoDBClient;
    private readonly IAmazonSimpleEmailService _sesClient;
    private const string ProductRequestsTableName = "ProductRequests"; // Update with your table names

    public DynamoDBStreamFunction()
    {
        _dynamoDBClient = new AmazonDynamoDBClient();
        _sesClient = new AmazonSimpleEmailServiceClient();
    }

    public DynamoDBStreamFunction(IAmazonDynamoDB dynamoDBClient, IAmazonSimpleEmailService sesClient)
    {
        _dynamoDBClient = dynamoDBClient;
        _sesClient = sesClient;
    }

    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        foreach (var record in dynamoEvent.Records)
        {
            if (record.EventName == OperationType.INSERT) // Check if it's an insert event
            {
                var newProduct = DeserializeProductItem(record.Dynamodb.NewImage);

                // Retrieve and process product requests
                var matchingRequests = await FindMatchingRequests(newProduct);

                // Update the product and request statuses
                await UpdateStatus(newProduct, matchingRequests);

                // Send email notifications
                await SendEmailNotifications(newProduct, matchingRequests);
            }
        }
    }

    private async Task<List<ReceiverItemRequest>> FindMatchingRequests(Product newProduct)
    {
        var matchingRequests = new List<ReceiverItemRequest>();

        try
        {
            // Create a DynamoDB client
            var dynamoDBClient = new AmazonDynamoDBClient();

            // Define the DynamoDB request
            var request = new ScanRequest
            {
                TableName = "ReceiverRequests", // Replace with your actual table name
                FilterExpression = "Category = :category AND Status = :status",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":category", new AttributeValue { S = newProduct.Category.ToString() } },
                { ":status", new AttributeValue { S = "Created" } } // Adjust status as needed
            }
            };

            // Execute the scan operation
            var response = await dynamoDBClient.ScanAsync(request);

            // Process the scan results
            foreach (var item in response.Items)
            {
                var request = DeserializeReceiverRequestItem(item);
                if (IsMatchingRequest(newProduct, request))
                {
                    matchingRequests.Add(request);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions or errors here
            Console.WriteLine($"Error finding matching requests: {ex.Message}");
        }

        return matchingRequests;
    }

    private async Task UpdateStatus(Product newProduct, List<ReceiverItemRequest> matchingRequests)
    {
        // Implement logic to update the status of the new product and the matching requests in DynamoDB.
        // You'll need to use the _dynamoDBClient to perform the necessary update operations.

        // Example pseudocode:
        // foreach (var request in matchingRequests)
        // {
        //     var updateRequest = new UpdateItemRequest
        //     {
        //         TableName = ProductRequestsTableName,
        //         Key = new Dictionary<string, AttributeValue>
        //         {
        //             { "RequestId", new AttributeValue { S = request.RequestId } }
        //         },
        //         UpdateExpression = "SET Status = :status",
        //         ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        //         {
        //             { ":status", new AttributeValue { S = "UpdatedStatus" } }
        //         }
        //     };

        //     await _dynamoDBClient.UpdateItemAsync(updateRequest);
        // }

        // Similar logic can be used to update the new product's status.

        // Placeholder - replace with actual logic
    }

    private async Task SendEmailNotifications(Product newProduct, List<ReceiverItemRequest> matchingRequests)
    {
        // Implement logic to send email notifications to the product and request owners.
        // You'll need to use the _sesClient to send emails using Amazon SES or another email service.

        // Example pseudocode:
        // foreach (var request in matchingRequests)
        // {
        //     var emailSubject = "Matching Product Request Found";
        //     var emailBody = $"A matching product request has been found for your product: {newProduct.ProductName}";

        //     var emailRequest = new SendEmailRequest
        //     {
        //         Source = "your_sender_email@example.com",
        //         Destination = new Destination
        //         {
        //             ToAddresses = new List<string> { request.RequesterEmail }
        //         },
        //         Message = new Message
        //         {
        //             Subject = new Content(emailSubject),
        //             Body = new Body
        //             {
        //                 Text = new Content(emailBody)
        //             }
        //         }
        //     };

        //     await _sesClient.SendEmailAsync(emailRequest);
        // }

        // Placeholder - replace with actual logic
    }

    private Product DeserializeProductItem(Dictionary<string, AttributeValue> item)
    {
        Product product = new Product();

        if (item.TryGetValue("ProductId", out var productIdValue) && Guid.TryParse(productIdValue.S, out Guid productId))
        {
            product.ProductId = productId;
        }

        if (item.TryGetValue("Quantity", out var quantityValue) && int.TryParse(quantityValue.N, out int quantity))
        {
            product.Quantity = quantity;
        }

        if (item.TryGetValue("PickupLocation", out var pickupLocationValue))
        {
            product.PickupLocation = pickupLocationValue.S;
        }

        if (item.TryGetValue("ContactNumber", out var contactNumberValue))
        {
            product.ContactNumber = contactNumberValue.S;
        }

        if (item.TryGetValue("Category", out var categoryValue) && Enum.TryParse<ProductCategory>(categoryValue.S, out ProductCategory category))
        {
            product.Category = category;
        }

        return product;
    }

}