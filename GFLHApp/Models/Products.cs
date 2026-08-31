namespace GFLHApp.Models
{
    /// <summary>
    /// Represents an inventory item sold by a specific producer.
    /// </summary>
    public class Products
    {
        public int ProductsId { get; set; }
        public int ProducersId { get; set; }

        public string ItemName { get; set; }
        public decimal ItemPrice { get; set; }
        public string? ImagePath { get; set; }
        public int QuantityInStock { get; set; }
        public bool Available { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Comma-separated allergen list (e.g. "Gluten, Dairy, Eggs") required for Natasha's Law compliance.
        /// </summary>
        public string? Allergens { get; set; }

        // Navigation properties
        public Producers Producers { get; set; }
        public ICollection<BasketProducts>? BasketProducts { get; set; }
        public ICollection<OrderProducts>? OrderProducts { get; set; }
    }
}

