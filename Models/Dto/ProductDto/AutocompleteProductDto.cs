namespace cl_be.Models.Dto.ProductDto
{
    public class AutocompleteProductDto
    {

        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;
        
        public decimal ListPrice { get; set; }

        public byte[]? ThumbNailPhoto { get; set; }

    }
}
