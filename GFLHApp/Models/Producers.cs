namespace GFLHApp.Models
{
    /// <summary>
    /// Represents a local food supplier/farm registered in the hub.
    /// </summary>
    public class Producers
    {
        public int ProducersId { get; set; }

        /// <summary>
        /// Links the producer record to an ASP.NET Core Identity user account.
        /// </summary>
        public string UserId { get; set; }

        public string ProducerName { get; set; }
        public string ProducerEmail { get; set; }
        public string ProducerInformation { get; set; }
        public string? ImagePath { get; set; }

        /// <summary>
        /// Validated UK VAT registration number (e.g. GB123456789) if VAT registered.
        /// </summary>
        public string? VATNumber { get; set; }
        public bool IsVATRegistered { get; set; }

        // Navigation properties
        public ICollection<Products>? Products { get; set; }
        public ICollection<ProducerOrders> ProducerOrders { get; set; }
    }
}

