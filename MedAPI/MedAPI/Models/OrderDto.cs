namespace MedAPI.Models
{
    public class OrderDto
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
    }
}
