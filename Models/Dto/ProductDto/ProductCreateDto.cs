namespace cl_be.Models.Dto.ProductDto
{
    public class ProductCreateDto
    {
        public string Name { get; set; }
        public string ProductNumber { get; set; }
        public decimal StandardCost { get; set; }
        public decimal ListPrice { get; set; }
        public int? ProductCategoryId { get; set; }
        public int? ProductModelId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal? Weight { get; set; }
        //public byte[] ThumbNailPhoto { get; set; }
        public string ThumbnailPhotoFileName { get; set; }
        public DateTime SellStartDate { get; set; }
        public DateTime? SellEndDate { get; set; }
        public DateTime? DiscontinuedDate { get; set; }
    }

}
