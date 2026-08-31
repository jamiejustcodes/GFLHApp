namespace GFLHApp.Models
{
    /// <summary>
    /// Represents a line item product and its chosen quantity within a customer's basket.
    /// </summary>
    public class BasketProducts
    {
        public int BasketProductsId { get; set; }

        public int BasketId { get; set; }

        public int ProductsId { get; set; }

        public int ProductQuantity { get; set; } 

        // Navigation properties
        public Products Products { get; set; }

        public Basket Basket { get; set; }
    }
}

