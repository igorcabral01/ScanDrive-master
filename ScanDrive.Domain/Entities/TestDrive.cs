using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ScanDrive.Domain.Entities
{
    public class TestDrive : BaseEntity
    {
        public Guid VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public Guid ShopId { get; set; }
        public Shop? Shop { get; set; }

        public string? CustomerId { get; set; }
        public IdentityUser? Customer { get; set; }

        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public DateTime PreferredDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsCancelled { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletionDate { get; set; }
        public string? CompletionNotes { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string? CancellationReason { get; set; }
    }
} 