using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FinancialSuite.Domain.Entities
{
    public class User : IdentityUser
    {
        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(50)]
        public string Nationality { get; set; } = string.Empty;

        public List<Project>? OwnedProjects { get; set; }
    }
}
