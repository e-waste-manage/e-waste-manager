using System.Text.Json.Serialization;

namespace ReceiverService.Models
{
    public class ReceiverItemRequest
    {
        [JsonPropertyName("RequestId")]
        public Guid RequestId { get; set; } // Primary key

        [JsonPropertyName("ProductId")]
        public Guid? ProductId { get; set; }

        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("ReceiverId")]
        public Guid ReceiverId { get; set; }

        [JsonPropertyName("ContactNumber")]
        public string? ContactNumber { get; set; }

        [JsonPropertyName("PickupLocation")]
        public string? PickupLocation { get; set; }

        [JsonPropertyName("RequestItemName")]
        public string? RequestItemName { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Category")]
        public RequestCategory? Category { get; set; }

        [JsonPropertyName("Status")]
        public RequestStatus? Status { get; set; } // Created, Reserved, Taken

        [JsonPropertyName("RequestDate")]
        public DateTime RequestDate { get; set; }
    }

    public enum RequestCategory
    {
        Laptop,
        Monitor,
        Accessories,
        Other
    }

    public enum RequestStatus
    {
        Created,
        PendingAssignment,
        PendingAcceptance,
        Completed
    }
}
