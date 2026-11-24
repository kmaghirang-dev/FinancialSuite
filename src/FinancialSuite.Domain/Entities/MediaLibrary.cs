using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Domain.Entities
{
    public class MediaLibrary
    {
        [Key]
        public Guid MediaId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid StatusId { get; set; }
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        public int Season { get; set; }
        public int EpisodeNo { get; set; }
        [MaxLength(1000)]
        public string Details { get; set; } = string.Empty;
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime DateUpdated { get; set; }
        public bool IsActive { get; set; }
        public DropDownDetail? Category { get; set; }
        public DropDownDetail? Status { get; set; }

    }
}
