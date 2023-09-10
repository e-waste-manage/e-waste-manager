public class Product
{
    public int ProductId { get; set; } // Primary key

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Category { get; set; }

    // Reference to the video stored in S3
    public string? VideoUrl { get; set; }

    // Reference to the photo stored in S3
    public string? PhotoUrl { get; set; }

    // If you need to handle file uploads, you can include byte arrays for video and photo
    public byte[]? VideoFile { get; set; }
    public byte[]? PhotoFile { get; set; }
}
