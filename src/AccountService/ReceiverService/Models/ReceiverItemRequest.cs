namespace ReceiverService.Models
{
    public class ReceiverItemRequest
    {
        public Guid RequestId { get; set; } // Primary key

        public Guid? ProductId { get; set; }    

        public int Quantity { get; set; }

        public Guid ReceiverId { get; set; }
        public string? ContactNumber { get; set; }

        public string? PickupLocation { get; set; }

        public string? RequestItemName { get; set; }

        public string? Description { get; set; }

        public RequestCategory? Category { get; set; }

        public RequestStatus? Status { get; set; } // Created, Reserved, Taken

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
