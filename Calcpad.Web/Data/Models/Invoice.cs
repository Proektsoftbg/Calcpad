using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calcpad.web.Data.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public int InvoiceNumber { get; set; }

        [Required, Column(TypeName = "smallmoney")]
        public Decimal NetAmmount { get; set; }

        [Required, Column(TypeName = "smallmoney")]
        public Decimal TaxAmmount { get; set; }

        [Required, Column(TypeName = "date")]
        public DateTime CreatedDate { get; set; }

        [Required, Column(TypeName = "date")]
        public DateTime PaymentDate { get; set; }

        public bool IsCanceled { get; set; }
    }
}
