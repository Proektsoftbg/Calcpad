using System;
using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Data.Models
{
    public class Order
    {
        public int Id { get; set; }

        public ApplicationUser User { get; set; }

        public int PlanId { get; set; }
        [Required]
        public SubscriptionPlan Plan { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public DateTime ActivatedOn { get; set; }

        public DateTime ExpiresOn { get; set; }

        public Invoice Invoice { get; set; }

    }
}
