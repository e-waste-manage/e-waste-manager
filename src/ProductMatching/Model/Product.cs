using DonorService;

public class Product
{
    public Guid ProductId { get; set; } // Primary key

    public DateTime ListedDate {get; set;}

    public int Quantity { get; set; }

    public Guid UserID { get; set; }

    public string? PickupLocation { get; set; }

    public string? ContactNumber { get; set; }

    public ProductStatus? Status { get; set; } // Created, Reserved, Taken

    public string? Name { get; set; }

    public string? Description { get; set; }

    public ProductCategory? Category { get; set; }

    // Reference to the video stored in S3
    public string? VideoUrl { get; set; }

    // Reference to the photo stored in S3
    public string? PhotoUrl { get; set; }

    // If you need to handle file uploads, you can include byte arrays for video and photo
    public byte[]? VideoFile { get; set; }

    public byte[]? PhotoFile { get; set; }
}
