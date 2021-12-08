using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calcpad.web.Data.Models
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }

        [Required, StringLength(25, MinimumLength = 3)]
        public string Name { get; set; }

        [Required, StringLength(255)]
        public string Description { get; set; }

        [Required]
        public byte Months { get; set; }

        [Required, Column(TypeName = "smallmoney")]
        public Decimal Price { get; set; }
    }
}
