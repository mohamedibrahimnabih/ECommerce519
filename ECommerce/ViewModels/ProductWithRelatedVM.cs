namespace ECommerce.ViewModels
{
    public class ProductWithRelatedVM
    {
        public Product Product { get; set; } = default!;

        public List<Product> RelatedProducts { get; set; } = [];
    }
}
