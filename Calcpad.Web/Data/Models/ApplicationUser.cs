using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }
        public int? CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
