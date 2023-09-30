using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.SimpleEmail;
using ReceiverService.Models;
using DonorService;
using System.Text.Json.Serialization;
using System.Text.Json;
using Amazon.SimpleEmail.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProductMatching;

public class DynamoDBStreamFunction
{
    private readonly IAmazonDynamoDB _dynamoDBClient;
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly HttpClient _httpClient;
    private readonly string matchingrequestapiurl;
    private readonly string getproducturl;
    private readonly string updaterequeststatusurl;

    public DynamoDBStreamFunction()
    {
        _dynamoDBClient = new AmazonDynamoDBClient();
        _sesClient = new AmazonSimpleEmailServiceClient();
        _httpClient = new HttpClient();
        matchingrequestapiurl = "";
        getproducturl = "";
        updaterequeststatusurl = "";
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
            string requesturl = matchingrequestapiurl + $"/{newProduct.Category}";
            HttpResponseMessage response = await _httpClient.GetAsync(matchingrequestapiurl);
            List<ReceiverItemRequest>? PotentialMatch = new List<ReceiverItemRequest>();

            if (response.IsSuccessStatusCode)
            {
                // Handle the API response here
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (apiResponse != null)
                {
                    PotentialMatch = JsonSerializer.Deserialize<List<ReceiverItemRequest>>(apiResponse);
                }
            }
            else
            {
                Console.WriteLine($"API request failed with status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions or errors here
            Console.WriteLine($"Error finding matching requests: {ex.Message}");
        }

        return matchingRequests;
    }

    private async Task SendEmailNotifications(Product newProduct, List<ReceiverItemRequest> matchingRequests)
    {
        //Implement logic to send email notifications to the product and request owners.
        // You'll need to use the _sesClient to send emails using Amazon SES or another email service.

         //Example pseudocode:
         foreach (var request in matchingRequests)
        {
            var emailSubject = "Matching Product Found";

            var productLink = $"<a href=\"{GenerateProductLink(newProduct)}\">{newProduct.Name}</a>";
            var acceptLink = $"<a href=\"{GenerateAcceptLink(request, newProduct)}\">Accept</a>";

            var emailBody = $"A matching product has been found for your request: {productLink}. " +
                            $"Please click on this link to check more details. " +
                            $"You may click the {acceptLink} button below to accept the item.";

            var emailRequest = new SendEmailRequest
            {
                Source = "your_sender_email@example.com",
                Destination = new Destination
                {
                    ToAddresses = new List<string> { request.ContactNumber }
                },
                Message = new Message
                {
                    Subject = new Content(emailSubject),
                    Body = new Body
                    {
                        Text = new Content(emailBody)
                    }
                }
            };

            await _sesClient.SendEmailAsync(emailRequest);
        }

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

    private string GenerateProductLink (Product product)
    {
        return getproducturl + $"/{product.ProductId}";
    }

    private string GenerateAcceptLink(ReceiverItemRequest request, Product product)
    {
        return updaterequeststatusurl + $"/{request.RequestId}/{RequestStatus.PendingAcceptance}/{product.ProductId}";
    }


}