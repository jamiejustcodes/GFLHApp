namespace GFLHApp.Models
{
    /// <summary>
    /// Represents an individual item line in a finalized customer order,
    /// linked to a specific producer's order slice and holding tax invoice information where applicable.
    /// </summary>
    public class OrderProducts
    {
        public int OrderProductsId { get; set; }
        public int OrdersId { get; set; }
        public int ProductsId { get; set; }
        public int ProductQuantity { get; set; }

        /// <summary>
        /// Auto-generated tax invoice number (generated only if the selling producer is VAT-registered).
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Foreign key to the parent producer order slice.
        /// </summary>
        public int? ProducerOrdersId { get; set; }

        // Navigation properties
        public Products Products { get; set; }
        public Orders Orders { get; set; }
        public ProducerOrders ProducerOrders { get; set; }
    }
}

