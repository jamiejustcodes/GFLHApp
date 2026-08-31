namespace GFLHApp.Models
{
    /// <summary>
    /// Represents a single producer's slice of a customer order,
    /// enabling per-producer order fulfillment, acceptance, item restocking, and cancellation.
    /// </summary>
    public class ProducerOrders
    {
        public int ProducerOrdersId { get; set; }
        public int OrdersId { get; set; }
        public string ProducerId { get; set; }
        public decimal ProducerSubtotal { get; set; }

        /// <summary>
        /// Fulfillment lifecycle status for this slice: "Pending", "Accepted", or "Cancelled".
        /// </summary>
        public string TrackingStatus { get; set; } = "Pending";

        // Navigation properties
        public Orders Orders { get; set; }
        public Producers Producers { get; set; }
        public ICollection<OrderProducts> OrderProducts { get; set; }
    }
}

