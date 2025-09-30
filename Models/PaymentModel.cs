namespace TaoOneBE.Models
{
    public class PaymentModel : UserModel
    {
        public Guid? id { get; set; }
        public string? payment_method {get; set;}
        public string? note {get; set;}
        public int? status { get; set; }
        public int? total_bill { get; set; }
        public string? date { get; set; }
        public List<PaymentProductModel>? products { get; set; }
    }
}
