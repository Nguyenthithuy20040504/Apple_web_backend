namespace TaoOneBE.Models
{
    public class ProductModel
    {
        public Guid? id { get; set; }   
        public Guid? category_id { get; set; }
        public string? category_code { get; set; }
        public Guid? category_detail_id { get; set; }
        public string? category_detail_name { get; set; }
        public string? img { get; set; }
        public string? name { get; set; }
        public string? size { get; set; }
        public int price { get; set; }
        public int salePrice { get; set; }
        public string? description { get; set; }
        public string? specs { get; set; }
        public int? status { get; set; }
        public List<SubImageModel>? listImages { get; set; }
    }
}
