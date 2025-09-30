namespace TaoOneBE.Models
{
    public class PaymentProductModel
    {
        public Guid? payment_id {  get; set; }
        public Guid? product_id { get; set; }
        public string? product_name { get; set; }
        public string? img { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
        public int salePrice { get; set; }
        public string? size { get; set; }
    }
}