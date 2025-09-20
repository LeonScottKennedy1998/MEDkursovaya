namespace MedAPI.Models
{
    public class ProductDto
    {
        public int ProductID { get; set; }
        public string NameProduct { get; set; } = "";
        public string DescriptionProduct { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string SupplierName { get; set; } = "";
    }
}
