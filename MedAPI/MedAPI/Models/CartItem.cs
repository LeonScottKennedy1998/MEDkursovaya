using System.Text.Json.Serialization;

namespace MedAPI.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string NameProduct { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        [JsonIgnore]
        public decimal Total => Price * Quantity;

    }
}
