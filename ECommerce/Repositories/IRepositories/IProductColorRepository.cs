using Microsoft.EntityFrameworkCore;

namespace ECommerce.Repositories.IRepositories
{
    public interface IProductColorRepository : IRepository<ProductColor>
    {
        void RemoveRange(IEnumerable<ProductColor> productColors);
    }
}
