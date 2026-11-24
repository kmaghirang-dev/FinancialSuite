using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Domain.Entities
{
    public class Project
    {
        [Key]
        public Guid ProjectId { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
        [MaxLength(450)]
        public string OwnerId { get; set; } = string.Empty; // UserId of the owner (IdentityUser keys are strings)
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime DateUpdated { get; set; }
        public bool IsActive { get; set; }
        public User? Owner { get; set; }
    }
}
