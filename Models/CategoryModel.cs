namespace TaoOneBE.Models
{
    public class CategoryModel
    {
        public Guid? id { set; get; }
        public string? code { set; get; }
        public string? name { set; get; }
        public string? img { set; get; }
        public int order { set; get; }
        public int status { set; get; }
        public int is_show_home { set; get; }
        public List<ProductModel>? products { set; get; } 
    }
}
