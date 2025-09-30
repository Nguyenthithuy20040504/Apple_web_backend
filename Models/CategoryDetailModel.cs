namespace TaoOneBE.Models
{
    public class CategoryDetailModel
    {
        public Guid? id { get; set; }
        public Guid category_id { get; set; }
        public string? name { get; set; }
        public int? product_count { get; set; }
        public string? size { get; set; }
    }
}
