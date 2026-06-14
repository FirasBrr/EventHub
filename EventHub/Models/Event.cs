using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventHub.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DateTime { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        // Map coordinates
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? FullAddress { get; set; }

        [Range(1, 10000)]
        public int Capacity { get; set; }

        public bool IsFree { get; set; }

        [Range(0, 1000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; } = "General";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation property for registrations
        public virtual ICollection<EventRegistration>? Registrations { get; set; }
    }

    public class EventRegistration
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public virtual Event? Event { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public bool IsAttended { get; set; } = false;
    }
}