namespace TaoOneBE.Models
{
    public class NewsModel
    {
        public Guid? id { get; set; }
        public string? title {  get; set; }
        public string? slug { get; set; }
        public string? thumbnailUrl { get; set; }
        public string? coverImageUrl { get; set; }
        public string? excerpt  { get; set; }
        public string? contentHtml { get; set; }
        public string? publishedAt { get; set; }
        public string? updatedAt { get; set; }
        public int? status { get; set; }
    }
}
