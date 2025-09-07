namespace Med.Models
{
    public class SalesAnalyticsResponse
    {
        public List<ProductSales> ByProduct { get; set; }
        public List<DateSales> ByDate { get; set; }
    }
}
