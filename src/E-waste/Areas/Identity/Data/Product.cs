using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_waste.Areas.Identity.Data
{
    public enum ProductCategory
    {
        Laptop,
        Monitor,
        Accessories,
        Other

    }

    public enum ProductStatus
    {
        Reserved,
        Requested,
        Donated,
        Taken
    }
    public class Product
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid ProductId { get; set; } // Primary key

        [Column(TypeName = "DATETIME")]
        [DisplayName("Listing Date")]
        [Required]
        public DateTime ListedDate { get; set; }

        [Column(TypeName = "int")]
        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [DisplayName("User ID")]
        public Guid UserID { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        [DisplayName("Drop off address")]
        [Required]
        public string? PickupLocation { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        [DisplayName("Email")]
        [Required]
        public string? ContactNumber { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public ProductStatus? Status { get; set; } // Created, Reserved, Taken

        [Column(TypeName = "nvarchar(256)")]
        [DisplayName("Product name")]
        [Required]
        public string? Name { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        [Required]
        public string? Description { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public ProductCategory? Category { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        [DisplayName("Video Url link")]
        // Reference to the video stored in S3
        public string? VideoUrl { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        [DisplayName("Photo Url link")]
        // Reference to the photo stored in S3
        public string? PhotoUrl { get; set; }

        [Column(TypeName = "varbinary(MAX)")]
        [DisplayName("Video file")]
        // If you need to handle file uploads, you can include byte arrays for video and photo
        public byte[]? VideoFile { get; set; }

        [Column(TypeName = "varbinary(MAX)")]
        [DisplayName("Photo file")]
        public byte[]? PhotoFile { get; set; }
    }
}
