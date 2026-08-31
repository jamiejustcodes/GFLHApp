namespace GFLHApp.Models
{
    /// <summary>
    /// Represents an active or completed shopping cart belonging to a customer.
    /// </summary>
    public class Basket
    {
        public int BasketId { get; set; }

        public string UserId { get; set; }

        /// <summary>
        /// True indicates an active/open cart; false indicates a completed or closed cart.
        /// </summary>
        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<BasketProducts>? BasketProducts { get; set; }
    }
}

