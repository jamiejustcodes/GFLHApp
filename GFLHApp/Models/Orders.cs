namespace GFLHApp.Models
{
    /// <summary>
    /// Represents an overall customer order placed through the store,
    /// tracking fulfillment method (delivery vs collection), address details, and composite status.
    /// </summary>
    public class Orders
    {
        public int OrdersId { get; set; }
        public string UserId { get; set; }

        public DateOnly OrderDate { get; set; }

        /// <summary>
        /// Selected delivery tier (e.g. Next Day, Standard, Economy). Null if collection is chosen.
        /// </summary>
        public string? DeliveryMethod { get; set; }

        public bool Delivery { get; set; }
        public bool Collection { get; set; }

        public decimal OrdersTotal { get; set; }

        /// <summary>
        /// Tracking status for courier delivery (e.g. "Preparing Delivery", "Awaiting Confirmation", "Delivered").
        /// </summary>
        public string TrackingStatus { get; set; }

        /// <summary>
        /// Explicit agreement to store terms and conditions required at checkout.
        /// </summary>
        public bool TermsAccepted { get; set; }

        /// <summary>
        /// Scheduled collection date if collection fulfillment is selected.
        /// </summary>
        public DateOnly? DateOfCollection { get; set; }

        /// <summary>
        /// Composite status derived from individual producer slice statuses ("Pending", "Accepted", "Cancelled", "Partially Complete").
        /// </summary>
        public string OrderStatus { get; set; } = "Pending";

        /// <summary>
        /// Indicates whether the customer has confirmed receipt of the delivery.
        /// </summary>
        public bool DeliveryConfirmed { get; set; } = false;

        // Billing address fields
        public string BillingLine1 { get; set; }
        public string? BillingLine2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingPostcode { get; set; }

        // Delivery address fields (populated only when delivery is chosen and separate from billing)
        public string? DeliveryLine1 { get; set; }
        public string? DeliveryLine2 { get; set; } 
        public string? DeliveryCity { get; set; }
        public string? DeliveryPostcode { get; set; }

        // Navigation properties
        public ICollection<OrderProducts>? OrderProducts { get; set; }
    }
}

