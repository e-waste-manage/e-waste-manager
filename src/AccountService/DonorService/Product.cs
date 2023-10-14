using DonorService;
using System.Text.Json.Serialization;

public class Product
{
    [JsonPropertyName("ProductId")]
    public Guid ProductId { get; set; } // Primary key

    [JsonPropertyName("ListedDate")]
    public DateTime ListedDate {get; set;}

    [JsonPropertyName("Quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("UserID")]
    public Guid UserID { get; set; }

    [JsonPropertyName("PickupLocation")]
    public string? PickupLocation { get; set; }

    [JsonPropertyName("ContactNumber")]
    public string? ContactNumber { get; set; }

    [JsonPropertyName("Status")]
    public ProductStatus? Status { get; set; } // Created, Reserved, Taken

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Category")]
    public ProductCategory? Category { get; set; }

    [JsonPropertyName("VideoUrl")]
    // Reference to the video stored in S3
    public string? VideoUrl { get; set; }

    [JsonPropertyName("PhotoUrl")]
    // Reference to the photo stored in S3
    public string? PhotoUrl { get; set; }

    // If you need to handle file uploads, you can include byte arrays for video and photo
    public byte[]? VideoFile { get; set; }

    public byte[]? PhotoFile { get; set; }
}
