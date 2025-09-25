namespace ECommerce.ViewModels
{
    public record FilterProductVM (
        string name, decimal? minPrice, decimal? maxPrice, int? categoryId, int? brandId, bool isHot
    );
}
